# BasicServiceSetupExample

A utility class providing asynchronous methods to register, list, enable, disable, and inspect services within a .NET application. Designed for scenarios where services are dynamically configured or managed at runtime.

## API

### `BasicServiceSetupExample`
The default constructor initializes a new instance of the `BasicServiceSetupExample` class. No external dependencies are required for instantiation.

### `async Task<string> RegisterServiceAsync(string serviceName, string serviceType, Dictionary<string, string>? configuration = null)`
Registers a new service with the specified name, type, and optional configuration.

- **Parameters**
  - `serviceName`: The unique name of the service to register.
  - `serviceType`: The fully qualified type name of the service implementation.
  - `configuration`: An optional dictionary of key-value pairs representing service configuration settings. Defaults to `null`.

- **Return Value**
  Returns a `Task<string>` that resolves to a unique service identifier upon successful registration.

- **Exceptions**
  Throws `ArgumentNullException` if `serviceName` or `serviceType` is `null`.
  Throws `InvalidOperationException` if a service with the same name already exists.

### `async Task ListServicesAsync()`
Lists all registered services in the current application context.

- **Return Value**
  Returns a `Task` that completes when the list of services has been retrieved.

- **Exceptions**
  Throws `InvalidOperationException` if the service registry is unavailable.

### `async Task EnableServiceAsync(string serviceName)`
Enables an existing service by name.

- **Parameters**
  - `serviceName`: The unique name of the service to enable.

- **Return Value**
  Returns a `Task` that completes when the service has been enabled.

- **Exceptions**
  Throws `ArgumentNullException` if `serviceName` is `null`.
  Throws `KeyNotFoundException` if no service with the given name exists.

### `async Task DisableServiceAsync(string serviceName)`
Disables an existing service by name.

- **Parameters**
  - `serviceName`: The unique name of the service to disable.

- **Return Value**
  Returns a `Task` that completes when the service has been disabled.

- **Exceptions**
  Throws `ArgumentNullException` if `serviceName` is `null`.
  Throws `InvalidOperationException` if the service is already disabled or cannot be disabled.

### `async Task GetServiceDetailsAsync(string serviceName)`
Retrieves detailed information about a registered service.

- **Parameters**
  - `serviceName`: The unique name of the service to inspect.

- **Return Value**
  Returns a `Task<string>` that resolves to a JSON-formatted string containing service metadata (name, type, status, configuration).

- **Exceptions**
  Throws `ArgumentNullException` if `serviceName` is `null`.
  Throws `KeyNotFoundException` if no service with the given name exists.

### `static async Task Main(string[] args)`
Entry point for demonstration or integration purposes. Executes a sample workflow using the `BasicServiceSetupExample` class.

- **Parameters**
  - `args`: Command-line arguments (unused in this implementation).

- **Return Value**
  Returns a `Task` that completes when the demonstration workflow finishes.

## Usage

### Example 1: Basic Service Registration and Inspection
