# AuditLogRepository
The `AuditLogRepository` class is designed to manage and retrieve audit logs from a database, providing a centralized location for logging and tracking system activities. It is constructed with a `ServiceScaffoldDbContext` instance and an `ILogger` instance, allowing for database operations and logging capabilities.

## API
* `AuditLogRepository(ServiceScaffoldDbContext context, ILogger<AuditLogRepository> logger)`: Constructs an instance of `AuditLogRepository` with the provided database context and logger. 
* `GetByUserIdAsync`: Retrieves a list of audit logs associated with a specific user ID. Returns an `IEnumerable<AuditLog>`. Throws if database operations fail.
* `GetByEntityAsync`: Retrieves a list of audit logs associated with a specific entity. Returns an `IEnumerable<AuditLog>`. Throws if database operations fail.
* `GetRecentLogsAsync`: Retrieves a list of recent audit logs. Returns an `IEnumerable<AuditLog>`. Throws if database operations fail.
* `GetFailedActionsAsync`: Retrieves a list of failed actions from the audit logs. Returns an `IEnumerable<AuditLog>`. Throws if database operations fail.
* `DeleteOldLogsAsync`: Deletes old audit logs from the database. Throws if database operations fail.

## Usage
```csharp
// Example 1: Retrieving audit logs for a specific user
var context = new ServiceScaffoldDbContext();
var logger = new LoggerFactory().CreateLogger<AuditLogRepository>();
var repository = new AuditLogRepository(context, logger);
var userId = 123;
var userLogs = await repository.GetByUserIdAsync(userId);
foreach (var log in userLogs)
{
    Console.WriteLine($"User {log.UserId} performed action {log.Action} on {log.Timestamp}");
}

// Example 2: Retrieving and deleting old audit logs
var context = new ServiceScaffoldDbContext();
var logger = new LoggerFactory().CreateLogger<AuditLogRepository>();
var repository = new AuditLogRepository(context, logger);
var recentLogs = await repository.GetRecentLogsAsync();
Console.WriteLine("Recent logs:");
foreach (var log in recentLogs)
{
    Console.WriteLine($"Action {log.Action} on {log.Timestamp}");
}
await repository.DeleteOldLogsAsync();
```

## Notes
The `AuditLogRepository` class is designed to be thread-safe, as it relies on the underlying database context and logger instances, which are expected to be thread-safe. However, concurrent access to the same database context instance may still lead to unexpected behavior. It is recommended to use a new instance of `ServiceScaffoldDbContext` for each operation or to implement proper synchronization mechanisms. Additionally, the `DeleteOldLogsAsync` method may throw if the database operations fail, and it is the caller's responsibility to handle such exceptions. The exact behavior of this method, including the definition of "old" logs, depends on the implementation details of the `ServiceScaffoldDbContext` instance.
