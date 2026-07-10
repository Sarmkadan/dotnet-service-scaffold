# UpstreamCluster

`UpstreamCluster` represents a logical grouping of upstream service instances within a service mesh, providing aggregated health information, circuit-breaker state, and metadata for a named cluster. It serves as the primary model for inspecting and monitoring upstream dependencies in the `dotnet-service-scaffold` project.

## API

### `public string Name`
The unique name identifying this upstream cluster within the service mesh. This value is set during discovery and is immutable for the lifetime of the object.

### `public string Endpoint`
The primary endpoint address (host and port) used to reach this cluster. This is the address that the local proxy uses when forwarding requests to a healthy host in the cluster.

### `public int HealthyHosts`
The current number of hosts in the cluster that are passing health checks and are eligible to receive traffic.

### `public int TotalHosts`
The total number of hosts known to the cluster, regardless of their health status. This includes healthy, unhealthy, and degraded hosts.

### `public bool CircuitBreakerOpen`
Indicates whether the circuit breaker for this cluster is currently open. When `true`, the proxy will immediately fail requests destined for this cluster without attempting to forward them, based on the configured circuit-breaking thresholds.

### `public decimal GetHealthPercent`
Returns the percentage of healthy hosts relative to the total hosts in the cluster, expressed as a decimal between 0 and 100. A value of 100 indicates all known hosts are healthy. Returns 0 if `TotalHosts` is zero.

### `public string ProxyId`
The unique identifier of the service proxy instance that discovered and manages this cluster. This can be used to correlate cluster state with a specific proxy process.

### `public string MeshName`
The name of the service mesh to which this cluster belongs. Useful in environments where multiple meshes coexist.

### `public string ProxyVersion`
The version string of the proxy software managing this cluster. This helps identify feature availability and potential behavioral differences across proxy deployments.

### `public ServiceMeshStatus Status`
An enumeration value representing the overall operational status of the cluster. Typical values include `Healthy`, `Degraded`, and `Unhealthy`, derived from the proportion of healthy hosts and circuit-breaker state.

### `public string AdminEndpoint`
The administrative endpoint of the proxy where detailed cluster statistics and configuration can be queried directly. This is typically a localhost address with an admin port.

### `public Dictionary<string, string> Labels`
A dictionary of key-value pairs containing metadata labels attached to the cluster. These originate from the service mesh configuration and can include environment, version, or custom tags used for routing decisions.

### `public List<UpstreamCluster> UpstreamClusters`
A list of child `UpstreamCluster` instances that this cluster depends on transitively. This forms a directed graph of service dependencies, enabling traversal of the entire upstream call chain.

### `public DateTime LastChecked`
The UTC timestamp of the most recent health check or state refresh performed against this cluster. This value is updated each time the proxy re-evaluates host health or circuit-breaker conditions.

### `public bool IsHealthy`
A convenience boolean that returns `true` when the cluster is considered fully healthy: all hosts are healthy, the circuit breaker is closed, and the status is not `Unhealthy` or `Degraded`.

### `public IEnumerable<UpstreamCluster> GetOpenCircuits()`
Returns an enumerable collection of all upstream clusters (including this one and any transitive dependencies) that currently have an open circuit breaker. Traverses the `UpstreamClusters` graph recursively. Returns an empty enumeration if no circuits are open.

## Usage

### Example 1: Monitoring Cluster Health
```csharp
UpstreamCluster cluster = meshClient.GetCluster("payment-service");

Console.WriteLine($"Cluster: {cluster.Name}");
Console.WriteLine($"Healthy hosts: {cluster.HealthyHosts}/{cluster.TotalHosts}");
Console.WriteLine($"Health percent: {cluster.GetHealthPercent:F1}%");
Console.WriteLine($"Circuit breaker open: {cluster.CircuitBreakerOpen}");
Console.WriteLine($"Overall healthy: {cluster.IsHealthy}");
Console.WriteLine($"Last checked: {cluster.LastChecked:yyyy-MM-dd HH:mm:ss}");

if (cluster.CircuitBreakerOpen)
{
    // Trigger alert or fallback logic
    AlertingService.RaiseCircuitOpen(cluster.Name, cluster.Endpoint);
}
```

### Example 2: Traversing Open Circuits
```csharp
UpstreamCluster root = meshClient.GetCluster("api-gateway");

IEnumerable<UpstreamCluster> openCircuits = root.GetOpenCircuits();

foreach (UpstreamCluster broken in openCircuits)
{
    Console.WriteLine($"OPEN CIRCUIT: {broken.Name} at {broken.Endpoint}");
    Console.WriteLine($"  Proxy: {broken.ProxyId} v{broken.ProxyVersion}");
    Console.WriteLine($"  Admin endpoint: {broken.AdminEndpoint}");

    foreach (var label in broken.Labels)
    {
        Console.WriteLine($"  Label: {label.Key} = {label.Value}");
    }
}

if (!openCircuits.Any())
{
    Console.WriteLine("All upstream circuits are closed.");
}
```

## Notes

- **Recursive traversal**: `GetOpenCircuits()` performs a depth-first traversal of the `UpstreamClusters` graph. In deeply nested or cyclic dependency graphs, ensure that the implementation guards against infinite recursion; the method is expected to handle cycles internally and return each unique cluster at most once.
- **Staleness of data**: `LastChecked` reflects the proxy's last refresh time. Values such as `HealthyHosts`, `CircuitBreakerOpen`, and `GetHealthPercent` represent a point-in-time snapshot and may be stale by milliseconds or more depending on the proxy's polling interval.
- **Thread safety**: This type is designed as a read-only snapshot. All public properties are safe to read from multiple threads concurrently. However, the object itself is not mutated after creation; a new instance is produced when cluster state changes.
- **Zero hosts**: When `TotalHosts` is zero (no hosts discovered), `GetHealthPercent` returns 0, `IsHealthy` returns `false`, and `Status` typically reflects an indeterminate or unhealthy state.
- **`UpstreamClusters` list**: This list may be empty for leaf services with no further upstream dependencies. It is never null.
- **`Labels` dictionary**: May be empty but is never null. Keys and values are non-null strings.
