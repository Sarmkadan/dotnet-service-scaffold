# ServiceScaffoldDbContext

`ServiceScaffoldDbContext` is a specialized Entity Framework Core `DbContext` implementation designed for the `dotnet-service-scaffold` project. It provides access to the application's data model, including user management, service registration tracking, health monitoring, metrics collection, event logging, API key management, audit trails, and service configuration. The context is optimized for asynchronous database initialization and supports the project's infrastructure requirements.

## API

### `ServiceScaffoldDbContext`
Initializes a new instance of the `ServiceScaffoldDbContext` class with default configuration. This constructor uses dependency injection to receive the database context options and applies the project's specific conventions and configurations.

```csharp
public ServiceScaffoldDbContext(DbContextOptions<ServiceScaffoldDbContext> options)
```

### `Users`
Gets or sets the `DbSet<User>` representing the collection of user entities in the database. This set allows querying and modifying user records through Entity Framework Core operations.

```csharp
public DbSet<User> Users { get; set; }
```

### `ServiceRegistrations`
Gets or sets the `DbSet<ServiceRegistration>` representing the collection of service registration entities. Used to track registered services within the application ecosystem.

```csharp
public DbSet<ServiceRegistration> ServiceRegistrations { get; set; }
```

### `HealthCheckResults`
Gets or sets the `DbSet<HealthCheckResult>` representing the collection of health check result entities. Stores historical and current health statuses of monitored services.

```csharp
public DbSet<HealthCheckResult> HealthCheckResults { get; set; }
```

### `ServiceMetrics`
Gets or sets the `DbSet<ServiceMetric>` representing the collection of service metric entities. Captures performance and usage metrics for services over time.

```csharp
public DbSet<ServiceMetric> ServiceMetrics { get; set; }
```

### `ServiceEvents`
Gets or sets the `DbSet<ServiceEvent>` representing the collection of service event entities. Records significant events occurring within the service ecosystem for auditing and diagnostics.

```csharp
public DbSet<ServiceEvent> ServiceEvents { get; set; }
```

### `ApiKeys`
Gets or sets the `DbSet<ApiKey>` representing the collection of API key entities. Manages authentication credentials used by services and clients.

```csharp
public DbSet<ApiKey> ApiKeys { get; set; }
```

### `AuditLogs`
Gets or sets the `DbSet<AuditLog>` representing the collection of audit log entities. Tracks administrative actions and system changes for compliance and security monitoring.

```csharp
public DbSet<AuditLog> AuditLogs { get; set; }
```

### `ServiceConfigurations`
Gets or sets the `DbSet<ServiceConfiguration>` representing the collection of service configuration entities. Stores runtime and deployment-specific settings for services.

```csharp
public DbSet<ServiceConfiguration> ServiceConfigurations { get; set; }
```

### `InitializeDatabaseAsync`
Asynchronously initializes the database schema if it does not already exist. Applies migrations or creates the database structure based on the current model. This method is idempotent and safe to call multiple times.

```csharp
public async Task InitializeDatabaseAsync()
```

**Exceptions:**
- Throws `DbUpdateException` if the database initialization fails due to a conflict or constraint violation.
- Throws `InvalidOperationException` if the context is disposed or the connection is not available.

## Usage

### Example 1: Basic Initialization and Query
```csharp
using var context = new ServiceScaffoldDbContext(options);
await context.InitializeDatabaseAsync();

// Query all active service registrations
var activeServices = await context.ServiceRegistrations
    .Where(s => s.IsActive)
    .ToListAsync();
```

### Example 2: Transactional Update with Audit Logging
```csharp
using var context = new ServiceScaffoldDbContext(options);
await context.InitializeDatabaseAsync();

using var transaction = await context.Database.BeginTransactionAsync();
try
{
    var user = new User { Username = "admin", IsActive = true };
    context.Users.Add(user);

    var auditLog = new AuditLog
    {
        Action = "UserCreated",
        EntityType = nameof(User),
        EntityId = user.Id,
        Timestamp = DateTime.UtcNow
    };
    context.AuditLogs.Add(auditLog);

    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

## Notes

- **Thread Safety:** Instances of `ServiceScaffoldDbContext` are not thread-safe. Each thread or async operation should use its own instance or ensure proper synchronization when sharing a context.
- **Disposal:** The context should be disposed after use to release database connections and resources. Prefer `using` statements or dependency injection scopes.
- **Async Support:** All database operations should be awaited to avoid blocking threads. The context is designed to work with asynchronous APIs throughout.
- **Schema Changes:** Migrations should be applied using Entity Framework Core tooling. Manual schema modifications may lead to inconsistencies.
- **Connection Resilience:** The context does not implement retry logic for transient failures. Consider wrapping operations with a retry policy if needed.
- **Performance:** For bulk operations, consider using `DbContext`'s change tracking optimizations or raw SQL commands to minimize overhead.
