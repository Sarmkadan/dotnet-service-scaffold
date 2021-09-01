# RegistryServiceDiscoveryProvider

A service discovery provider that integrates with a registry system (such as Consul or similar) to resolve, register, and deregister services dynamically. It supports asynchronous operations for service resolution, registration, and deregistration, as well as watching for service changes and querying available services.

## API

### `RegistryServiceDiscoveryProvider`

The primary class providing service discovery functionality via a registry backend. It is designed to be initialized with registry-specific configuration and exposes methods for service lifecycle management and discovery.

### `async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveAsync()`

Resolves all service instances registered under their respective service names from the registry.

- **Parameters**: None
- **Return Value**: A `Result` containing a read-only list of `ServiceDiscoveryRecord` objects representing the discovered services. The result may indicate failure if the registry is unavailable or the operation times out.
- **Exceptions**: May throw if the underlying registry client fails or if the operation is canceled.

### `async Task<Result> RegisterAsync()`

Registers the current service instance with the registry.

- **Parameters**: None
- **Return Value**: A `Result` indicating success or failure of the registration operation.
- **Exceptions**: May throw if the registry is unreachable or if the service definition is invalid.

### `async Task<Result> DeregisterAsync()`

Deregisters the current service instance from the registry.

- **Parameters**: None
- **Return Value**: A `Result` indicating success or failure of the deregistration operation.
- **Exceptions**: May throw if the registry is unreachable or if the service was not previously registered.

### `async IAsyncEnumerable<IReadOnlyList<ServiceDiscoveryRecord>> WatchAsync()`

Watches the registry for changes to service instances and yields batches of updated service discovery records whenever changes occur.

- **Parameters**: None
- **Return Value**: An asynchronous enumerable of read-only lists of `ServiceDiscoveryRecord` objects, each representing the current state of services after a change.
- **Exceptions**: May throw if the watch operation fails to initialize or if the registry connection is lost.

### `async Task<bool> IsAvailableAsync()`

Checks whether the registry is currently available and reachable.

- **Parameters**: None
- **Return Value**: A boolean indicating whether the registry is available (`true`) or not (`false`).
- **Exceptions**: May throw if the health check operation fails unexpectedly.

### `async Task<Result<IReadOnlyList<string>>> GetAllServiceNamesAsync()`

Retrieves a list of all unique service names currently registered in the registry.

- **Parameters**: None
- **Return Value**: A `Result` containing a read-only list of service names. The result may indicate failure if the registry is unavailable.
- **Exceptions**: May throw if the operation cannot be completed due to registry unavailability.

### `string DeregisterCriticalServiceAfter`

Gets or sets the duration after which a service should be automatically deregistered if it has not sent a heartbeat (e.g., "5m" for 5 minutes). This value is used during registration to configure the registry's behavior.

- **Type**: `string`
- **Default**: Typically "0" or empty, meaning no automatic deregistration.
- **Usage**: Must be a valid duration string compatible with the underlying registry (e.g., Consul's format).

## Usage

### Example 1: Registering and Resolving a Service
