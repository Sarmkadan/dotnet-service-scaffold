#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Reflection;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Caching;
using DotnetServiceScaffold.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Orchestrates DNS-based and registry-based service discovery with caching,
/// configurable load balancing, and self-registration lifecycle management.
/// </summary>
public sealed class ServiceDiscoveryService : IServiceDiscoveryService
{
    private const string CacheKeyPrefix = "discovery:";

    private readonly DnsServiceDiscoveryProvider _dnsProvider;
    private readonly RegistryServiceDiscoveryProvider _registryProvider;
    private readonly ICacheService _cache;
    private readonly ServiceDiscoveryOptions _options;
    private readonly ILogger<ServiceDiscoveryService> _logger;

    private readonly ConcurrentDictionary<string, int> _roundRobinCounters = new();
    private readonly ConcurrentDictionary<string, ResolutionMeta> _metaCache = new();

    private Guid _selfInstanceId = Guid.NewGuid();

    /// <summary>
    /// Initialises a new <see cref="ServiceDiscoveryService"/> with both resolution providers.
    /// </summary>
    public ServiceDiscoveryService(
        DnsServiceDiscoveryProvider dnsProvider,
        RegistryServiceDiscoveryProvider registryProvider,
        ICacheService cache,
        IOptions<ServiceDiscoveryOptions> options,
        ILogger<ServiceDiscoveryService> logger)
    {
        _dnsProvider = dnsProvider;
        _registryProvider = registryProvider;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> DiscoverAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Success(Array.Empty<ServiceDiscoveryRecord>());

        var cacheKey = CacheKeyPrefix + serviceName.ToLowerInvariant();
        var cached = await _cache.GetAsync<IReadOnlyList<ServiceDiscoveryRecord>>(cacheKey);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {ServiceName} ({Count} instance(s))", serviceName, cached.Count);
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Success(cached);
        }

        var result = await ResolveFromProvidersAsync(serviceName, cancellationToken);

        if (result.IsSuccess && result.Value is { Count: > 0 } instances)
        {
            await _cache.SetAsync(cacheKey, instances, _options.CacheTtl);
            _metaCache[serviceName] = new ResolutionMeta(
                DateTime.UtcNow,
                DateTime.UtcNow.Add(_options.CacheTtl),
                instances[0].Source);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<ServiceDiscoveryRecord>> SelectEndpointAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var discovery = await DiscoverAsync(serviceName, cancellationToken);
        if (!discovery.IsSuccess)
            return Result<ServiceDiscoveryRecord>.Failure(discovery.ErrorMessage!, discovery.ErrorCode);

        var alive = discovery.Value!.Where(r => r.IsAlive()).ToList();
        if (alive.Count == 0)
            return Result<ServiceDiscoveryRecord>.Failure($"No healthy instances found for service '{serviceName}'.", "NO_HEALTHY_INSTANCES");

        return Result<ServiceDiscoveryRecord>.Success(SelectByStrategy(alive, serviceName));
    }

    /// <inheritdoc/>
    public async Task<Result> RegisterSelfAsync(CancellationToken cancellationToken = default)
    {
        var self = _options.SelfRegistration;
        if (!self.Enabled)
            return Result.Success();

        var name = self.ServiceName
            ?? Assembly.GetEntryAssembly()?.GetName().Name
            ?? "dotnet-service";

        var host = self.AdvertiseHost ?? System.Net.Dns.GetHostName();

        var record = new ServiceDiscoveryRecord
        {
            InstanceId = _selfInstanceId,
            ServiceName = name,
            Version = self.Version,
            Host = host,
            Port = self.AdvertisePort,
            Scheme = self.AdvertiseScheme,
            Tags = [.. self.Tags],
            Source = DiscoverySource.LocalRegistry,
            HealthStatus = DiscoveryHealthStatus.Passing
        };

        if (!string.IsNullOrEmpty(self.Version))
            record.Metadata["version"] = self.Version;

        var provider = PickWritableProvider();
        var result = await provider.RegisterAsync(record, cancellationToken);

        if (result.IsSuccess)
            _logger.LogInformation("Self-registered as {ServiceName} ({InstanceId}) via {Provider}", name, _selfInstanceId, provider.ProviderName);
        else
            _logger.LogWarning("Self-registration failed via {Provider}: {Error}", provider.ProviderName, result.ErrorMessage);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result> DeregisterSelfAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.SelfRegistration.Enabled)
            return Result.Success();

        var provider = PickWritableProvider();
        var result = await provider.DeregisterAsync(_selfInstanceId, cancellationToken);

        if (result.IsSuccess)
            _logger.LogInformation("Self-deregistered instance {InstanceId} via {Provider}", _selfInstanceId, provider.ProviderName);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<string>>> GetRegisteredServicesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_options.Mode is DiscoveryMode.Dns)
            return Result<IReadOnlyList<string>>.Failure("Service catalog enumeration requires Registry or Hybrid mode.", "DNS_READ_ONLY");

        return await _registryProvider.GetAllServiceNamesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RefreshAsync(string? serviceName = null, CancellationToken cancellationToken = default)
    {
        if (serviceName is not null)
        {
            await _cache.RemoveAsync(CacheKeyPrefix + serviceName.ToLowerInvariant());
            _metaCache.TryRemove(serviceName, out _);
            _logger.LogDebug("Cache invalidated for service {ServiceName}", serviceName);
            return;
        }

        await _cache.RemoveByPatternAsync(CacheKeyPrefix + "*");
        _metaCache.Clear();
        _logger.LogDebug("Discovery cache fully invalidated");
    }

    /// <inheritdoc/>
    public async Task UpdateHeartbeatAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.SelfRegistration.Enabled)
        {
            _logger.LogDebug("Self-registration is disabled, skipping heartbeat update");
            return;
        }

        var provider = PickWritableProvider();

        // Create a heartbeat record for the current instance
        var heartbeatRecord = new ServiceDiscoveryRecord
        {
            InstanceId = _selfInstanceId,
            ServiceName = _options.SelfRegistration.ServiceName ?? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "dotnet-service",
            Host = _options.SelfRegistration.AdvertiseHost ?? System.Net.Dns.GetHostName(),
            Port = _options.SelfRegistration.AdvertisePort,
            Scheme = _options.SelfRegistration.AdvertiseScheme,
            Version = _options.SelfRegistration.Version,
            Tags = [.. _options.SelfRegistration.Tags],
            Metadata = new Dictionary<string, string>(),
            Source = DiscoverySource.LocalRegistry,
            HealthStatus = DiscoveryHealthStatus.Passing,
            LastHeartbeatUtc = DateTime.UtcNow,
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(_options.SelfRegistration.Version))
            heartbeatRecord.Metadata["version"] = _options.SelfRegistration.Version;

        // Send heartbeat to registry
        var result = await provider.RegisterAsync(heartbeatRecord, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogDebug("Heartbeat updated for instance {InstanceId} via {Provider}", _selfInstanceId, provider.ProviderName);
        }
        else
        {
            _logger.LogWarning("Failed to update heartbeat for instance {InstanceId} via {Provider}: {Error}",
                _selfInstanceId, provider.ProviderName, result.ErrorMessage);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<ServiceDiscoveryStats>> GetServiceStatsAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var discovery = await DiscoverAsync(serviceName, cancellationToken);
        var records = discovery.IsSuccess ? discovery.Value! : Array.Empty<ServiceDiscoveryRecord>();

        _metaCache.TryGetValue(serviceName, out var meta);

        var stats = new ServiceDiscoveryStats(
            ServiceName: serviceName,
            TotalInstances: records.Count,
            HealthyInstances: records.Count(r => r.HealthStatus == DiscoveryHealthStatus.Passing && !r.IsStale && !r.IsEvicted),
            DegradedInstances: records.Count(r => r.HealthStatus == DiscoveryHealthStatus.Warning && !r.IsStale && !r.IsEvicted),
            CriticalInstances: records.Count(r => r.HealthStatus == DiscoveryHealthStatus.Critical && !r.IsStale && !r.IsEvicted),
            StaleInstances: records.Count(r => r.IsStale && !r.IsEvicted),
            EvictedInstances: records.Count(r => r.IsEvicted),
            LastResolvedAt: meta?.LastResolvedAt,
            CacheExpiresAt: meta?.CacheExpiresAt,
            ActiveSource: meta?.Source ?? DiscoverySource.Unknown);

        return Result<ServiceDiscoveryStats>.Success(stats);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveFromProvidersAsync(
        string serviceName,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_options.ResolutionTimeout);

        return _options.Mode switch
        {
            DiscoveryMode.Registry => await _registryProvider.ResolveAsync(serviceName, cts.Token),
            DiscoveryMode.Dns => await _dnsProvider.ResolveAsync(serviceName, cts.Token),
            DiscoveryMode.Hybrid => await ResolveHybridAsync(serviceName, cts.Token),
            _ => Result<IReadOnlyList<ServiceDiscoveryRecord>>.Failure("Unknown discovery mode.", "INVALID_MODE")
        };
    }

    private async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveHybridAsync(
        string serviceName,
        CancellationToken cancellationToken)
    {
        var registryResult = await _registryProvider.ResolveAsync(serviceName, cancellationToken);
        if (registryResult.IsSuccess && registryResult.Value!.Count > 0)
            return registryResult;

        _logger.LogDebug("Registry returned no instances for {ServiceName}; falling back to DNS", serviceName);
        return await _dnsProvider.ResolveAsync(serviceName, cancellationToken);
    }

    private ServiceDiscoveryRecord SelectByStrategy(List<ServiceDiscoveryRecord> instances, string serviceName) =>
        _options.LoadBalancing switch
        {
            LoadBalancingStrategy.Random => instances[Random.Shared.Next(instances.Count)],
            LoadBalancingStrategy.Weighted => SelectWeighted(instances),
            LoadBalancingStrategy.Priority => instances.MinBy(r => r.Priority)!,
            _ => SelectRoundRobin(instances, serviceName)
        };

    private ServiceDiscoveryRecord SelectRoundRobin(List<ServiceDiscoveryRecord> instances, string serviceName)
    {
        var counter = _roundRobinCounters.AddOrUpdate(serviceName, 0, (_, v) => v + 1);
        return instances[counter % instances.Count];
    }

    private static ServiceDiscoveryRecord SelectWeighted(List<ServiceDiscoveryRecord> instances)
    {
        int totalWeight = instances.Sum(r => r.Weight);
        int roll = Random.Shared.Next(0, totalWeight);
        int cumulative = 0;

        foreach (var instance in instances)
        {
            cumulative += instance.Weight;
            if (roll < cumulative) return instance;
        }

        return instances[^1];
    }

    private IServiceDiscoveryProvider PickWritableProvider() =>
        _options.Mode is DiscoveryMode.Dns ? _dnsProvider : _registryProvider;

    private sealed record ResolutionMeta(DateTime LastResolvedAt, DateTime CacheExpiresAt, DiscoverySource Source);
}

/// <summary>
/// Extension methods for registering the service discovery infrastructure in
/// the ASP.NET Core dependency injection container.
/// </summary>
public static class ServiceDiscoveryExtensions
{
    /// <summary>
    /// Registers all service discovery components — both DNS and registry providers,
    /// the orchestrating <see cref="IServiceDiscoveryService"/>, and a named
    /// <see cref="System.Net.Http.HttpClient"/> for registry communication.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="configuration">Application configuration used to bind <see cref="ServiceDiscoveryOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
    public static IServiceCollection AddServiceDiscovery(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServiceDiscoveryOptions>(
            configuration.GetSection(ServiceDiscoveryOptions.SectionName));

        services.AddHttpClient(RegistryServiceDiscoveryProvider.HttpClientName, (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
            client.BaseAddress = new Uri(opts.Registry.AgentEndpoint);
            client.Timeout = opts.ResolutionTimeout + TimeSpan.FromSeconds(2);
            client.DefaultRequestHeaders.Add(
                "User-Agent",
                $"dotnet-service-scaffold/{typeof(ServiceDiscoveryExtensions).Assembly.GetName().Version}");

            if (!string.IsNullOrEmpty(opts.Registry.AclToken))
                client.DefaultRequestHeaders.Add("X-Consul-Token", opts.Registry.AclToken);
        });

        services.AddSingleton<DnsServiceDiscoveryProvider>();
        services.AddSingleton<RegistryServiceDiscoveryProvider>();
        services.AddSingleton<IServiceDiscoveryService, ServiceDiscoveryService>();
    services.AddSingleton<IHostedService, ServiceHeartbeatBackgroundService>();
    services.AddSingleton<IHostedService, ServiceStaleEvictionBackgroundService>();

        return services;
    }

    /// <summary>
    /// Wires up application-lifetime hooks that self-register this service instance with the
    /// discovery backend on startup and deregister it on graceful shutdown.
    /// Only active when <see cref="SelfRegistrationOptions.Enabled"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="app">The built web application.</param>
    /// <returns>The same <see cref="WebApplication"/> for fluent chaining.</returns>
    public static WebApplication UseServiceDiscovery(this WebApplication app)
    {
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        var discovery = app.Services.GetRequiredService<IServiceDiscoveryService>();
        var logger = app.Services.GetRequiredService<ILogger<ServiceDiscoveryService>>();

        lifetime.ApplicationStarted.Register(() =>
        {
            _ = Task.Run(async () =>
            {
                var result = await discovery.RegisterSelfAsync();
                if (!result.IsSuccess)
                    logger.LogWarning("Service self-registration failed: {Error}", result.ErrorMessage);
            });
        });

        lifetime.ApplicationStopping.Register(() =>
        {
            _ = Task.Run(async () =>
            {
                await discovery.DeregisterSelfAsync();
            });
        });

        return app;
    }
}
