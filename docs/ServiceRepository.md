# ServiceRepository

`ServiceRepository` is the Entity Framework Core repository for
`ServiceRegistration` entities. It extends `Repository<ServiceRegistration>` and
implements `IServiceRepository`.

## Responsibilities

- Query service registrations by name, status, owner, enabled state, and health
  state.
- Load a registration together with a limited number of its newest metrics.
- Identify enabled registrations whose health checks are missing or stale.
- Provide the generic repository operations inherited from
  `Repository<ServiceRegistration>` for reading and persisting registrations.
- Log repository operations through `ILogger<ServiceRepository>`.

Queries return tracked entities because the repository does not use
`AsNoTracking`. Changes made to returned entities can therefore be persisted by
calling `SaveChangesAsync`. A repository instance should normally have a scoped
lifetime: its `ServiceScaffoldDbContext` is not thread-safe and must not be used
concurrently.

## Construction

```csharp
public ServiceRepository(
    ServiceScaffoldDbContext context,
    ILogger<ServiceRepository> logger)
```

The context supplies the `DbSet<ServiceRegistration>`, and the logger records
query and persistence activity. Both dependencies are required.

## Service-specific methods

| Method | Result and behavior |
| --- | --- |
| `GetByNameAsync(string serviceName, CancellationToken cancellationToken = default)` | Returns the first registration whose `ServiceName` exactly matches `serviceName`, or `null`. A null or empty name throws `ArgumentException`. |
| `GetByStatusAsync(ServiceStatus status, CancellationToken cancellationToken = default)` | Returns registrations with the requested status, ordered by `ServiceName`. |
| `GetEnabledServicesAsync(CancellationToken cancellationToken = default)` | Returns registrations whose `IsEnabled` value is `true`, ordered by `ServiceName`. |
| `GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)` | Returns registrations with the requested `OwnerId`, ordered by `ServiceName`. |
| `GetWithMetricsAsync(Guid serviceId, int metricsCount = 10, CancellationToken cancellationToken = default)` | Returns the matching registration, or `null`, and includes at most `metricsCount` metrics ordered from newest to oldest by `RecordedAt`. |
| `GetUnhealthyServicesAsync()` | Returns enabled registrations in either `Unhealthy` or `Degraded` status, ordered by `UpdatedAt` descending. |
| `GetServicesWithoutRecentHealthCheckAsync(int minutesThreshold = 5)` | Returns enabled registrations whose `LastHealthCheckAt` is null or older than `DateTime.UtcNow.AddMinutes(-minutesThreshold)`, ordered by `LastHealthCheckAt`. |

All methods execute their queries asynchronously. Methods that accept a
`CancellationToken` check it before querying and pass it to Entity Framework
Core. The two health-oriented methods do not currently accept a cancellation
token.

## Inherited methods

The following public operations come from `Repository<ServiceRegistration>`:

| Method | Purpose |
| --- | --- |
| `GetByIdAsync(Guid id, CancellationToken cancellationToken = default)` | Finds a registration by primary key, returning `null` when it does not exist. |
| `GetAllAsync(CancellationToken cancellationToken = default)` | Returns all registrations. |
| `AddAsync(ServiceRegistration entity, CancellationToken cancellationToken = default)` | Adds the registration and immediately saves the context. |
| `UpdateAsync(ServiceRegistration entity, CancellationToken cancellationToken = default)` | Marks the registration as updated and immediately saves the context. |
| `DeleteAsync(Guid id)` | Deletes the registration when found and immediately saves the context. |
| `ExistsAsync(Guid id, CancellationToken cancellationToken = default)` | Reports whether a registration with the primary key exists. |
| `SaveChangesAsync()` | Persists pending changes in the context. |

The inherited generic operations wrap data-access failures in
`DataAccessException`. Service-specific query methods let Entity Framework Core
query exceptions propagate directly.

## Example usage

Register the repository with the same scoped lifetime as its database context:

```csharp
using DotnetServiceScaffold.Infrastructure.Data.Repository;

services.AddScoped<IServiceRepository, ServiceRepository>();
```

Inject `IServiceRepository` into an application service and pass through the
caller's cancellation token:

```csharp
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Infrastructure.Data.Repository;

public sealed class ServiceHealthReport
{
    private readonly IServiceRepository _services;

    public ServiceHealthReport(IServiceRepository services)
    {
        _services = services;
    }

    public async Task<IReadOnlyList<string>> GetAffectedServiceNamesAsync(
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        var ownedServices = await _services.GetByOwnerAsync(
            ownerId,
            cancellationToken);

        return ownedServices
            .Where(service => service.Status is ServiceStatus.Degraded
                or ServiceStatus.Unhealthy)
            .Select(service => service.ServiceName)
            .ToList();
    }
}
```

To retrieve a registration with its five newest metrics:

```csharp
var registration = await repository.GetWithMetricsAsync(
    serviceId,
    metricsCount: 5,
    cancellationToken);

if (registration is null)
{
    return;
}

foreach (var metric in registration.Metrics)
{
    Console.WriteLine($"{metric.RecordedAt:u}");
}
```

The repository does not validate `ownerId`, `serviceId`, `metricsCount`, or
`minutesThreshold`. Callers should validate those values when their application
requires stricter constraints.
