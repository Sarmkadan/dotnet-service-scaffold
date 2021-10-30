// ... (rest of the README.md content remains the same)

## ResponseFormatterFactoryExtensions

The `ResponseFormatterFactoryExtensions` class provides a set of extension methods for working with response formatters. These methods enable you to retrieve a formatter for a given media type, register custom formatters, and check if any media types are supported.

### Usage Examples

```csharp
// Get a formatter for a specific media type
var formatter = ResponseFormatterFactoryExtensions.GetFormatterOrDefault("application/json");

// Check if a formatter exists for a media type
var hasFormatter = ResponseFormatterFactoryExtensions.TryGetFormatter("application/json", out var formatter);

// Get a formatter, throwing if it doesn't exist
var requiredFormatter = ResponseFormatterFactoryExtensions.GetFormatterRequired("application/json");

// Register a custom formatter
ResponseFormatterFactoryExtensions.RegisterFormatter("application/custom", new CustomResponseFormatter());

// Check if any media types are supported
var areMediaTypesSupported = ResponseFormatterFactoryExtensions.AreAnyMediaTypesSupported(new[] { "application/json", "application/xml" });

// Get the default formatter
var defaultFormatter = ResponseFormatterFactoryExtensions.GetDefaultFormatter();
```

These extension methods are useful for configuring and using response formatters in your application, allowing you to handle different media types and customize the formatting of responses.

## ServiceRepositoryExtensions

`ServiceRepositoryExtensions` adds a collection of query‑focused helper methods for working with `ServiceRegistration` entities. They simplify common retrieval scenarios such as finding services by name, status, owner, or health state, and provide aggregated information like service counts per status.

### Usage Example

```csharp
// Assume an injected repository that works with ServiceRegistration entities
IRepository<ServiceRegistration> repository = /* resolved from DI */;

// Get a single service registration by its unique name
var registration = await repository.GetByNameAsync("OrderService");

// Retrieve all services that are currently in a specific status
var runningServices = await repository.GetByStatusAsync(ServiceStatus.Running);

// Get every enabled service together with its latest metrics
var enabledWithMetrics = await repository.GetEnabledServicesWithMetricsAsync();

// Find all services owned by a particular team or user
var teamServices = await repository.GetByOwnerAsync("team-alpha");

// List services that are currently unhealthy
var unhealthyServices = await repository.GetUnhealthyServicesAsync();

// Identify services that have not reported a health check within the last hour
var staleHealthServices = await repository.GetServicesWithoutRecentHealthCheckAsync(TimeSpan.FromHours(1));

// Get services that are due for a health check based on the configured schedule
var dueForCheck = await repository.GetServicesDueForHealthCheckAsync();

// Obtain a dictionary that maps each ServiceStatus to the number of services in that state
var statusCounts = await repository.GetServiceCountsByStatusAsync();
```

These extension methods enable concise, readable data‑access code when working with service registrations, reducing boilerplate and keeping query logic in a single, well‑tested place.
