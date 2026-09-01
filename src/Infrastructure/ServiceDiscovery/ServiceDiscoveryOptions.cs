#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Root configuration for the service discovery subsystem.
/// Bind from the <c>"ServiceDiscovery"</c> section in <c>appsettings.json</c>.
/// </summary>
public sealed class ServiceDiscoveryOptions : IServiceDiscoveryOptions, IEquatable<ServiceDiscoveryOptions>
{
    /// <summary>The <c>appsettings.json</c> section key used for configuration binding.</summary>
    public const string SectionName = "ServiceDiscovery";

    /// <summary>Gets or sets whether the service discovery subsystem is active.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets the resolution strategy the discovery engine uses.</summary>
    public DiscoveryMode Mode { get; set; } = DiscoveryMode.Dns;

    /// <summary>Gets or sets the load-balancing algorithm applied when selecting from healthy instances.</summary>
    public LoadBalancingStrategy LoadBalancing { get; set; } = LoadBalancingStrategy.RoundRobin;

    /// <summary>Gets or sets how long resolved records are cached before the backend is re-queried.</summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Gets or sets the background poll interval used when watching for instance changes.</summary>
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Gets or sets the heartbeat TTL that determines when a service instance is considered stale.
        /// Instances that haven't sent a heartbeat within this period are marked as stale.
        /// </summary>
        public TimeSpan HeartbeatStaleThreshold { get; set; } = TimeSpan.FromMinutes(3);

        /// <summary>
        /// Gets or sets the eviction TTL that determines when a stale service instance is removed.
        /// Stale instances are evicted after this additional period of inactivity.
        /// </summary>
        public TimeSpan EvictionThreshold { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the interval at which the stale eviction background service runs.
        /// </summary>
        public TimeSpan StaleEvictionInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Gets or sets the per-call timeout applied to individual resolution requests.</summary>
    public TimeSpan ResolutionTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Gets or sets DNS-specific resolution settings.</summary>
    public DnsDiscoveryOptions Dns { get; set; } = new();

    /// <summary>Gets or sets HTTP registry discovery settings.</summary>
    public RegistryDiscoveryOptions Registry { get; set; } = new();

    /// <summary>Gets or sets self-registration settings for the current service instance.</summary>
    public SelfRegistrationOptions SelfRegistration { get; set; } = new();

    public string SearchDomain { get => Dns.SearchDomain; set => Dns.SearchDomain = value; }
    public bool PreferSrvRecords { get => Dns.PreferSrvRecords; set => Dns.PreferSrvRecords = value; }
    public string DnsServerAddress { get => Dns.DnsServerAddress; set => Dns.DnsServerAddress = value; }
    public int DnsServerPort { get => Dns.DnsServerPort; set => Dns.DnsServerPort = value; }
    public int DefaultPort { get => Dns.DefaultPort; set => Dns.DefaultPort = value; }
    public string DefaultScheme { get => Dns.DefaultScheme; set => Dns.DefaultScheme = value; }
    public int MaxRetries { get => Dns.MaxRetries; set => Dns.MaxRetries = value; }
    public TimeSpan SocketTimeout { get => Dns.SocketTimeout; set => Dns.SocketTimeout = value; }
    public string AgentEndpoint { get => Registry.AgentEndpoint; set => Registry.AgentEndpoint = value; }
    public string? AclToken { get => Registry.AclToken; set => Registry.AclToken = value; }
    public bool OnlyHealthyInstances { get => Registry.OnlyHealthyInstances; set => Registry.OnlyHealthyInstances = value; }

    public bool Equals(ServiceDiscoveryOptions? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Enabled == other.Enabled
            && Mode == other.Mode
            && LoadBalancing == other.LoadBalancing
            && CacheTtl == other.CacheTtl
            && RefreshInterval == other.RefreshInterval
            && ResolutionTimeout == other.ResolutionTimeout
            && Dns.Equals(other.Dns)
            && Registry.Equals(other.Registry);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ServiceDiscoveryOptions)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Enabled, Mode, LoadBalancing, CacheTtl, RefreshInterval, ResolutionTimeout, Dns, Registry);
    }

    public static bool operator ==(ServiceDiscoveryOptions? left, ServiceDiscoveryOptions? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(ServiceDiscoveryOptions? left, ServiceDiscoveryOptions? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"ServiceDiscoveryOptions {{ Enabled = {Enabled}, Mode = {Mode}, LoadBalancing = {LoadBalancing}, CacheTtl = {CacheTtl}, RefreshInterval = {RefreshInterval}, ResolutionTimeout = {ResolutionTimeout} }}";
    }
}

/// <summary>DNS-specific settings for service-instance resolution.</summary>
public sealed class DnsDiscoveryOptions
{
    /// <summary>
    /// Gets or sets the DNS search domain appended to bare service names.
    /// For Kubernetes clusters this is typically <c>svc.cluster.local</c>.
    /// </summary>
    public string SearchDomain { get; set; } = "svc.cluster.local";

    /// <summary>Gets or sets whether SRV records are queried before falling back to A/AAAA.</summary>
    public bool PreferSrvRecords { get; set; } = true;

    /// <summary>
    /// Gets or sets the IP address of the DNS server used for SRV queries.
    /// Defaults to <c>127.0.0.53</c> (systemd-resolved stub listener).
    /// </summary>
    public string DnsServerAddress { get; set; } = "127.0.0.53";

    /// <summary>Gets or sets the DNS server port (standard is 53).</summary>
    public int DnsServerPort { get; set; } = 53;

    /// <summary>
    /// Gets or sets the default port assigned to A-record endpoints when no SRV record
    /// supplies an explicit port number.
    /// </summary>
    public int DefaultPort { get; set; } = 443;

    /// <summary>Gets or sets the URI scheme assumed for A-record-derived endpoints.</summary>
    public string DefaultScheme { get; set; } = "https";

    /// <summary>Gets or sets the maximum number of UDP retry attempts on transient DNS failures.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Gets or sets the UDP socket receive timeout applied per attempt.</summary>
    public TimeSpan SocketTimeout { get; set; } = TimeSpan.FromSeconds(2);
}

/// <summary>HTTP service registry (Consul-compatible API) discovery settings.</summary>
public sealed class RegistryDiscoveryOptions
{
    /// <summary>Gets or sets the base URL of the registry agent (e.g. <c>http://localhost:8500</c>).</summary>
    public string AgentEndpoint { get; set; } = "http://localhost:8500";

    /// <summary>Gets or sets the ACL token sent in the <c>X-Consul-Token</c> header.</summary>
    public string? AclToken { get; set; }

    /// <summary>Gets or sets whether only instances with passing health checks are returned.</summary>
    public bool OnlyHealthyInstances { get; set; } = true;

    /// <summary>Gets or sets the target datacenter. Uses the agent default when <see langword="null"/>.</summary>
    public string? Datacenter { get; set; }

    /// <summary>Gets or sets how often TTL heartbeats are sent for self-registered service entries.</summary>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(10);
}

/// <summary>Controls automatic self-registration of the current service instance on startup.</summary>
public sealed class SelfRegistrationOptions
{
    /// <summary>Gets or sets whether self-registration runs when the application starts.</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the logical service name to register under.
    /// Defaults to the entry assembly name when <see langword="null"/>.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>Gets or sets the version tag advertised in the registry entry.</summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the host name or IP address to advertise.
    /// Uses <see cref="System.Net.Dns.GetHostName()"/> when <see langword="null"/>.
    /// </summary>
    public string? AdvertiseHost { get; set; }

    /// <summary>Gets or sets the port to advertise during registration.</summary>
    public int AdvertisePort { get; set; } = 443;

    /// <summary>Gets or sets the URI scheme advertised in the registration entry.</summary>
    public string AdvertiseScheme { get; set; } = "https";

    /// <summary>Gets or sets the HTTP health check path used for registry-driven HTTP probes.</summary>
    public string HealthCheckPath { get; set; } = "/health";

    /// <summary>Gets or sets arbitrary tags to attach to the registration entry.</summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>Specifies which resolution strategy the discovery engine employs.</summary>
public enum DiscoveryMode
{
    /// <summary>Resolve service instances exclusively via DNS SRV/A records.</summary>
    Dns = 0,
    /// <summary>Resolve service instances exclusively via the HTTP service registry.</summary>
    Registry = 1,
    /// <summary>Attempt the registry first; fall back to DNS on failure or empty results.</summary>
    Hybrid = 2
}

/// <summary>Load-balancing algorithm used when selecting a single endpoint from multiple instances.</summary>
public enum LoadBalancingStrategy
{
    /// <summary>Distributes requests sequentially across all healthy instances.</summary>
    RoundRobin = 0,
    /// <summary>Selects a uniformly random healthy instance per request.</summary>
    Random = 1,
    /// <summary>Routes traffic proportionally to each instance's <c>Weight</c> value.</summary>
    Weighted = 2,
    /// <summary>Always selects the instance with the lowest <c>Priority</c> value; ties broken by index order.</summary>
    Priority = 3
}
