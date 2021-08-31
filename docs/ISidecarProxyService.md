# ISidecarProxyService

Provides an abstraction for interacting with a sidecar proxy service (Envoy) to retrieve configuration, health, and control-plane state. Implementations typically wrap Envoy xDS APIs or management server calls.

## API

### `SidecarProxyService`
Constructor for the concrete service implementation. Initializes the proxy client and configuration required to communicate with the sidecar (Envoy) control plane or admin interface.

### `async Task<SidecarProxyInfo> GetProxyInfoAsync()`
Retrieves metadata and runtime information about the proxy instance.
- **Returns**: A `SidecarProxyInfo` object containing version, state, node identity, and cluster details.
- **Throws**: `InvalidOperationException` if the proxy is unreachable or returns malformed data.

### `async Task<bool> CheckReadinessAsync()`
Checks whether the proxy is ready to accept traffic.
- **Returns**: `true` if the proxy reports readiness; otherwise `false`.
- **Throws**: `InvalidOperationException` if the readiness endpoint is unavailable or returns an unexpected status.

### `async Task<IReadOnlyList<UpstreamCluster>> GetUpstreamClustersAsync()`
Fetches the list of upstream clusters known to the proxy.
- **Returns**: A read-only list of `UpstreamCluster` objects describing each cluster’s configuration and endpoints.
- **Throws**: `InvalidOperationException` if the xDS or cluster discovery service is unreachable.

### `async Task DrainConnectionsAsync()`
Initiates a graceful shutdown by draining active connections.
- **Behavior**: Stops accepting new connections and allows in-flight requests to complete before termination.
- **Throws**: `InvalidOperationException` if the proxy does not support draining or the operation fails.

### `async Task<bool> IsServiceMeshEnabledAsync()`
Determines whether the proxy is operating within a service mesh context.
- **Returns**: `true` if mesh features (e.g., SDS, EDS) are enabled; otherwise `false`.
- **Throws**: `InvalidOperationException` if the proxy configuration cannot be queried.

### `string? Version`
Gets the semantic version of the proxy (e.g., `"1.25.0"`).
- **Returns**: The version string, or `null` if unknown or unavailable.

### `string? State`
Gets the current operational state of the proxy (e.g., `"LIVE"`, `"DRAINING"`, `"WARMING"`).
- **Returns**: The state string, or `null` if unknown.

### `EnvoyNode? Node`
Gets the node identity used by the proxy when communicating with the control plane.
- **Returns**: An `EnvoyNode` descriptor, or `null` if not configured.

### `string? Id`
Gets the unique identifier of the proxy instance.
- **Returns**: The ID string, or `null` if not assigned.

### `string? Cluster`
Gets the cluster name this proxy belongs to.
- **Returns**: The cluster name, or `null` if not defined.

### `List<EnvoyClusterStatus>? ClusterStatuses`
Gets the status of each cluster managed by the proxy.
- **Returns**: A list of `EnvoyClusterStatus` objects, or `null` if unavailable.

### `string? Name`
Gets the human-readable name of the proxy instance.
- **Returns**: The name string, or `null` if not set.

### `List<EnvoyHostStatus>? HostStatuses`
Gets the health status of individual hosts within upstream clusters.
- **Returns**: A list of `EnvoyHostStatus` objects, or `null` if not populated.

### `EnvoyAddress? Address`
Gets the network address where the proxy is listening.
- **Returns**: An `EnvoyAddress` descriptor, or `null` if unknown.

### `EnvoyHealthStatus? HealthStatus`
Gets the overall health status of the proxy.
- **Returns**: An `EnvoyHealthStatus` value, or `null` if indeterminate.

### `EnvoySocketAddress? SocketAddress`
Gets the socket-level address details of the proxy.
- **Returns**: An `EnvoySocketAddress` descriptor, or `null` if not available.

### `string? Address`
Gets the IP address or hostname of the proxy.
- **Returns**: The address string, or `null` if unknown.

### `int PortValue`
Gets the port number the proxy is listening on.
- **Returns**: The port number, or `0` if unknown.

### `string? EdsHealthStatus`
Gets the EDS-reported health status of the proxy.
- **Returns**: The EDS health status string, or `null` if not reported.
