#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Represents the operational status of the service mesh integration layer.
/// </summary>
public enum ServiceMeshStatus
{
    /// <summary>Status cannot be determined — proxy has not been contacted yet.</summary>
    Unknown,

    /// <summary>Sidecar proxy is starting and not yet ready to forward traffic.</summary>
    Initializing,

    /// <summary>Proxy is healthy and forwarding traffic normally.</summary>
    Ready,

    /// <summary>Proxy is reachable but reporting one or more degraded upstream clusters.</summary>
    Degraded,

    /// <summary>Proxy is unreachable or the admin API is unresponsive.</summary>
    Disconnected
}

/// <summary>
/// Represents a single upstream cluster tracked by the sidecar proxy.
/// </summary>
public class UpstreamCluster
{
    /// <summary>Logical name of the cluster as registered in the service mesh.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Active load-balancing endpoint resolved for this cluster.</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Number of hosts currently passing health checks.</summary>
    public int HealthyHosts { get; set; }

    /// <summary>Total number of hosts in the cluster, including unhealthy ones.</summary>
    public int TotalHosts { get; set; }

    /// <summary>
    /// Indicates whether the circuit breaker has effectively tripped for this cluster.
    /// True when the cluster has hosts but none are healthy.
    /// </summary>
    public bool CircuitBreakerOpen { get; set; }

    /// <summary>
    /// Computes the ratio of healthy hosts as a percentage.
    /// Returns 100 when the cluster has no configured hosts.
    /// </summary>
    public decimal GetHealthPercent() =>
        TotalHosts == 0 ? 100m : (decimal)HealthyHosts / TotalHosts * 100;
}

/// <summary>
/// Snapshot of the sidecar proxy state used for observability and routing decisions.
/// Populated by querying the proxy admin API and is not persisted.
/// </summary>
public class SidecarProxyInfo
{
    /// <summary>Unique identifier for this proxy instance, typically the pod or host name.</summary>
    public string ProxyId { get; set; } = string.Empty;

    /// <summary>Name of the service mesh environment (e.g., "istio", "linkerd", "consul-connect").</summary>
    public string MeshName { get; set; } = string.Empty;

    /// <summary>Version string reported by the proxy admin API.</summary>
    public string ProxyVersion { get; set; } = string.Empty;

    /// <summary>Overall operational status of the proxy at the time of the snapshot.</summary>
    public ServiceMeshStatus Status { get; set; } = ServiceMeshStatus.Unknown;

    /// <summary>Base URL of the proxy admin API that was queried to build this snapshot.</summary>
    public string AdminEndpoint { get; set; } = string.Empty;

    /// <summary>Key/value labels attached to this proxy (e.g., workload identity metadata).</summary>
    public Dictionary<string, string> Labels { get; set; } = new();

    /// <summary>Upstream clusters known to the proxy at the time of the snapshot.</summary>
    public List<UpstreamCluster> UpstreamClusters { get; set; } = new();

    /// <summary>UTC timestamp when this snapshot was captured.</summary>
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Returns true when the proxy is fully operational and ready to forward traffic.
    /// </summary>
    public bool IsHealthy() => Status == ServiceMeshStatus.Ready;

    /// <summary>
    /// Returns the subset of upstream clusters that have the circuit breaker tripped.
    /// </summary>
    public IEnumerable<UpstreamCluster> GetOpenCircuits() =>
        UpstreamClusters.Where(c => c.CircuitBreakerOpen);
}
