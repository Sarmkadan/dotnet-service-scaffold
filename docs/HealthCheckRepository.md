# HealthCheckRepository
The `HealthCheckRepository` class is designed to manage and retrieve health check results for services, providing a centralized repository for health check data. It encapsulates the data access logic, allowing for efficient retrieval and manipulation of health check results.

## API
The `HealthCheckRepository` class provides the following public members:
* `HealthCheckRepository(ServiceScaffoldDbContext context, ILogger<HealthCheckRepository> logger)`: Constructs a new instance of the `HealthCheckRepository` class, taking a `ServiceScaffoldDbContext` instance and an `ILogger<HealthCheckRepository>` instance as parameters.
* `GetByServiceIdAsync`: Retrieves a collection of health check results for a specific service identifier. Returns an `IEnumerable<HealthCheckResult>`.
* `GetRecentResultsAsync`: Retrieves a collection of recent health check results. Returns an `IEnumerable<HealthCheckResult>`.
* `GetLatestResultAsync`: Retrieves the latest health check result. Returns a `HealthCheckResult` instance, or `null` if no results are found.
* `GetFailedResultsAsync`: Retrieves a collection of failed health check results. Returns an `IEnumerable<HealthCheckResult>`.
* `GetAverageResponseTimeAsync`: Calculates the average response time of health checks. Returns a `decimal` value.
* `GetFailureCountAsync`: Retrieves the number of failed health checks. Returns an `int` value.
* `DeleteOldResultsAsync`: Deletes old health check results.

## Usage
The following examples demonstrate how to use the `HealthCheckRepository` class:
```csharp
// Example 1: Retrieving health check results for a specific service
var context = new ServiceScaffoldDbContext();
var logger = new LoggerFactory().CreateLogger<HealthCheckRepository>();
var repository = new HealthCheckRepository(context, logger);
var results = await repository.GetByServiceIdAsync("service-123");
foreach (var result in results)
{
    Console.WriteLine($"Service: {result.ServiceId}, Status: {result.Status}");
}

// Example 2: Calculating average response time and failure count
var averageResponseTime = await repository.GetAverageResponseTimeAsync();
var failureCount = await repository.GetFailureCountAsync();
Console.WriteLine($"Average Response Time: {averageResponseTime}ms, Failure Count: {failureCount}");
```

## Notes
When using the `HealthCheckRepository` class, consider the following:
* The `GetByServiceIdAsync`, `GetRecentResultsAsync`, `GetFailedResultsAsync` methods may return an empty collection if no results are found.
* The `GetLatestResultAsync` method may return `null` if no results are found.
* The `GetAverageResponseTimeAsync` and `GetFailureCountAsync` methods may throw exceptions if the underlying data storage is unavailable or corrupted.
* The `DeleteOldResultsAsync` method may throw exceptions if the underlying data storage is unavailable or corrupted.
* The `HealthCheckRepository` class is designed to be thread-safe, allowing for concurrent access to health check results. However, it is still important to follow standard concurrency guidelines when using the class in a multi-threaded environment.
