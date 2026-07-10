# ServiceDiscoveryRecord

Represents a registered service instance within the service discovery infrastructure. It holds the identity, network location, health status, and operational metadata required for routing, load balancing, and lifecycle management. Instances are typically populated by discovery sources and consumed by resolvers or load balancers.

## API

### InstanceId
`public Guid InstanceId`

Unique identifier for this service instance. Remains stable across health status transitions and metadata updates for the same logical instance.

### ServiceName
`public required string ServiceName`

Logical name of the service this instance belongs to. Must be supplied at creation; used to group instances under a common service identity.

### Version
`public string? Version`

Optional version string for the service instance. May be used for version-aware routing or canary deployments. Null when not specified by the discovery source.

### Host
`public required string Host`

Hostname or IP address where the instance is reachable. Required at creation. Combined with `Port` and `Scheme` to form endpoint URIs.

### Port
`public int Port`

Network port number the instance listens on. Defaults to `0` if not explicitly set, though a valid port is expected for reachable instances.

### Scheme
`public string Scheme`

URI scheme used to communicate with the instance (e.g., `"http"`, `"https"`, `"grpc"`). Defaults to `"http"` when not specified.

### Weight
`public int Weight`

Relative load-balancing weight assigned to this instance. Higher values attract proportionally more traffic. A value of `0` typically excludes the instance from active routing.

### Priority
`public int Priority`

Priority tier for the instance. Lower numeric values indicate higher priority. Used for priority-based routing where higher-priority instances are selected first.

### HealthStatus
`public DiscoveryHealthStatus HealthStatus`

Current health state of the instance. Updated via `RecordHealthy()` and `RecordUnhealthy()` calls. Consumers should inspect this before routing traffic.

### Source
`public DiscoverySource Source`

Origin of the discovery record (e.g., static configuration, DNS, consul, kubernetes). Indicates which discovery mechanism registered this instance.

### Tags
`public List<string> Tags`

Collection of string tags for categorization and filtering. Commonly used for environment, region, or capability-based routing decisions.

### Metadata
`public Dictionary<string, string> Metadata`

Arbitrary key-value pairs carrying additional instance information. May contain source-specific data such as datacenter, build info, or custom annotations.

### RegisteredAt
`public DateTime RegisteredAt`

Timestamp (UTC) when this instance was first registered in the discovery system. Set at creation and not modified by health updates.

### LastSeenAt
`public DateTime LastSeenAt`

Timestamp (UTC) of the most recent health check or discovery refresh that confirmed this instance's presence. Updated by `RecordHealthy()`.

### DnsTtlSeconds
`public int? DnsTtlSeconds`

Optional DNS time-to-live in seconds. Relevant when the record originates from DNS-based discovery. Null for non-DNS sources.

### ConsecutiveFailures
`public int ConsecutiveFailures`

Counter tracking the number of consecutive failed health checks. Incremented by `RecordUnhealthy()` and reset to zero by `RecordHealthy()`. Used by failure threshold logic.

### ToEndpointUri
`public string ToEndpointUri`

Returns a formatted URI string constructed from `Scheme`, `Host`, and `Port` (e.g., `"https://host:443"`). Omits the port component when the port is `0`. Does not throw.

### IsAlive
`public bool IsAlive`

Indicates whether the instance is considered healthy and eligible to receive traffic. Returns `true` when `HealthStatus` reflects a healthy state. Read-only property derived from `HealthStatus`.

### RecordHealthy
`public void RecordHealthy()`

Marks the instance as healthy. Sets `HealthStatus` to a healthy state, resets `ConsecutiveFailures` to `0`, and updates `LastSeenAt` to the current UTC time. Does not throw.

### RecordUnhealthy
`public void RecordUnhealthy()`

Marks the instance as unhealthy. Sets `HealthStatus` to an unhealthy state, increments `ConsecutiveFailures` by `1`, and updates `LastSeenAt` to the current UTC time. Does not throw.

## Usage

### Registering and monitoring a static service instance

```csharp
var record = new ServiceDiscoveryRecord
{
    InstanceId = Guid.NewGuid(),
    ServiceName = "payment-api",
    Version = "2.1.0",
    Host = "10.0.1.45",
    Port = 8443,
    Scheme = "https",
    Weight = 10,
    Priority = 1,
    Source = DiscoverySource.Static,
    Tags = new List<string> { "production", "us-east" },
    Metadata = new Dictionary<string, string>
    {
        ["datacenter"] = "dc1",
        ["commit"] = "a3f2b9c"
    },
    RegisteredAt = DateTime.UtcNow,
    LastSeenAt = DateTime.UtcNow
};

// Mark healthy after successful health check
record.RecordHealthy();

if (record.IsAlive)
{
    Console.WriteLine($"Endpoint: {record.ToEndpointUri()}");
    // Output: Endpoint: https://10.0.1.45:8443
}
```

### Handling failure thresholds before eviction

```csharp
var record = GetInstanceFromDiscovery("order-service");

// Simulate consecutive health check failures
for (int i = 0; i < 3; i++)
{
    bool checkPassed = PerformHealthCheck(record);
    if (!checkPassed)
    {
        record.RecordUnhealthy();
    }
}

// Evict if consecutive failures exceed threshold
const int failureThreshold = 3;
if (record.ConsecutiveFailures >= failureThreshold)
{
    DeregisterInstance(record);
    Console.WriteLine($"Instance {record.InstanceId} evicted after {record.ConsecutiveFailures} failures.");
}
else if (record.IsAlive)
{
    RouteTraffic(record);
}
```

## Notes

- `RecordHealthy()` and `RecordUnhealthy()` are not thread-safe by themselves. When multiple health check workers may update the same record concurrently, external synchronization (e.g., locking or using `ConcurrentDictionary` for record storage) is required to avoid torn updates to `HealthStatus`, `ConsecutiveFailures`, and `LastSeenAt`.
- `ConsecutiveFailures` is an unbounded counter. Consumers should implement their own threshold logic to trigger eviction or alerting; the record does not self-evict.
- `ToEndpointUri()` omits the port when `Port` is `0`, producing URIs like `"http://host"`. Ensure `Port` is set correctly for schemes that require explicit ports.
- `IsAlive` is a computed property reflecting the current `HealthStatus`. It does not consider `ConsecutiveFailures` or any time-based staleness. Consumers may need additional logic to treat instances as dead when `LastSeenAt` exceeds a grace period.
- `DnsTtlSeconds` is only meaningful for DNS-sourced records. It is null for other sources and has no effect on health checking or routing behavior within the record itself.
- The `required` modifier on `ServiceName` and `Host` enforces initialization at construction. Attempting to create a record without these properties results in a compile-time error.
