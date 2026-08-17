#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Represents a resolved service instance obtained from a discovery backend.
/// Combines addressing information (host, port, scheme) with health telemetry
/// and registry metadata so that consumers can select and route to live endpoints.
/// </summary>
public sealed class ServiceDiscoveryRecord
{
    /// <summary>Gets or sets the unique identifier for this service instance.</summary>
    [Key]
    public Guid InstanceId { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the logical service name used for discovery lookups.</summary>
    [Required]
    [StringLength(200)]
    public required string ServiceName { get; set; }

    /// <summary>Gets or sets the semantic version advertised by this instance.</summary>
    [StringLength(50)]
    public string? Version { get; set; }

    /// <summary>Gets or sets the host name or IP address of this instance.</summary>
    [Required]
    [StringLength(253)]
    public required string Host { get; set; }

    /// <summary>Gets or sets the TCP port this instance is listening on.</summary>
    [Range(1, 65535)]
    public int Port { get; set; }

    /// <summary>Gets or sets the URI scheme (http, https, grpc, tcp).</summary>
    [StringLength(10)]
    public string Scheme { get; set; } = "https";

    /// <summary>
    /// Gets or sets the relative weight used for weighted load balancing.
    /// Higher values attract proportionally more traffic. Valid range: 1–100.
    /// </summary>
    [Range(1, 100)]
    public int Weight { get; set; } = 10;

    /// <summary>
    /// Gets or sets the failover priority. Lower values are preferred when the
    /// load-balancing strategy is priority-based.
    /// </summary>
    [Range(0, 65535)]
    public int Priority { get; set; }

    /// <summary>Gets or sets the current health evaluation for this instance.</summary>
    public DiscoveryHealthStatus HealthStatus { get; set; } = DiscoveryHealthStatus.Unknown;

    /// <summary>Gets or sets the resolution backend that populated this record.</summary>
    public DiscoverySource Source { get; set; } = DiscoverySource.Unknown;

    /// <summary>Gets or sets the tags associated with this instance (e.g. version, region, capability).</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets or sets arbitrary key-value metadata from the registry entry.</summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>Gets or sets when this record was first observed.</summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp of the last successful health confirmation.</summary>
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp of the last heartbeat sent by this instance.
    /// Used for TTL-based staleness detection in registry mode.
    /// </summary>
    public DateTime? LastHeartbeatUtc { get; set; }

    /// <summary>
    /// Gets or sets whether this instance is marked as stale (hasn't sent a heartbeat within the stale threshold).
    /// </summary>
    public bool IsStale { get; set; }

    /// <summary>
    /// Gets or sets whether this instance has been evicted from the registry.
    /// </summary>
    public bool IsEvicted { get; set; }

    /// <summary>
    /// Gets or sets the DNS time-to-live in seconds if this record originated from a DNS lookup.
    /// <see langword="null"/> for registry-sourced records.
    /// </summary>
    public int? DnsTtlSeconds { get; set; }

    /// <summary>Gets or sets the number of consecutive health-check failures for this instance.</summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>
    /// Builds the base endpoint URI for this instance (e.g. <c>https://api.internal:8443</c>).
    /// </summary>
    public string ToEndpointUri() => $"{Scheme}://{Host}:{Port}";

    /// <summary>
    /// Returns <see langword="true"/> when this instance is healthy and its record is not stale.
    /// </summary>
    /// <param name="staleThreshold">
    /// Maximum time since <see cref="LastSeenAt"/> before a record is considered stale.
    /// Defaults to 5 minutes when omitted.
    /// </param>
    public bool IsAlive(TimeSpan? staleThreshold = null)
    {
        var threshold = staleThreshold ?? TimeSpan.FromMinutes(5);

        // Check if instance is evicted or explicitly marked as stale
        if (IsEvicted || IsStale)
            return false;

        // Check health status
        if (HealthStatus is DiscoveryHealthStatus.Critical)
            return false;

        // Check time since last seen
        if ((DateTime.UtcNow - LastSeenAt) >= threshold)
            return false;

        return true;
    }

    /// <summary>
    /// Records a successful health probe, resetting the consecutive failure counter
    /// and promoting status to <see cref="DiscoveryHealthStatus.Passing"/>.
    /// </summary>
    public void RecordHealthy()
    {
        HealthStatus = DiscoveryHealthStatus.Passing;
        ConsecutiveFailures = 0;
        LastSeenAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a failed health probe. Status escalates to
    /// <see cref="DiscoveryHealthStatus.Critical"/> once <paramref name="criticalThreshold"/>
    /// consecutive failures are reached.
    /// </summary>
    /// <param name="criticalThreshold">Failure count that triggers the critical escalation.</param>
    public void RecordUnhealthy(int criticalThreshold = 3)
    {
        ConsecutiveFailures++;
        HealthStatus = ConsecutiveFailures >= criticalThreshold
            ? DiscoveryHealthStatus.Critical
            : DiscoveryHealthStatus.Warning;
    }
}

/// <summary>Health status levels for a discovered service instance.</summary>
public enum DiscoveryHealthStatus
{
    /// <summary>Health has not yet been evaluated.</summary>
    Unknown = 0,
    /// <summary>The instance is healthy and accepting traffic.</summary>
    Passing = 1,
    /// <summary>The instance is degraded but may still serve some requests.</summary>
    Warning = 2,
    /// <summary>The instance is unhealthy and must not receive traffic.</summary>
    Critical = 3
}

/// <summary>Identifies which resolution backend populated a <see cref="ServiceDiscoveryRecord"/>.</summary>
public enum DiscoverySource
{
    /// <summary>Origin is unknown or unspecified.</summary>
    Unknown = 0,
    /// <summary>Resolved from DNS SRV or A/AAAA records.</summary>
    Dns = 1,
    /// <summary>Retrieved from an HTTP service registry (Consul-compatible API).</summary>
    Registry = 2,
    /// <summary>Registered directly within the current process.</summary>
    LocalRegistry = 3
}
