# ServiceDiscoveryOptions

Configures all aspects of service discovery within the `dotnet-service-scaffold` framework. This class centralizes settings for enabling or disabling discovery, choosing the resolution mechanism (DNS, registry, or agent), tuning cache and refresh intervals, and controlling how discovered endpoints are selected and optionally self-registered. An instance of `ServiceDiscoveryOptions` is typically bound from application configuration and injected into the discovery pipeline.

## API

### `public bool Enabled`
Master switch for the service discovery subsystem. When `false`, all discovery logic is bypassed, and the application must rely on statically configured endpoints. Defaults to `false`.

### `public DiscoveryMode Mode`
Selects the discovery backend.
- `DiscoveryMode.Dns` – resolves services via DNS queries (A, SRV, or custom records).
- `DiscoveryMode.Registry` – queries an external service registry.
- `DiscoveryMode.Agent` – delegates resolution to a local agent process.

The chosen mode determines which nested options block (`Dns`, `Registry`, or `AgentEndpoint`) is consulted.

### `public LoadBalancingStrategy LoadBalancing`
Specifies how one endpoint is chosen when multiple healthy instances are returned.
- `LoadBalancingStrategy.RoundRobin` – cycles through instances sequentially.
- `LoadBalancingStrategy.Random` – picks an instance at random.
- `LoadBalancingStrategy.LeastConnections` – prefers the instance with the fewest active connections (requires connection tracking).
- `LoadBalancingStrategy.PowerOfTwoChoices` – selects two instances at random and returns the one with lower latency or load.

Defaults to `RoundRobin`.

### `public TimeSpan CacheTtl`
Duration for which a successfully resolved set of endpoints is considered fresh. Subsequent requests within this window return cached results without re-querying the backend. Set to `TimeSpan.Zero` to disable caching. Defaults to 30 seconds.

### `public TimeSpan RefreshInterval`
Interval at which a background refresh proactively updates the cache, independent of incoming requests. Must be less than or equal to `CacheTtl` when both are positive. Defaults to 25 seconds.

### `public TimeSpan ResolutionTimeout`
Maximum time allowed for a single discovery query (DNS lookup, registry HTTP call, or agent request) before it is aborted and considered failed. Defaults to 5 seconds.

### `public DnsDiscoveryOptions Dns`
Nested options used only when `Mode` is `DiscoveryMode.Dns`. Contains settings such as query type, name servers, and retry policies for DNS-based resolution.

### `public RegistryDiscoveryOptions Registry`
Nested options used only when `Mode` is `DiscoveryMode.Registry`. Contains the registry base URI, authentication, pagination, and health-filtering parameters.

### `public SelfRegistrationOptions SelfRegistration`
Controls whether the current application instance registers itself with the discovery backend. Includes registration TTL, heartbeat interval, and metadata tags. Ignored when `Enabled` is `false` or when the chosen `Mode` does not support self-registration.

### `public string SearchDomain`
DNS search domain appended to unqualified service names during DNS resolution. For example, setting this to `"svc.cluster.local"` transforms a query for `"orders"` into `"orders.svc.cluster.local"`. Only relevant for `DiscoveryMode.Dns`.

### `public bool PreferSrvRecords`
When `true` and `Mode` is `DiscoveryMode.Dns`, the resolver attempts SRV lookups first and falls back to A/AAAA records only if no SRV records exist. When `false`, A/AAAA resolution is used directly. Defaults to `true`.

### `public string DnsServerAddress`
IP address or hostname of an explicit DNS server to query. When `null` or empty, the system-configured resolver is used. Only relevant for `DiscoveryMode.Dns`.

### `public int DnsServerPort`
Port number for the explicit DNS server specified in `DnsServerAddress`. Defaults to 53.

### `public int DefaultPort`
Fallback port assigned to a resolved endpoint when the discovery response does not include a port (e.g., an A record without an accompanying SRV record). Defaults to 80.

### `public string DefaultScheme`
URI scheme (`"http"` or `"https"`) applied when the discovery response lacks scheme information. Defaults to `"http"`.

### `public int MaxRetries`
Maximum number of retry attempts for a failed discovery query before an exception is surfaced to the caller. Each retry is subject to `ResolutionTimeout`. Defaults to 3.

### `public TimeSpan SocketTimeout`
Low-level socket connect and read timeout applied during discovery queries that involve network calls (HTTP to registry, TCP to agent, or UDP to DNS). Distinct from `ResolutionTimeout`, which governs the overall operation. Defaults to 2 seconds.

### `public string AgentEndpoint`
URI of the local discovery agent (e.g., `"http://localhost:8500"`). Required when `Mode` is `DiscoveryMode.Agent`; ignored otherwise.

### `public string? AclToken`
Access control token injected into registry or agent requests when the backend requires authentication. `null` means no token is sent. Only relevant for `DiscoveryMode.Registry` and `DiscoveryMode.Agent`.

### `public bool OnlyHealthyInstances`
When `true`, endpoints marked as unhealthy by the discovery backend are excluded from the returned set. When `false`, all instances are returned regardless of health status. Defaults to `true`.

## Usage

### Example 1: DNS-based discovery with SRV preference and caching
```csharp
var options = new ServiceDiscoveryOptions
{
    Enabled = true,
    Mode = DiscoveryMode.Dns,
    SearchDomain = "services.consul",
    PreferSrvRecords = true,
    DefaultPort = 443,
    DefaultScheme = "https",
    CacheTtl = TimeSpan.FromSeconds(60),
    RefreshInterval = TimeSpan.FromSeconds(50),
    ResolutionTimeout = TimeSpan.FromSeconds(3),
    MaxRetries = 2,
    LoadBalancing = LoadBalancingStrategy.RoundRobin,
    OnlyHealthyInstances = true
};

// Bind to a service collection
services.Configure<ServiceDiscoveryOptions>(configuration.GetSection("Discovery"));
```

### Example 2: Registry-based discovery with self-registration and ACL token
```csharp
var options = new ServiceDiscoveryOptions
{
    Enabled = true,
    Mode = DiscoveryMode.Registry,
    Registry = new RegistryDiscoveryOptions
    {
        BaseUri = new Uri("https://registry.internal.example.com"),
        PollingInterval = TimeSpan.FromSeconds(15)
    },
    SelfRegistration = new SelfRegistrationOptions
    {
        Enabled = true,
        HeartbeatInterval = TimeSpan.FromSeconds(10),
        Ttl = TimeSpan.FromSeconds(30),
        Tags = new[] { "region=us-east-1", "version=2.4.0" }
    },
    AclToken = Environment.GetEnvironmentVariable("REGISTRY_ACL_TOKEN"),
    OnlyHealthyInstances = true,
    LoadBalancing = LoadBalancingStrategy.LeastConnections,
    CacheTtl = TimeSpan.FromSeconds(10),
    ResolutionTimeout = TimeSpan.FromSeconds(5),
    MaxRetries = 3,
    SocketTimeout = TimeSpan.FromSeconds(2)
};

services.Configure<ServiceDiscoveryOptions>(configuration.GetSection("Discovery"));
```

## Notes

- **Mode-dependent validation**: Options blocks `Dns`, `Registry`, and `AgentEndpoint` are only validated when their corresponding `Mode` is active. Setting `Dns` options while `Mode` is `Registry` has no effect and does not cause errors.
- **Caching and staleness**: When `CacheTtl` is `TimeSpan.Zero`, every resolution request triggers a live query. If `RefreshInterval` exceeds `CacheTtl`, the background refresh will never execute before the cache expires, effectively disabling proactive refresh.
- **Timeout ordering**: `SocketTimeout` applies to individual network operations within a single attempt. `ResolutionTimeout` encompasses all retries and internal logic for one resolution call. Setting `SocketTimeout` greater than `ResolutionTimeout` causes the socket timeout to be clamped to the resolution timeout at runtime.
- **Self-registration prerequisites**: `SelfRegistration.Enabled` is silently ignored if the global `Enabled` flag is `false` or if the selected `Mode` does not implement the registration interface.
- **Thread safety**: `ServiceDiscoveryOptions` is a plain options object. Its properties are not synchronized. After initial binding at startup, instances should be treated as immutable. Changing values on a live instance while discovery operations are in flight leads to unpredictable behavior and is not supported.
- **`AclToken` security**: The token is held in memory as a plain string. Avoid binding it directly from hardcoded configuration files; prefer environment variables or secure configuration providers.
- **`OnlyHealthyInstances` and load balancers**: When set to `false`, load-balancing strategies that rely on health metadata (e.g., `LeastConnections`) may route traffic to degraded instances. Ensure this combination is intentional.
