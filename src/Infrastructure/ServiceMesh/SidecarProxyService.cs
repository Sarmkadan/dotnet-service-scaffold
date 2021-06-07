#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetServiceScaffold.Domain.Models;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceMesh;

/// <summary>
/// Contract for interacting with the local sidecar proxy admin API.
/// Implementations communicate with Envoy-compatible proxies such as those
/// injected by Istio, Consul Connect, or Linkerd.
/// </summary>
public interface ISidecarProxyService
{
    /// <summary>
    /// Returns a snapshot of the sidecar proxy state including version, status,
    /// and the full set of known upstream clusters.
    /// </summary>
    Task<SidecarProxyInfo> GetProxyInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the sidecar proxy has completed initialization and is ready
    /// to forward both inbound and outbound traffic.
    /// </summary>
    Task<bool> CheckReadinessAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current set of upstream clusters visible to the proxy.
    /// </summary>
    Task<IReadOnlyList<UpstreamCluster>> GetUpstreamClustersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Signals the proxy to begin draining existing connections before a shutdown.
    /// Awaits for the specified drain period before returning.
    /// </summary>
    Task DrainConnectionsAsync(int drainSeconds = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects whether the application is running inside a service mesh by attempting
    /// to contact the sidecar admin API. Returns false immediately when the integration
    /// is disabled via configuration.
    /// </summary>
    Task<bool> IsServiceMeshEnabledAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Communicates with the sidecar proxy admin API (Envoy-compatible) to expose
/// service mesh state for health reporting, graceful shutdown, and observability.
/// </summary>
public class SidecarProxyService : ISidecarProxyService
{
    private readonly HttpClient _httpClient;
    private readonly ServiceMeshOptions _options;
    private readonly ILogger<SidecarProxyService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance with a named HTTP client, options, and logger.
    /// </summary>
    public SidecarProxyService(
        HttpClient httpClient,
        IOptions<ServiceMeshOptions> options,
        ILogger<SidecarProxyService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SidecarProxyInfo> GetProxyInfoAsync(CancellationToken cancellationToken = default)
    {
        var info = new SidecarProxyInfo
        {
            AdminEndpoint = _options.AdminEndpoint,
            MeshName = _options.MeshName,
            LastChecked = DateTime.UtcNow
        };

        try
        {
            var response = await _httpClient.GetAsync($"{_options.AdminEndpoint}/server_info", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var serverInfo = JsonSerializer.Deserialize<EnvoyServerInfo>(content, JsonOptions);

                info.ProxyVersion = serverInfo?.Version ?? "unknown";
                info.ProxyId = serverInfo?.Node?.Id ?? Environment.MachineName;
                info.Status = serverInfo?.State == "LIVE" ? ServiceMeshStatus.Ready : ServiceMeshStatus.Initializing;
            }
            else
            {
                info.Status = ServiceMeshStatus.Degraded;
            }

            info.UpstreamClusters = (await GetUpstreamClustersAsync(cancellationToken)).ToList();

            if (info.UpstreamClusters.Any(c => c.CircuitBreakerOpen) && info.Status == ServiceMeshStatus.Ready)
                info.Status = ServiceMeshStatus.Degraded;

            _logger.LogDebug("Sidecar proxy info retrieved: status={Status}, clusters={ClusterCount}",
                info.Status, info.UpstreamClusters.Count);
        }
        catch (HttpRequestException ex)
        {
            info.Status = ServiceMeshStatus.Disconnected;
            _logger.LogWarning(ex, "Unable to reach sidecar proxy admin API at {AdminEndpoint}", _options.AdminEndpoint);
        }
        catch (OperationCanceledException)
        {
            info.Status = ServiceMeshStatus.Disconnected;
            _logger.LogWarning("Sidecar proxy admin API request was cancelled");
        }

        return info;
    }

    /// <inheritdoc />
    public async Task<bool> CheckReadinessAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.ReadinessTimeoutSeconds));

            var response = await _httpClient.GetAsync($"{_options.AdminEndpoint}/ready", cts.Token);
            _logger.LogDebug("Sidecar proxy readiness check: {IsReady}", response.IsSuccessStatusCode);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Sidecar proxy readiness check failed");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UpstreamCluster>> GetUpstreamClustersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.AdminEndpoint}/clusters?format=json", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve upstream clusters: HTTP {StatusCode}", response.StatusCode);
                return Array.Empty<UpstreamCluster>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var clusters = JsonSerializer.Deserialize<EnvoyClustersResponse>(content, JsonOptions);

            if (clusters?.ClusterStatuses is null)
                return Array.Empty<UpstreamCluster>();

            return clusters.ClusterStatuses
                .Select(cs =>
                {
                    var total = cs.HostStatuses?.Count ?? 0;
                    var healthy = cs.HostStatuses?.Count(h => h.HealthStatus?.EdsHealthStatus == "HEALTHY") ?? 0;
                    return new UpstreamCluster
                    {
                        Name = cs.Name ?? string.Empty,
                        Endpoint = cs.HostStatuses?.FirstOrDefault()?.Address?.SocketAddress?.Address ?? string.Empty,
                        HealthyHosts = healthy,
                        TotalHosts = total,
                        CircuitBreakerOpen = total > 0 && healthy == 0
                    };
                })
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            _logger.LogError(ex, "Error parsing upstream cluster response from sidecar proxy");
            return Array.Empty<UpstreamCluster>();
        }
    }

    /// <inheritdoc />
    public async Task DrainConnectionsAsync(int drainSeconds = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initiating sidecar proxy connection drain for {DrainSeconds}s", drainSeconds);

        try
        {
            await _httpClient.PostAsync(
                $"{_options.AdminEndpoint}/drain_listeners?inboundonly&graceful",
                content: null, cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(drainSeconds), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Connection drain interrupted before the {DrainSeconds}s window elapsed", drainSeconds);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to signal connection drain to sidecar proxy");
        }

        _logger.LogInformation("Sidecar proxy connection drain completed");
    }

    /// <inheritdoc />
    public async Task<bool> IsServiceMeshEnabledAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return false;

        return await CheckReadinessAsync(cancellationToken);
    }

    // Minimal Envoy admin API response shapes — only the fields required for this integration.
    private sealed class EnvoyServerInfo
    {
        public string? Version { get; set; }
        public string? State { get; set; }
        public EnvoyNode? Node { get; set; }
    }

    private sealed class EnvoyNode
    {
        public string? Id { get; set; }
        public string? Cluster { get; set; }
    }

    private sealed class EnvoyClustersResponse
    {
        public List<EnvoyClusterStatus>? ClusterStatuses { get; set; }
    }

    private sealed class EnvoyClusterStatus
    {
        public string? Name { get; set; }
        public List<EnvoyHostStatus>? HostStatuses { get; set; }
    }

    private sealed class EnvoyHostStatus
    {
        public EnvoyAddress? Address { get; set; }
        public EnvoyHealthStatus? HealthStatus { get; set; }
    }

    private sealed class EnvoyAddress
    {
        public EnvoySocketAddress? SocketAddress { get; set; }
    }

    private sealed class EnvoySocketAddress
    {
        public string? Address { get; set; }
        public int PortValue { get; set; }
    }

    private sealed class EnvoyHealthStatus
    {
        public string? EdsHealthStatus { get; set; }
    }
}
