# ConfigurationServiceTests

The `ConfigurationServiceTests` class contains unit tests for the `ConfigurationService` component of the `dotnet-service-scaffold` project. Each test method validates a specific behavior of the service’s CRUD operations, ensuring that configurations are retrieved, created, and updated correctly under both expected and exceptional conditions. The tests follow the Arrange-Act-Assert pattern and are designed to run asynchronously.

## API

### `public ConfigurationServiceTests()`

Initializes a new instance of the `ConfigurationServiceTests` class. The constructor typically sets up any required test fixtures, mocks, or dependencies (e.g., an in-memory database or a mock repository) before each test run.

### `public async Task GetConfigurationByIdAsync_ShouldReturnConfiguration_WhenConfigurationExists()`

**Purpose:** Verifies that `GetConfigurationByIdAsync` returns a configuration object when a matching record exists in the data store.

**Parameters:** None (test method).

**Return value:** `Task` representing the asynchronous test operation. The test passes if the returned configuration is not null and matches the expected data.

**Throws:** Does not throw directly; the test framework reports failure if the assertion fails.

### `public async Task GetConfigurationByIdAsync_ShouldReturnNull_WhenConfigurationDoesNotExist()`

**Purpose:** Verifies that `GetConfigurationByIdAsync` returns `null` when no configuration with the given identifier exists.

**Parameters:** None.

**Return value:** `Task`. The test passes if the result is `null`.

**Throws:** None.

### `public async Task GetConfigurationByKeyAsync_ShouldReturnConfiguration_WhenConfigurationExists()`

**Purpose:** Verifies that `GetConfigurationByKeyAsync` returns a configuration object when a record with the specified key exists.

**Parameters:** None.

**Return value:** `Task`. The test passes if the returned configuration is not null and matches the expected key.

**Throws:** None.

### `public async Task GetConfigurationByKeyAsync_ShouldReturnNull_WhenConfigurationDoesNotExist()`

**Purpose:** Verifies that `GetConfigurationByKeyAsync` returns `null` when no configuration with the given key exists.

**Parameters:** None.

**Return value:** `Task`. The test passes if the result is `null`.

**Throws:** None.

### `public async Task CreateConfigurationAsync_ShouldReturnConfiguration_WhenCreatedSuccessfully()`

**Purpose:** Verifies that `CreateConfigurationAsync` successfully creates a new configuration and returns the created object (including any generated identifier).

**Parameters:** None.

**Return value:** `Task`. The test passes if the returned configuration is not null and contains the expected values.

**Throws:** None.

### `public async Task CreateConfigurationAsync_ShouldThrowException_WhenKeyAlreadyExists()`

**Purpose:** Verifies that `CreateConfigurationAsync` throws an appropriate exception (e.g., `InvalidOperationException` or `ArgumentException`) when attempting to create a configuration with a key that already exists.

**Parameters:** None.

**Return value:** `Task`. The test passes if the expected exception is thrown.

**Throws:** The test itself does not throw; it expects the service method to throw.

### `public async Task UpdateConfigurationAsync_ShouldUpdateConfiguration_WhenConfigurationExists()`

**Purpose:** Verifies that `UpdateConfigurationAsync` correctly updates an existing configuration and returns the updated object.

**Parameters:** None.

**Return value:** `Task`. The test passes if the returned configuration reflects the applied changes.

**Throws:** None.

### `public async Task UpdateConfigurationAsync_ShouldThrowException_WhenConfigurationDoesNotExist()`

**Purpose:** Verifies that `UpdateConfigurationAsync` throws an appropriate exception (e.g., `KeyNotFoundException` or `InvalidOperationException`) when trying to update a configuration that does not exist.

**Parameters:** None.

**Return value:** `Task`. The test passes if the expected exception is thrown.

**Throws:** None.

## Usage

The following examples demonstrate how to run the tests and interpret their results. These tests are typically executed via a test runner (e.g., `dotnet test` or Visual Studio Test Explorer).

### Example 1: Running all tests from the command line

```bash
dotnet test --filter "FullyQualifiedName~ConfigurationServiceTests"
```

This command runs every test method in the `ConfigurationServiceTests` class. A successful run produces output similar to:

```
Passed! - Failed: 0, Passed: 8, Skipped: 0, Total: 8
```

### Example 2: Running a single test and inspecting failure

```csharp
// In a test file or interactive session, you can invoke the test directly:
var testInstance = new ConfigurationServiceTests();
await testInstance.GetConfigurationByIdAsync_ShouldReturnConfiguration_WhenConfigurationExists();
```

If the test fails (e.g., because the service returns `null` unexpectedly), the test runner will report an assertion failure with details about the expected and actual values. For instance, a failure message might read:

```
Assert.IsNotNull failed. Expected: non-null, Actual: null.
```

## Notes

- **Edge cases:** The tests cover both existence and non-existence scenarios for retrieval and update operations, as well as duplicate key prevention during creation. They do not explicitly test boundary values (e.g., very long keys, null keys, or empty strings) unless those are part of the service contract. Additional tests may be needed for such edge cases.
- **Thread safety:** These tests are not designed to be run concurrently on the same data store instance. If the underlying repository is not thread-safe, parallel execution of tests that modify state (e.g., `CreateConfigurationAsync_ShouldReturnConfiguration_WhenCreatedSuccessfully` and `CreateConfigurationAsync_ShouldThrowException_WhenKeyAlreadyExists`) could lead to race conditions and flaky results. It is recommended to use a fresh test fixture (e.g., a new in-memory database) for each test method, or to run tests sequentially when they share state.
- **Dependency isolation:** The tests assume that dependencies (e.g., database context, repository) are replaced with mocks or in-memory implementations. The exact setup is not exposed in the public API but is typically configured in the constructor or via a test initializer.
