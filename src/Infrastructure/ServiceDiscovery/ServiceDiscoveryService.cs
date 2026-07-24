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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Orchestrates service discovery with caching, load balancing, and self-registration lifecycle management.
/// This service acts as a policy layer that coordinates between pluggable <see cref="IServiceDiscoveryProvider"/> backends
/// and applies cross-cutting concerns like caching, health filtering, and load balancing.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ServiceDiscoveryService"/> is the high-level orchestrator that provides a consistent abstraction
/// over various <see cref="IServiceDiscoveryProvider"/> implementations (DNS, Registry, InMemory, etc.).
/// </para>
/// <para>
/// Responsibilities:
/// <list type="bullet">
///   <item>Caching resolved service instances</item>
///   <item>Applying load balancing strategies</item>
///   <item>Health status filtering</item>
///   <item>Self-registration lifecycle management</item>
///   <item>Statistics aggregation</item>
/// </list>
/// </para>
/// <para>
/// The actual backend operations (register, resolve, deregister) are delegated to the configured
/// <see cref="IServiceDiscoveryProvider"/> which is selected by <see cref="IServiceDiscoveryProviderSelector"/>.
/// </para>
/// </remarks>
public sealed class ServiceDiscoveryService : IServiceDiscoveryService
{
    private const string CacheKeyPrefix = "discovery:";

    private readonly IServiceDiscoveryProvider _provider;
    private readonly IServiceDiscoveryProviderSelector _providerSelector;
    private readonly ICacheService _cache;
    private readonly ServiceDiscoveryOptions _options;
    private readonly ILogger<ServiceDiscoveryService> _logger;

    private readonly ConcurrentDictionary<string, int> _roundRobinCounters = new();
    private readonly ConcurrentDictionary<string, ResolutionMeta> _metaCache = new();

    private Guid _selfInstanceId = Guid.NewGuid();

    /// <summary>
    /// Initialises a new <see cref="ServiceDiscoveryService"/> with provider selection and caching.
    /// </summary>
    /// <param name="providerSelector">Strategy for selecting the appropriate provider based on configuration.</param>
    /// <param name="cache">Cache service for storing resolved instances.</param>
    /// <param name="options">Service discovery configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
    public ServiceDiscoveryService(
        IServiceDiscoveryProviderSelector providerSelector,
        ICacheService cache,
        IOptions<ServiceDiscoveryOptions> options,
        ILogger<ServiceDiscoveryService> logger)
    {
        _providerSelector = providerSelector ?? throw new ArgumentNullException(nameof(providerSelector));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Get the active provider for this instance
        _provider = _providerSelector.GetProvider();
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceName"/> is null.</exception>
    public async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> DiscoverAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceName);

        if (!_options.Enabled)
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Success(Array.Empty<ServiceDiscoveryRecord>());

        var cacheKey = CacheKeyPrefix + serviceName.ToLowerInvariant();
        var cached = await _cache.GetAsync<IReadOnlyList<ServiceDiscoveryRecord>>(cacheKey);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {ServiceName} ({Count} instance(s))", serviceName, cached.Count);
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Success(cached);
        }

        // Delegate resolution to the selected provider
        var result = await _provider.ResolveAsync(serviceName, cancellationToken);

        // Cache successful results with healthy instances
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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceName"/> is null.</exception>
    public async Task<Result<ServiceDiscoveryRecord>> SelectEndpointAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceName);

        var discovery = await DiscoverAsync(serviceName, cancellationToken);
        if (!discovery.IsSuccess)
            return Result<ServiceDiscoveryRecord>.Failure(discovery.ErrorMessage!, discovery.ErrorCode);

        var alive = discovery.Value!.Where(r => r.IsAlive()).ToList();
        if (alive.Count == 0)
            return Result<ServiceDiscoveryRecord>.Failure($"No healthy instances found for service '{serviceName}'.", "NO_HEALTHY_INSTANCES");

        // Apply load balancing strategy to select a single endpoint
        return Result<ServiceDiscoveryRecord>.Success(SelectByStrategy(alive, serviceName));
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cancellationToken"/> is in cancelled state.</exception>
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

        // Use the active provider for self-registration
        var result = await _provider.RegisterAsync(record, cancellationToken);

        if (result.IsSuccess)
            _logger.LogInformation("Self-registered as {ServiceName} ({InstanceId}) via {Provider}", name, _selfInstanceId, _provider.ProviderName);
        else
            _logger.LogWarning("Self-registration failed via {Provider}: {Error}", _provider.ProviderName, result.ErrorMessage);

        return result;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cancellationToken"/> is in cancelled state.</exception>
    public async Task<Result> DeregisterSelfAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.SelfRegistration.Enabled)
            return Result.Success();

        // Use the active provider for self-deregistration
        var result = await _provider.DeregisterAsync(_selfInstanceId, cancellationToken);

        if (result.IsSuccess)
            _logger.LogInformation("Self-deregistered instance {InstanceId} via {Provider}", _selfInstanceId, _provider.ProviderName);

        return result;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cancellationToken"/> is in cancelled state.</exception>
    public async Task<Result<IReadOnlyList<string>>> GetRegisteredServicesAsync(
        CancellationToken cancellationToken = default)
    {
        // Only registry-based providers support service catalog enumeration
        if (_provider is RegistryServiceDiscoveryProvider registryProvider)
        {
            return await registryProvider.GetAllServiceNamesAsync(cancellationToken);
        }

        return Result<IReadOnlyList<string>>.Failure("Service catalog enumeration requires a registry-based provider.", "PROVIDER_UNSUPPORTED");
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cancellationToken"/> is in cancelled state.</exception>
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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cancellationToken"/> is in cancelled state.</exception>
    public async Task UpdateHeartbeatAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.SelfRegistration.Enabled)
        {
            _logger.LogDebug("Self-registration is disabled, skipping heartbeat update");
            return;
        }

        // Use the active provider to send heartbeat
        var heartbeatRecord = new ServiceDiscoveryRecord
        {
            InstanceId = _selfInstanceId,
            ServiceName = _options.SelfRegistration.ServiceName ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "dotnet-service",
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

        // Send heartbeat to the active provider
        var result = await _provider.RegisterAsync(heartbeatRecord, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogDebug("Heartbeat updated for instance {InstanceId} via {Provider}", _selfInstanceId, _provider.ProviderName);
        }
        else
        {
            _logger.LogWarning("Failed to update heartbeat for instance {InstanceId} via {Provider}: {Error}",
                _selfInstanceId, _provider.ProviderName, result.ErrorMessage);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceName"/> is null.</exception>
    public async Task<Result<ServiceDiscoveryStats>> GetServiceStatsAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceName);

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

    // ── Load Balancing Strategy Implementation ────────────────────────────────────

    private ServiceDiscoveryRecord SelectByStrategy(List<ServiceDiscoveryRecord> instances, string serviceName) =>
        _options.LoadBalancing switch
        {
            LoadBalancingStrategy.Random => instances[Random.Shared.Next(instances.Count)],
            LoadBalancingStrategy.Weighted => SelectWeighted(instances),
            LoadBalancingStrategy.Priority => instances.MinBy(r => r.Priority)!, // Safe because instances is non-empty
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

        return instances[^1]; // Fallback to last instance
    }

    private sealed record ResolutionMeta(DateTime LastResolvedAt, DateTime CacheExpiresAt, DiscoverySource Source);
}