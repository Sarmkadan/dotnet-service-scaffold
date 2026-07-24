#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Background service that periodically sends heartbeats for the current service instance.
/// This prevents the instance from being marked as stale during eviction processing.
/// </summary>
public sealed class ServiceHeartbeatBackgroundService : BackgroundService
{
    private readonly IServiceDiscoveryService _discoveryService;
    private readonly ServiceDiscoveryOptions _options;
    private readonly ILogger<ServiceHeartbeatBackgroundService> _logger;

    /// <summary>
    /// Initialises a new <see cref="ServiceHeartbeatBackgroundService"/> with the required dependencies.
    /// </summary>
    /// <param name="discoveryService">The discovery service for updating heartbeats.</param>
    /// <param name="options">Service discovery configuration options.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public ServiceHeartbeatBackgroundService(
        IServiceDiscoveryService discoveryService,
        IOptions<ServiceDiscoveryOptions> options,
        ILogger<ServiceHeartbeatBackgroundService> logger)
    {
        _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.SelfRegistration.Enabled)
        {
            _logger.LogInformation("Self-registration is disabled, heartbeat background service will not run");
            return;
        }

        _logger.LogInformation("Service heartbeat background service started. Interval: {Interval}", _options.Registry.HeartbeatInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _discoveryService.UpdateHeartbeatAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error during heartbeat update");
            }

            try
            {
                await Task.Delay(_options.Registry.HeartbeatInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Service heartbeat background service stopped");
    }
}