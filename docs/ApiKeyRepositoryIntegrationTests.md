# ApiKeyRepositoryIntegrationTests

Integration test suite for the `ApiKeyRepository` component, verifying that CRUD operations against the underlying data store behave as expected. The tests use a real database instance (typically an in‑memory or test‑specific SQL database) to ensure that the repository implementation correctly translates entity changes into persisted state.

## API

### ApiKeyRepositoryIntegrationTests
- **Purpose**: Test class that contains asynchronous test methods for validating the behavior of `ApiKeyRepository`.
- **Parameters**: None.
- **Return value**: N/A (type).
- **Exceptions**: None thrown directly by the type; individual test methods may propagate exceptions from the repository or data access layer.

### AddApiKey_ShouldAddApiKeyToDatabase
- **Purpose**: Verifies that calling `AddApiKey` on the repository inserts a new API key record into the database.
- **Parameters**: None.
- **Return value**: `Task` representing the asynchronous operation.
- **When it throws**: 
  - `DbUpdateException` if the insert violates a database constraint (e.g., duplicate unique key).
  - Any exception thrown by the underlying data access layer propagates upward.

### GetApiKeyById_ShouldReturnCorrectApiKey
- **Purpose**: Confirms that `GetApiKeyById` retrieves the exact API key entity previously inserted, matching by its identifier.
- **Parameters**: None.
- **Return value**: `Task`.
- **When it throws**: 
  - `InvalidOperationException` if the repository cannot map the returned row to an entity.
  - Any data‑access exception is bubbled up.

### UpdateApiKey_ShouldUpdateApiKeyInDatabase
- **Purpose**: Ensures that `UpdateApiKey` modifies an existing API key record and that the changes are persisted.
- **Parameters**: None.
- **Return value**: `Task`.
- **When it throws**: 
  - `DbUpdateConcurrencyException` if the record has been altered concurrently.
  - `ArgumentNullException` if a null entity is passed (though the test supplies a valid instance).
  - Any other repository‑level exception.

### DeleteApiKey_ShouldRemoveApiKeyFromDatabase
- **Purpose**: Validates that `DeleteApiKey` removes the specified API key row from the database.
- **Parameters**: None.
- **Return value**: `Task`.
- **When it throws**: 
  - `DbUpdateException` if a foreign‑key constraint prevents deletion.
  - Any exception from the data access layer surfaces to the test.

### GetAllApiKeys_ShouldReturnAllApiKeys
- **Purpose**: Checks that `GetAllApiKeys` returns every API key row currently stored in the test database.
- **Parameters**: None.
- **Return value**: `Task<IReadOnlyList<ApiKey>>` (the test asserts on the result).
- **When it throws**: 
  - Exceptions from the query execution (e.g., `SqlException`) are propagated.

### GetApiKeyByNonExistentId_ShouldReturnNull
- **Purpose**: Asserts that requesting an API key with an identifier that does not exist yields `null`.
- **Parameters**: None.
- **Return value**: `Task`.
- **When it throws**: 
  - Only throws if the repository encounters an unexpected error; otherwise returns `null` successfully.

### AddApiKey_WithExistingPrefix_ShouldThrowException
- **Purpose**: Confirms that attempting to add an API key with a prefix that already exists in the database throws an appropriate exception, enforcing uniqueness.
- **Parameters**: None.
- **Return value**: `Task`.
- **When it throws**: 
  - Expected exception type is defined by the repository contract (commonly `InvalidOperationException` or a custom `DuplicatePrefixException`). 
  - Any other unexpected exception will cause the test to fail.

## Usage

```csharp
// Example 1: Executing a single test method via an instance.
var testClass = new ApiKeyRepositoryIntegrationTests();
await testClass.AddApiKey_ShouldAddApiKeyToDatabase();
// If the method completes without throwing, the insert succeeded.

// Example 2: Running the whole test suite with a test runner (xUnit shown).
// Assuming the project targets .NET and uses the xUnit runner:
dotnet test --filter FullyQualifiedName~ApiKeyRepositoryIntegrationTests
```
The first snippet demonstrates direct invocation for debugging or manual verification. The second snippet shows the typical way to run all tests in the class using the CLI test runner, which discovers each `[Fact]` (or equivalent) method and executes them asynchronously.

## Notes

- Each test method assumes a clean database state; the test class typically initializes a fresh database or rolls back transactions in its constructor or `IAsyncLifetime` implementation. Consequently, tests are **not** thread‑safe if executed concurrently against the same database instance without proper isolation.
- The tests do not accept parameters; any variation in test data is handled internally (e.g., by generating unique prefixes or identifiers).
- If the underlying repository changes its exception contract, the corresponding test assertions may need updating; the documentation reflects the current expected behavior based on the method names.
- These are integration tests; they exercise the real data access layer and therefore run slower than pure unit tests. They should be executed in a dedicated test environment to avoid interfering with development or production data.
