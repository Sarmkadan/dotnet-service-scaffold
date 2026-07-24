#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Caching;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Background service that periodically checks for stale service instances and evicts them.
/// Stale instances are those that haven't sent heartbeats within the stale threshold.
/// Evicted instances are removed from the registry after the eviction threshold.
/// </summary>
public sealed class ServiceStaleEvictionBackgroundService : BackgroundService
{
    private readonly IServiceDiscoveryProvider _registryProvider;
    private readonly IServiceDiscoveryService _discoveryService;
    private readonly ICacheService _cache;
    private readonly ServiceDiscoveryOptions _options;
    private readonly ILogger<ServiceStaleEvictionBackgroundService> _logger;

    /// <summary>
    /// Initialises a new <see cref="ServiceStaleEvictionBackgroundService"/> with the required dependencies.
    /// </summary>
    /// <param name="registryProvider">The registry provider for managing service instances.</param>
    /// <param name="discoveryService">The discovery service for resolving instances.</param>
    /// <param name="cache">The cache service for cache management.</param>
    /// <param name="options">Service discovery configuration options.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public ServiceStaleEvictionBackgroundService(
        IServiceDiscoveryProvider registryProvider,
        IServiceDiscoveryService discoveryService,
        ICacheService cache,
        IOptions<ServiceDiscoveryOptions> options,
        ILogger<ServiceStaleEvictionBackgroundService> logger)
    {
        _registryProvider = registryProvider ?? throw new ArgumentNullException(nameof(registryProvider));
        _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service stale eviction background service started. Interval: {Interval}", _options.StaleEvictionInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessStaleEvictionAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error during stale eviction processing");
            }

            try
            {
                await Task.Delay(_options.StaleEvictionInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Service stale eviction background service stopped");
    }

    private async Task ProcessStaleEvictionAsync(CancellationToken cancellationToken)
    {
        if (_options.Mode is DiscoveryMode.Dns)
        {
            _logger.LogDebug("Skipping stale eviction in DNS-only mode");
            return;
        }

        var servicesResult = await _discoveryService.GetRegisteredServicesAsync(cancellationToken);
        if (!servicesResult.IsSuccess || servicesResult.Value is not { Count: > 0 } services)
        {
            _logger.LogDebug("No registered services found or failed to retrieve services");
            return;
        }

        foreach (var serviceName in services)
        {
            await ProcessServiceForStaleEvictionAsync(serviceName, cancellationToken);
        }
    }

    private async Task ProcessServiceForStaleEvictionAsync(string serviceName, CancellationToken cancellationToken)
    {
        try
        {
            var resolveResult = await _discoveryService.DiscoverAsync(serviceName, cancellationToken);
            if (!resolveResult.IsSuccess)
            {
                _logger.LogDebug("Failed to discover service {ServiceName} for stale eviction: {Error}", serviceName, resolveResult.ErrorMessage);
                return;
            }

            var instances = resolveResult.Value ?? Array.Empty<ServiceDiscoveryRecord>();
            var staleThreshold = _options.HeartbeatStaleThreshold;
            var evictionThreshold = _options.EvictionThreshold;
            var now = DateTime.UtcNow;

            var staleInstances = new List<ServiceDiscoveryRecord>();
            var evictedInstances = new List<ServiceDiscoveryRecord>();

            foreach (var instance in instances)
            {
                if (instance.IsEvicted)
                {
                    evictedInstances.Add(instance);
                    continue;
                }

                // Determine the last heartbeat time - prefer LastHeartbeatUtc, fall back to LastSeenAt
                var lastHeartbeat = instance.LastHeartbeatUtc ?? instance.LastSeenAt;

                if (instance.IsStale)
                {
                    // Check if stale instance should be evicted
                    var timeSinceStale = now - lastHeartbeat;
                    if (timeSinceStale >= evictionThreshold)
                    {
                        evictedInstances.Add(instance);
                        _logger.LogInformation(
                            "Evicting stale instance {InstanceId} for service {ServiceName} (last heartbeat: {LastHeartbeat})",
                            instance.InstanceId,
                            serviceName,
                            lastHeartbeat);
                    }
                }
                else
                {
                    // Check if instance should be marked as stale
                    var timeSinceHeartbeat = now - lastHeartbeat;
                    if (timeSinceHeartbeat >= staleThreshold)
                    {
                        staleInstances.Add(instance);
                        instance.IsStale = true;
                        _logger.LogDebug(
                            "Marking instance {InstanceId} as stale for service {ServiceName} (last heartbeat: {LastHeartbeat})",
                            instance.InstanceId,
                            serviceName,
                            lastHeartbeat);
                    }
                }
            }

            // Evict stale instances from registry
            foreach (var instance in evictedInstances)
            {
                if (instance.Source == DiscoverySource.Registry || instance.Source == DiscoverySource.LocalRegistry)
                {
                    var deregisterResult = await _registryProvider.DeregisterAsync(instance.InstanceId, cancellationToken);
                    if (deregisterResult.IsSuccess)
                    {
                        instance.IsEvicted = true;
                        _logger.LogInformation("Evicted instance {InstanceId} from registry for service {ServiceName}", instance.InstanceId, serviceName);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to evict instance {InstanceId} from registry for service {ServiceName}: {Error}",
                            instance.InstanceId,
                            serviceName,
                            deregisterResult.ErrorMessage);
                    }
                }
            }

            // Clear cache for this service to force fresh resolution
            await _cache.RemoveAsync($"discovery:{serviceName.ToLowerInvariant()}");
            _logger.LogDebug("Cache cleared for service {ServiceName} after stale eviction processing", serviceName);

            if (staleInstances.Count > 0 || evictedInstances.Count > 0)
            {
                _logger.LogInformation(
                    "Processed stale eviction for service {ServiceName}: {StaleCount} stale, {EvictedCount} evicted",
                    serviceName,
                    staleInstances.Count,
                    evictedInstances.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process stale eviction for service {ServiceName}", serviceName);
        }
    }
}