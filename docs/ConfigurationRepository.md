# ConfigurationRepository

Provides data access methods for retrieving, checking, and deleting service configuration entries from the database. Designed for use within applications built on the `dotnet-service-scaffold` project to manage service-specific settings stored as key-value pairs.

## API

### `ConfigurationRepository(ServiceScaffoldDbContext context, ILogger<ConfigurationRepository> logger)`

Initializes a new instance of the `ConfigurationRepository` with the specified database context and logger.

- **Parameters**
  - `context`: The `ServiceScaffoldDbContext` instance used to interact with the database.
  - `logger`: The `ILogger<ConfigurationRepository>` instance used for logging operational details and errors.

### `async Task<ServiceConfiguration?> GetByKeyAsync(string key)`

Retrieves a service configuration entry by its key.

- **Parameters**
  - `key`: The unique identifier of the configuration entry to retrieve.
- **Returns**
  - A `Task` resolving to the `ServiceConfiguration` instance if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `async Task<IEnumerable<ServiceConfiguration>> GetByServiceIdAsync(int serviceId)`

Retrieves all service configuration entries associated with the specified service identifier.

- **Parameters**
  - `serviceId`: The identifier of the service whose configurations are to be retrieved.
- **Returns**
  - A `Task` resolving to an `IEnumerable<ServiceConfiguration>` containing all matching configurations.
- **Exceptions**
  - Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `async Task<bool> KeyExistsAsync(string key)`

Determines whether a configuration entry with the specified key exists.

- **Parameters**
  - `key`: The key to check for existence.
- **Returns**
  - A `Task<bool>` resolving to `true` if the key exists; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `async Task DeleteByKeyAsync(string key)`

Deletes the configuration entry with the specified key.

- **Parameters**
  - `key`: The key of the configuration entry to delete.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.
  - Throws `InvalidOperationException` if the configuration entry does not exist.

## Usage

### Retrieving a configuration by key
