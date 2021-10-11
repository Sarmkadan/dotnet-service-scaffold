# AuditServiceTests

The `AuditServiceTests` class serves as the dedicated test suite for validating the behavior of the audit logging infrastructure within the `dotnet-service-scaffold` project. It encapsulates a series of asynchronous test methods designed to verify that audit logs are correctly persisted, timestamped, and retrieved based on user or entity filters, ensuring the integrity and reliability of the system's auditing capabilities.

## API

### `public AuditServiceTests`
Initializes a new instance of the `AuditServiceTests` class. This constructor sets up the necessary test context, including mock repositories and service dependencies required for executing the audit validation scenarios. It does not accept parameters and does not return a value.

### `public async Task LogAuditAsync_ShouldAddAuditLogToRepository`
Verifies that invoking the audit logging mechanism results in a new entry being added to the underlying repository.
*   **Parameters**: None (uses test context setup).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Exceptions**: Throws an assertion exception if the repository add method is not invoked exactly once or if the persisted entity does not match the expected audit data.

### `public async Task LogAuditAsync_ShouldSetCreatedAtTimestamp`
Ensures that when an audit log is created, the `CreatedAt` timestamp is populated with the current system time.
*   **Parameters**: None (uses test context setup).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Exceptions**: Throws an assertion exception if the `CreatedAt` property is null, default, or falls outside an acceptable time delta relative to the execution time.

### `public async Task GetAuditLogsForUserAsync_ShouldReturnLogsForUser`
Validates that querying audit logs by a specific user identifier returns only the logs associated with that user.
*   **Parameters**: None (user ID is defined within the test arrangement).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Exceptions**: Throws an assertion exception if the returned collection is empty, contains logs for other users, or does not match the expected count.

### `public async Task GetAuditLogsForUserAsync_ShouldReturnEmpty_WhenNoLogsForUser`
Confirms that requesting audit logs for a user who has no recorded activity results in an empty collection rather than null or an error.
*   **Parameters**: None (user ID is defined within the test arrangement).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Exceptions**: Throws an assertion exception if the returned collection is not empty or is null.

### `public async Task GetAuditLogsForEntityAsync_ShouldReturnLogsForEntity`
Validates that querying audit logs by a specific entity identifier returns only the logs related to that entity.
*   **Parameters**: None (entity ID is defined within the test arrangement).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Exceptions**: Throws an assertion exception if the returned collection contains logs for unrelated entities or fails to include all relevant logs.

### `public async Task GetAuditLogsForEntityAsync_ShouldReturnEmpty_WhenNoLogsForEntity`
Confirms that requesting audit logs for an entity with no recorded history results in an empty collection.
*   **Parameters**: None (entity ID is defined within the test arrangement).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Exceptions**: Throws an assertion exception if the returned collection is not empty or is null.

## Usage

The following examples demonstrate how the `AuditServiceTests` class functions within a typical xUnit test environment, illustrating both positive validation and edge case handling.

### Example 1: Verifying Log Persistence and Timestamping
This example illustrates the execution of tests ensuring that a new audit log is successfully added to the repository and that the creation timestamp is accurately set.

```csharp
using Xunit;
using DotNetServiceScaffold.Tests;

public class AuditIntegrationRunner
{
    [Fact]
    public async Task RunAuditCreationTests()
    {
        var testSuite = new AuditServiceTests();
        
        // Execute tests to verify log addition and timestamp accuracy
        await testSuite.LogAuditAsync_ShouldAddAuditLogToRepository();
        await testSuite.LogAuditAsync_ShouldSetCreatedAtTimestamp();
        
        // If no exceptions are thrown, the audit service correctly persists data
        Assert.True(true, "Audit creation tests passed.");
    }
}
```

### Example 2: Validating Retrieval Filters and Empty States
This example demonstrates running tests that verify the filtering logic for users and entities, specifically checking for correct data retrieval and proper handling of non-existent records.

```csharp
using Xunit;
using DotNetServiceScaffold.Tests;

public class AuditQueryRunner
{
    [Fact]
    public async Task RunAuditRetrievalTests()
    {
        var testSuite = new AuditServiceTests();

        // Verify filtering logic for existing data
        await testSuite.GetAuditLogsForUserAsync_ShouldReturnLogsForUser();
        await testSuite.GetAuditLogsForEntityAsync_ShouldReturnLogsForEntity();

        // Verify graceful handling of missing data
        await testSuite.GetAuditLogsForUserAsync_ShouldReturnEmpty_WhenNoLogsForUser();
        await testSuite.GetAuditLogsForEntityAsync_ShouldReturnEmpty_WhenNoLogsForEntity();
        
        Assert.True(true, "Audit retrieval and edge case tests passed.");
    }
}
```

## Notes

*   **Asynchronous Execution**: All test methods are asynchronous (`async Task`), indicating that the underlying audit service operations involve I/O-bound tasks such as database calls. Test runners must await these methods to ensure proper execution flow.
*   **State Isolation**: As a test class, `AuditServiceTests` likely relies on fresh instantiation or specific setup/teardown logic (not exposed in the public API) to ensure that the state of the mock repository is reset between tests. Running these tests in parallel without proper isolation mechanisms may lead to false positives or negatives if shared static state is inadvertently used in the implementation.
*   **Edge Case Handling**: The explicit inclusion of `ShouldReturnEmpty` methods highlights the system's contract that retrieval operations must return empty collections rather than `null` when no data exists, preventing `NullReferenceException` issues in consuming code.
*   **Timestamp Precision**: The `LogAuditAsync_ShouldSetCreatedAtTimestamp` test implies a dependency on system time. In environments with significant clock skew or high-latency test runners, assertions regarding timestamp accuracy may require tolerance thresholds to remain stable.
