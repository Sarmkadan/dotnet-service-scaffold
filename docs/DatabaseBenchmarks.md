# DatabaseBenchmarks

The `DatabaseBenchmarks` class serves as the primary harness for performance testing and validation of database operations within the `dotnet-service-scaffold` project. It encapsulates the lifecycle of benchmark scenarios, providing asynchronous methods to execute CRUD operations, transactional workflows, and bulk data insertion against the underlying data store. This type manages its own resource initialization and cleanup, ensuring that each benchmark run starts from a known state and leaves the environment clean upon completion.

## API

### `Setup`
```csharp
public async Task Setup()
```
Initializes the database context and prepares the necessary schema or seed data required for subsequent benchmark operations. This method must be called before any other operation in the class.
*   **Returns**: A `Task` representing the asynchronous initialization operation.
*   **Throws**: May throw database connection exceptions or migration failures if the underlying infrastructure is unavailable or misconfigured.

### `Cleanup`
```csharp
public void Cleanup()
```
Removes test data generated during the benchmark session and resets the database state to its pre-benchmark condition. This is typically invoked after all benchmark methods have executed.
*   **Returns**: None.
*   **Throws**: May throw exceptions if the cleanup process encounters locked resources or integrity constraints.

### `Dispose`
```csharp
public void Dispose()
```
Releases unmanaged resources and disposes of internal database contexts or connections held by the instance. Implements the standard disposal pattern to ensure deterministic resource release.
*   **Returns**: None.
*   **Throws**: Generally should not throw; implementations should suppress exceptions during disposal unless critical resource leaks occur.

### `CreateUser`
```csharp
public async Task CreateUser()
```
Executes a single user creation operation to measure insert latency and overhead.
*   **Returns**: A `Task` representing the asynchronous write operation.
*   **Throws**: Throws on constraint violations (e.g., duplicate keys) or connection failures.

### `ReadUserByEmail`
```csharp
public async Task ReadUserByEmail()
```
Performs a lookup operation retrieving a user entity based on an email address, testing index efficiency and read latency.
*   **Returns**: A `Task` representing the asynchronous read operation.
*   **Throws**: Throws if the query fails due to database errors; does not throw if the user is not found (returns silently or handles internally depending on benchmark logic).

### `UpdateUser`
```csharp
public async Task UpdateUser()
```
Executes an update operation on an existing user record to measure write-amend latency.
*   **Returns**: A `Task` representing the asynchronous update operation.
*   **Throws**: Throws if the target record does not exist or if concurrency conflicts occur.

### `DeleteUser`
```csharp
public async Task DeleteUser()
```
Removes a specific user record from the database to test delete operation performance and cascade behaviors.
*   **Returns**: A `Task` representing the asynchronous delete operation.
*   **Throws**: Throws if foreign key constraints prevent deletion or if the record is missing.

### `CreateService`
```csharp
public async Task CreateService()
```
Inserts a new service entity into the database, validating the performance of service-related table writes.
*   **Returns**: A `Task` representing the asynchronous write operation.
*   **Throws**: Throws on schema mismatches or connection issues.

### `ListServices`
```csharp
public async Task ListServices()
```
Retrieves a collection of service entities, testing enumeration performance and data transfer overhead.
*   **Returns**: A `Task` representing the asynchronous read operation.
*   **Throws**: Throws if the query execution fails.

### `BulkCreateUsers`
```csharp
public async Task BulkCreateUsers()
```
Inserts a large set of user records in a single operation or optimized batch to measure throughput under high-load write scenarios.
*   **Returns**: A `Task` representing the asynchronous bulk write operation.
*   **Throws**: Throws if the transaction log fills, timeouts occur, or batch integrity is violated.

### `TransactionCommit`
```csharp
public async Task TransactionCommit()
```
Executes a series of database operations within an explicit transaction scope and commits the changes, measuring transactional overhead and ACID compliance performance.
*   **Returns**: A `Task` representing the asynchronous transactional operation.
*   **Throws**: Throws if any operation within the transaction fails, triggering a rollback, or if the commit itself fails.

## Usage

### Basic Benchmark Lifecycle
The following example demonstrates the standard lifecycle of initializing the harness, running a specific read/write scenario, and cleaning up resources.

```csharp
using var benchmarks = new DatabaseBenchmarks();

try 
{
    // Initialize schema and seed data
    await benchmarks.Setup();

    // Execute specific benchmark scenarios
    await benchmarks.CreateUser();
    await benchmarks.ReadUserByEmail();
    await benchmarks.UpdateUser();
}
finally 
{
    // Ensure test data is removed
    benchmarks.Cleanup();
}
```

### Transactional and Bulk Operations
This example illustrates testing high-throughput and transactional integrity scenarios, ensuring proper disposal of resources via the `using` statement.

```csharp
using (var benchmarks = new DatabaseBenchmarks())
{
    await benchmarks.Setup();

    // Measure bulk insertion throughput
    await benchmarks.BulkCreateUsers();

    // Measure transaction commit latency
    await benchmarks.TransactionCommit();

    // Validate service listing performance
    await benchmarks.ListServices();

    benchmarks.Cleanup();
}
// Dispose is called automatically here
```

## Notes

*   **Execution Order**: The `Setup` method must be invoked before any operational method (`CreateUser`, `ReadUserByEmail`, etc.). Calling operational methods prior to setup will likely result in null reference exceptions or database errors due to uninitialized contexts.
*   **State Management**: The `Cleanup` method assumes that `Setup` was successfully called. If `Setup` fails, `Cleanup` may operate on an undefined state; however, `Dispose` remains safe to call at any time.
*   **Thread Safety**: This class is not thread-safe. The internal database context and state trackers are designed for sequential execution within a single benchmark iteration. Concurrent calls to methods like `BulkCreateUsers` and `TransactionCommit` on the same instance may lead to race conditions, transaction deadlocks, or corrupted state.
*   **Resource Disposal**: While `Cleanup` handles logical data removal, `Dispose` is required to release physical connections and memory. Always wrap the instance in a `using` statement or ensure `Dispose` is called in a `finally` block.
*   **Exception Handling**: Operational methods do not swallow exceptions; they propagate database errors to the caller. Benchmark runners should implement retry logic or error recording around these calls to distinguish between infrastructure failures and performance outliers.
