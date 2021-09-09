# ServiceRepository
The `ServiceRepository` class is designed to manage and retrieve service registrations from a database context. It provides various methods to fetch services based on different criteria such as name, status, owner, and health. This class is part of the `dotnet-service-scaffold` project and relies on a `ServiceScaffoldDbContext` instance and an `ILogger` for logging purposes.

## API
The `ServiceRepository` class exposes the following public members:
* `GetByNameAsync`: Retrieves a service registration by its name. Returns a `ServiceRegistration` object if found, otherwise `null`.
* `GetByStatusAsync`: Retrieves a list of service registrations based on their status. Returns an `IEnumerable` of `ServiceRegistration` objects.
* `GetEnabledServicesAsync`: Retrieves a list of enabled service registrations. Returns an `IEnumerable` of `ServiceRegistration` objects.
* `GetByOwnerAsync`: Retrieves a list of service registrations owned by a specific entity. Returns an `IEnumerable` of `ServiceRegistration` objects.
* `GetWithMetricsAsync`: Retrieves a service registration with its associated metrics. Returns a `ServiceRegistration` object if found, otherwise `null`.
* `GetUnhealthyServicesAsync`: Retrieves a list of service registrations that are currently unhealthy. Returns an `IEnumerable` of `ServiceRegistration` objects.
* `GetServicesWithoutRecentHealthCheckAsync`: Retrieves a list of service registrations that have not had a recent health check. Returns an `IEnumerable` of `ServiceRegistration` objects.

## Usage
Here are two examples of using the `ServiceRepository` class:
```csharp
// Example 1: Retrieving a service by name
var context = new ServiceScaffoldDbContext();
var logger = new LoggerFactory().CreateLogger<ServiceRepository>();
var repository = new ServiceRepository(context, logger);
var service = await repository.GetByNameAsync("MyService");
if (service != null)
{
    Console.WriteLine($"Service {service.Name} found.");
}
else
{
    Console.WriteLine("Service not found.");
}

// Example 2: Retrieving all enabled services
var context = new ServiceScaffoldDbContext();
var logger = new LoggerFactory().CreateLogger<ServiceRepository>();
var repository = new ServiceRepository(context, logger);
var enabledServices = await repository.GetEnabledServicesAsync();
foreach (var service in enabledServices)
{
    Console.WriteLine($"Enabled service: {service.Name}");
}
```

## Notes
When using the `ServiceRepository` class, note that all methods are asynchronous and may throw exceptions if the database context or logger is not properly configured. Additionally, the `GetWithMetricsAsync` method may return `null` if the service registration is not found or if there are no associated metrics. The `GetUnhealthyServicesAsync` and `GetServicesWithoutRecentHealthCheckAsync` methods rely on the health check data being up-to-date and may not reflect the current health status of the services if the data is stale. The `ServiceRepository` class is designed to be thread-safe, but it is still important to ensure that the database context and logger are properly synchronized to avoid concurrency issues.
