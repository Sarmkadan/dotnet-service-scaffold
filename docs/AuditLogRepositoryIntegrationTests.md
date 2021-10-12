# AuditLogRepositoryIntegrationTests

The `AuditLogRepositoryIntegrationTests` class contains integration tests for the `AuditLogRepository` component. It validates that the repository correctly performs CRUD operations against a real database instance. Each test method is asynchronous and uses an xUnit `Fact` attribute (or equivalent) to execute against a shared database fixture. The class is intended to be run as part of a continuous integration pipeline or manually to verify data access layer behavior.

## API

### `AuditLogRepositoryIntegrationTests`

The constructor initializes the test class, typically by setting up a database context or a test fixture that provides a fresh database state for each test run. No parameters are exposed publicly.

### `public async Task AddAuditLog_ShouldAddAuditLogToDatabase`

- **Purpose**: Verifies that a new audit log entry can be inserted into the database and that the entry is persisted with the expected properties.
- **Parameters**: None.
- **Return value**: `Task` – the test completes when the assertion passes or fails.
- **Throws**: `Xunit.Sdk.XunitException` if the inserted record is not found or its properties do not match the expected values. May also throw database-related exceptions (e.g., `DbUpdateException`) if the database is unavailable or constraints are violated.

### `public async Task GetAuditLogById_ShouldReturnCorrectAuditLog`

- **Purpose**: Ensures that retrieving an audit log by its unique identifier returns the correct entity.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the returned entity is null or its identifier does not match the requested ID. Database connectivity errors may also be thrown.

### `public async Task UpdateAuditLog_ShouldUpdateAuditLogInDatabase`

- **Purpose**: Confirms that an existing audit log can be updated and that the changes are reflected in the database after the update.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the updated entity does not contain the new values or if the entity cannot be found after update. Database exceptions may occur on concurrency conflicts or connection failures.

### `public async Task DeleteAuditLog_ShouldRemoveAuditLogFromDatabase`

- **Purpose**: Validates that deleting an audit log removes the record from the database and that subsequent retrieval returns `null`.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the record still exists after deletion or if the delete operation fails silently. Database exceptions may be thrown if the record is already deleted or the connection is lost.

### `public async Task GetAllAuditLogs_ShouldReturnAllAuditLogs`

- **Purpose**: Checks that the repository returns all audit log entries present in the database, and that the count matches the expected number of seeded records.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the returned collection is null, empty when it should contain records, or has an unexpected count. Database exceptions may occur.

### `public async Task GetAuditLogByNonExistentId_ShouldReturnNull`

- **Purpose**: Verifies that querying for an audit log with an ID that does not exist in the database returns `null` rather than throwing an exception.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the result is not `null`. Database exceptions may be thrown if the query itself fails (e.g., invalid connection).

## Usage

The following examples demonstrate how to run the integration tests using a typical xUnit test runner. The tests assume a database fixture is configured (e.g., via a shared `DatabaseFixture` class that manages a test database instance).

**Example 1: Running a single test method**

```csharp
// Arrange: The test class is instantiated by the test framework.
var test = new AuditLogRepositoryIntegrationTests();

// Act & Assert: The test method performs its own setup and assertions.
await test.AddAuditLog_ShouldAddAuditLogToDatabase();
```

**Example 2: Using a collection fixture to share database state**

```csharp
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

[Collection("Database collection")]
public class AuditLogRepositoryIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public AuditLogRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllAuditLogs_ShouldReturnAllAuditLogs()
    {
        // The fixture provides a fresh database context.
        var repository = new AuditLogRepository(_fixture.CreateContext());
        var logs = await repository.GetAllAsync();
        Assert.NotNull(logs);
        // Additional assertions...
    }
}
```

## Notes

- **Edge cases**: The test `GetAuditLogByNonExistentId_ShouldReturnNull` explicitly covers the scenario where a requested ID does not exist. Other tests assume that the database is seeded with known data before each test run. Tests that modify data (Add, Update, Delete) should be isolated to avoid side effects on subsequent tests.
- **Thread safety**: Integration tests in this class are not inherently thread-safe. Running tests in parallel against the same database instance may cause race conditions, duplicate key violations, or inconsistent state. It is recommended to either use a dedicated test database per test run or to disable parallel test execution for this class (e.g., via `[Collection]` attribute or xUnit configuration).
- **Database state**: Each test method should clean up any data it creates, or the test fixture should reset the database between tests (e.g., by rolling back transactions or truncating tables). Failure to do so may cause tests to interfere with one another.
- **Dependencies**: The class depends on a configured database connection string and a running database instance. Tests will fail if the database is unreachable or if the schema does not match the repository expectations.
