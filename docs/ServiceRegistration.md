# ServiceRegistration

`ServiceRegistration` is a data model that represents a registered service within the `dotnet-service-scaffold` system. It captures the service’s identity, health check configuration, runtime metrics, and ownership. Instances are typically persisted in a database and used to drive health monitoring, service discovery, and lifecycle management. The type uses C# 11 `required` modifiers to enforce that essential fields are provided at construction time.

## API

The following public members are defined on `ServiceRegistration`. All properties are read/write unless otherwise noted.

| Member | Type | Description |
|--------|------|-------------|
| `Id` | `Guid` | Unique identifier for the service registration. |
| `ServiceName` | `string` (required) | Human-readable name of the service. Must be provided at instantiation. |
| `Description` | `string?` | Optional free‑text description of the service. |
| `HealthCheckUrl` | `string` (required) | URL endpoint used for health checks. Must be provided at instantiation. |
| `Version` | `string` (required) | Version string of the service (e.g., `"1.0.0"`). Must be provided at instantiation. |
| `Endpoint` | `string` (required) | Primary network endpoint of the service (e.g., `"http://localhost:5000"`). Must be provided at instantiation. |
| `Status` | `ServiceStatus` | Current health status of the service. The `ServiceStatus` enum is defined elsewhere in the project. |
| `CreatedAt` | `DateTime` | Timestamp when the registration was created. |
| `UpdatedAt` | `DateTime` | Timestamp of the last modification to the registration. |
| `LastHealthCheckAt` | `DateTime?` | Timestamp of the most recent health check execution, or `null` if none has occurred. |
| `OwnerId` | `Guid` | Identifier of the user or team that owns this service. |
| `Owner` | `User?` | Navigation property to the owning `User` entity. May be `null` if not loaded. |
| `HealthCheckIntervalSeconds` | `int` | Interval in seconds between scheduled health checks. |
| `TimeoutSeconds` | `int` | Timeout in seconds for each health check request. |
| `IsEnabled` | `bool` | Whether the service is currently enabled for health monitoring and routing. |
| `ConsecutiveFailures` | `int` | Count of consecutive health check failures. |
| `TotalRequests` | `int` | Total number of health check requests made to this service. |
| `SuccessfulRequests` | `int` | Number of health check requests that returned a successful status. |
| `SystemdServiceName` | `string?` | Optional systemd unit name if the service is managed as a systemd service. |
| `HealthCheckResults` | `ICollection<HealthCheckResult>` | Collection of historical health check results. Typically used for navigation in an ORM context. |

**Throws:**  
Setting a `required` property to `null` or omitting it during object initialization will cause a compiler error (C# 11 required members). No runtime exceptions are thrown by the property setters themselves.

## Usage

### Example 1: Creating a new service registration

```csharp
var registration = new ServiceRegistration
{
    ServiceName = "order-api",
    HealthCheckUrl = "https://order-api.example.com/health",
    Version = "2.1.0",
    Endpoint = "https://order-api.example.com",
    OwnerId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
    HealthCheckIntervalSeconds = 30,
    TimeoutSeconds = 5,
    IsEnabled = true
};

// The Id and timestamps are typically set by the persistence layer.
// For manual assignment:
registration.Id = Guid.NewGuid();
registration.CreatedAt = DateTime.UtcNow;
registration.UpdatedAt = DateTime.UtcNow;
```

### Example 2: Updating health check metrics after a failure

```csharp
var registration = await dbContext.ServiceRegistrations.FindAsync(serviceId);

if (registration != null)
{
    registration.ConsecutiveFailures++;
    registration.TotalRequests++;
    registration.LastHealthCheckAt = DateTime.UtcNow;
    registration.UpdatedAt = DateTime.UtcNow;

    if (registration.ConsecutiveFailures >= 3)
    {
        registration.Status = ServiceStatus.Unhealthy;
        registration.IsEnabled = false;
    }

    await dbContext.SaveChangesAsync();
}
```

## Notes

- **Required members:** `ServiceName`, `HealthCheckUrl`, `Version`, and `Endpoint` must be provided when creating a `ServiceRegistration`. The compiler enforces this; omitting them results in a build error.
- **Nullable properties:** `Description`, `LastHealthCheckAt`, `Owner`, and `SystemdServiceName` can be `null`. Code consuming these properties should check for `null` before use, especially when accessing navigation properties like `Owner` that may not be loaded.
- **Thread safety:** This type is a plain data object (POCO) with no synchronization. It is not thread‑safe. Concurrent reads and writes from multiple threads may lead to data corruption. In a typical web application, each instance is accessed within a single request context or protected by a database transaction.
- **Collection navigation:** `HealthCheckResults` is an `ICollection<HealthCheckResult>`. When using an ORM like Entity Framework Core, this collection is often lazy‑loaded. Accessing it after the `DbContext` has been disposed will throw an `ObjectDisposedException`. Ensure the collection is eagerly loaded or the context remains alive during access.
- **Default values:** `Id` is a `Guid` and defaults to `00000000-0000-0000-0000-000000000000` if not explicitly set. `CreatedAt` and `UpdatedAt` default to `DateTime.MinValue` unless assigned. It is recommended to set these timestamps explicitly or rely on database defaults.
- **Enum `ServiceStatus`:** The exact values of `ServiceStatus` are defined elsewhere in the project. Common values include `Healthy`, `Unhealthy`, `Degraded`, and `Unknown`.
