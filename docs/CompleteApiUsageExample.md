# CompleteApiUsageExample

A utility class demonstrating complete API usage patterns for user management, service administration, and system monitoring in a .NET service scaffold. It encapsulates common asynchronous operations for user registration, authentication, API key generation, service lifecycle management, and system diagnostics.

## API

### `CompleteApiUsageExample`

The main entry point for the API usage examples. This class provides static methods to demonstrate realistic workflows and is also used as a demonstration harness.

### `async Task<string> RegisterUserAsync(string username, string password, string email)`

Registers a new user in the system.
- **Parameters**:
  - `username`: Unique username for the new user.
  - `password`: Secure password for the user.
  - `email`: Valid email address for the user.
- **Return value**: A task that resolves to a confirmation message or user identifier upon success.
- **Throws**: `ArgumentException` if `username`, `password`, or `email` is invalid or already exists.

### `async Task LoginAsync(string username, string password)`

Authenticates a user and establishes a session.
- **Parameters**:
  - `username`: Registered username.
  - `password`: Password matching the user.
- **Return value**: A task that completes upon successful login.
- **Throws**: `UnauthorizedAccessException` if credentials are invalid.

### `async Task<string> CreateApiKeyAsync(string username, string purpose)`

Generates a new API key for programmatic access.
- **Parameters**:
  - `username`: Owner of the API key.
  - `purpose`: Descriptive purpose of the key (e.g., "CI/CD pipeline").
- **Return value**: A task that resolves to the generated API key string.
- **Throws**: `ArgumentException` if `username` does not exist or `purpose` is invalid.

### `async Task<string> RegisterServiceAsync(string name, string description, string ownerUsername)`

Registers a new service under a user account.
- **Parameters**:
  - `name`: Unique service name.
  - `description`: Human-readable description.
  - `ownerUsername`: Username of the service owner.
- **Return value**: A task that resolves to a service identifier upon success.
- **Throws**: `ArgumentException` if `name` already exists or `ownerUsername` does not exist.

### `async Task<string> GetServicesAsync(string ownerUsername)`

Retrieves all services owned by a user.
- **Parameters**:
  - `ownerUsername`: Username of the service owner.
- **Return value**: A task that resolves to a JSON string containing service details.
- **Throws**: `ArgumentException` if `ownerUsername` does not exist.

### `async Task<string> PerformHealthCheckAsync(string serviceId)`

Performs an immediate health check on a registered service.
- **Parameters**:
  - `serviceId`: Identifier of the service to check.
- **Return value**: A task that resolves to a health status report.
- **Throws**: `KeyNotFoundException` if `serviceId` does not exist.

### `async Task<string> GetHealthHistoryAsync(string serviceId, DateTime from, DateTime to)`

Retrieves historical health check data for a service within a time range.
- **Parameters**:
  - `serviceId`: Identifier of the service.
  - `from`: Start of the time range (inclusive).
  - `to`: End of the time range (inclusive).
- **Return value**: A task that resolves to a JSON string containing health history records.
- **Throws**: `ArgumentException` if `from` is after `to` or if `serviceId` does not exist.

### `async Task<string> GetMetricsAsync(string serviceId, DateTime from, DateTime to)`

Retrieves performance metrics for a service within a time range.
- **Parameters**:
  - `serviceId`: Identifier of the service.
  - `from`: Start of the time range (inclusive).
  - `to`: End of the time range (inclusive).
- **Return value**: A task that resolves to a JSON string containing metric records.
- **Throws**: `ArgumentException` if `from` is after `to` or if `serviceId` does not exist.

### `async Task<string> GetAuditLogsAsync(string serviceId, DateTime from, DateTime to)`

Retrieves audit logs for actions performed on a service within a time range.
- **Parameters**:
  - `serviceId`: Identifier of the service.
  - `from`: Start of the time range (inclusive).
  - `to`: End of the time range (inclusive).
- **Return value**: A task that resolves to a JSON string containing audit entries.
- **Throws**: `ArgumentException` if `from` is after `to` or if `serviceId` does not exist.

### `async Task EnableServiceAsync(string serviceId)`

Enables a previously disabled service.
- **Parameters**:
  - `serviceId`: Identifier of the service to enable.
- **Return value**: A task that completes upon success.
- **Throws**: `KeyNotFoundException` if `serviceId` does not exist.

### `async Task DisableServiceAsync(string serviceId)`

Disables a running service.
- **Parameters**:
  - `serviceId`: Identifier of the service to disable.
- **Return value**: A task that completes upon success.
- **Throws**: `KeyNotFoundException` if `serviceId` does not exist.

### `async Task ChangePasswordAsync(string username, string currentPassword, string newPassword)`

Changes the password for an existing user.
- **Parameters**:
  - `username`: Username of the user.
  - `currentPassword`: Current valid password.
  - `newPassword`: New secure password.
- **Return value**: A task that completes upon success.
- **Throws**: `UnauthorizedAccessException` if `currentPassword` is invalid or `ArgumentException` if `newPassword` is invalid.

### `static async Task Main(string[] args)`

Entry point for demonstration execution. Orchestrates a sequence of API calls to showcase typical usage patterns.
- **Parameters**:
  - `args`: Command-line arguments (unused in this context).
- **Return value**: A task that completes when the demonstration finishes.

## Usage
