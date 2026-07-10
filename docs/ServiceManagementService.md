# ServiceManagementService

Provides methods for registering, querying, updating, and unregistering services within a service registry. It supports service health tracking, ownership-based queries, and success-rate monitoring for registered services.

## API

### `ServiceManagementService`
Initializes a new instance of the `ServiceManagementService` class with required dependencies for service registration and discovery.

### `async Task<ServiceRegistration> RegisterServiceAsync`
Registers a new service in the registry.

- **Parameters**
  - `service`: The service metadata and configuration to register.
- **Return value**
  - A `Task<ServiceRegistration>` representing the asynchronous operation. The result contains the registered service details including generated identifiers.
- **Exceptions**
  - Throws `ArgumentNullException` if `service` is `null`.
  - Throws `InvalidOperationException` if the service name or identifier conflicts with an existing registration.

### `async Task<ServiceRegistration?> GetServiceAsync`
Retrieves a service by its unique identifier.

- **Parameters**
  - `id`: The unique identifier of the service to retrieve.
- **Return value**
  - A `Task<ServiceRegistration?>` representing the asynchronous operation. Returns the service registration if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is empty or whitespace.

### `async Task<ServiceRegistration?> GetServiceByNameAsync`
Retrieves a service by its name.

- **Parameters**
  - `name`: The name of the service to retrieve.
- **Return value**
  - A `Task<ServiceRegistration?>` representing the asynchronous operation. Returns the service registration if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `name` is empty or whitespace.

### `async Task<IEnumerable<ServiceRegistration>> GetServicesByOwnerAsync`
Retrieves all services owned by a specific owner.

- **Parameters**
  - `owner`: The owner identifier to filter services by.
- **Return value**
  - A `Task<IEnumerable<ServiceRegistration>>` representing the asynchronous operation. Returns an enumerable of matching service registrations.
- **Exceptions**
  - Throws `ArgumentException` if `owner` is empty or whitespace.

### `async Task<IEnumerable<ServiceRegistration>> GetAllServicesAsync`
Retrieves all registered services in the registry.

- **Return value**
  - A `Task<IEnumerable<ServiceRegistration>>` representing the asynchronous operation. Returns an enumerable of all service registrations.

### `async Task<ServiceRegistration> UpdateServiceAsync`
Updates an existing service registration.

- **Parameters**
  - `service`: The updated service metadata and configuration.
- **Return value**
  - A `Task<ServiceRegistration>` representing the asynchronous operation. The result contains the updated service details.
- **Exceptions**
  - Throws `ArgumentNullException` if `service` is `null`.
  - Throws `InvalidOperationException` if the service identifier does not exist or if the update conflicts with another registration.

### `async Task UnregisterServiceAsync`
Removes a service registration from the registry.

- **Parameters**
  - `id`: The unique identifier of the service to remove.
- **Return value**
  - A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentException` if `id` is empty or whitespace.
  - Throws `KeyNotFoundException` if the service identifier does not exist.

### `async Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync`
Retrieves all services currently marked as unhealthy.

- **Return value**
  - A `Task<IEnumerable<ServiceRegistration>>` representing the asynchronous operation. Returns an enumerable of unhealthy service registrations.

### `async Task<ServiceRegistration> DisableServiceAsync`
Disables a service by marking it as inactive.

- **Parameters**
  - `id`: The unique identifier of the service to disable.
- **Return value**
  - A `Task<ServiceRegistration>` representing the asynchronous operation. The result contains the updated service details.
- **Exceptions**
  - Throws `ArgumentException` if `id` is empty or whitespace.
  - Throws `InvalidOperationException` if the service is already disabled or does not exist.

### `async Task<ServiceRegistration> EnableServiceAsync`
Enables a previously disabled service.

- **Parameters**
  - `id`: The unique identifier of the service to enable.
- **Return value**
  - A `Task<ServiceRegistration>` representing the asynchronous operation. The result contains the updated service details.
- **Exceptions**
  - Throws `ArgumentException` if `id` is empty or whitespace.
  - Throws `InvalidOperationException` if the service is already enabled or does not exist.

### `async Task<decimal> GetServiceSuccessRateAsync`
Calculates the success rate of a service based on recent health checks.

- **Parameters**
  - `id`: The unique identifier of the service.
- **Return value**
  - A `Task<decimal>` representing the asynchronous operation. Returns a decimal value between 0 and 1 representing the success rate.
- **Exceptions**
  - Throws `ArgumentException` if `id` is empty or whitespace.
  - Throws `InvalidOperationException` if the service does not exist or has no health check history.

## Usage
