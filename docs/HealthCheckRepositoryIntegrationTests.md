# HealthCheckRepositoryIntegrationTests

Integration tests for verifying the behavior of the `HealthCheckRepository` class against a real database. These tests ensure that health check results can be correctly persisted, retrieved, queried, and deleted in a realistic storage environment.

## API

### `HealthCheckRepositoryIntegrationTests`
The test class containing integration tests for health check repository operations. It exercises the repository's methods against a real database context to validate CRUD operations and query behaviors.

### `AddHealthCheckResultAsync_ShouldAddResultToDatabase`
Verifies that calling `AddHealthCheckResultAsync` persists a health check result in the database. The test asserts that the result is stored and can be retrieved afterward.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test completion)
- **Throws**: Propagates any exceptions from the underlying repository or database operations (e.g., connection failures, constraint violations)

### `GetHealthCheckResultsForServiceAsync_ShouldReturnResultsForService`
Ensures that `GetHealthCheckResultsForServiceAsync` returns all health check results associated with a specific service. The test inserts multiple results and confirms the correct subset is returned.

- **Parameters**: None (uses injected test service identifier)
- **Return value**: `Task` (asynchronous test completion)
- **Throws**: Propagates exceptions from repository or database access

### `GetHealthCheckResultsForServiceAsync_ShouldReturnEmpty_WhenNoResults`
Validates that `GetHealthCheckResultsForServiceAsync` returns an empty collection when no health check results exist for the specified service.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test completion)
- **Throws**: Propagates exceptions from repository or database access

### `GetLatestHealthCheckResultForServiceAsync_ShouldReturnLatestResult`
Confirms that `GetLatestHealthCheckResultForServiceAsync` returns the most recent health check result for a given service based on timestamp ordering.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test completion)
- **Throws**: Propagates exceptions from repository or database access

### `GetLatestHealthCheckResultForServiceAsync_ShouldReturnNull_WhenNoResults`
Ensures that `GetLatestHealthCheckResultForServiceAsync` returns `null` when no health check results exist for the specified service.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test completion)
- **Throws**: Propagates exceptions from repository or database access

### `DeleteHealthCheckResultAsync_ShouldRemoveResultFromDatabase`
Tests that `DeleteHealthCheckResultAsync` removes a specific health check result from the database. The test inserts a result, deletes it, and verifies it is no longer retrievable.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test completion)
- **Throws**: Propagates exceptions from repository or database access

## Usage
