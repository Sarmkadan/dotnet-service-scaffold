# HealthCheckService
The `HealthCheckService` class is designed to manage and perform health checks on a service, providing methods to execute health checks, retrieve health check history, calculate success rates, and clean up old results. This class is essential for monitoring the health and performance of a service, allowing for proactive maintenance and issue resolution.

## API
### Constructors
* `public HealthCheckService`: Initializes a new instance of the `HealthCheckService` class.

### Methods
* `public async Task<HealthCheckResult> PerformHealthCheckAsync`: Performs a health check on the service and returns the result. This method does not take any parameters and returns a `HealthCheckResult` object. It may throw exceptions if the health check fails or if there is an issue with the underlying service.
* `public async Task<IEnumerable<HealthCheckResult>> GetServiceHealthHistoryAsync`: Retrieves the health check history for the service. This method does not take any parameters and returns a collection of `HealthCheckResult` objects. It may throw exceptions if there is an issue with retrieving the history.
* `public async Task<decimal> GetServiceSuccessRateAsync`: Calculates the success rate of the service based on the health check history. This method does not take any parameters and returns a decimal value representing the success rate. It may throw exceptions if there is an issue with calculating the success rate.
* `public async Task<string> GetServiceHealthStatusAsync`: Retrieves the current health status of the service. This method does not take any parameters and returns a string representing the health status. It may throw exceptions if there is an issue with retrieving the health status.
* `public async Task<IEnumerable<HealthCheckResult>> GetFailedChecksAsync`: Retrieves a collection of failed health checks for the service. This method does not take any parameters and returns a collection of `HealthCheckResult` objects. It may throw exceptions if there is an issue with retrieving the failed checks.
* `public async Task CleanupOldResultsAsync`: Cleans up old health check results for the service. This method does not take any parameters and does not return a value. It may throw exceptions if there is an issue with cleaning up the results.
* `public async Task<HealthCheckResult> CreateHealthCheckResultAsync`: Creates a new health check result for the service. This method does not take any parameters and returns a `HealthCheckResult` object. It may throw exceptions if there is an issue with creating the result.

## Usage
The following examples demonstrate how to use the `HealthCheckService` class:
```csharp
// Example 1: Performing a health check and retrieving the result
var healthCheckService = new HealthCheckService();
var result = await healthCheckService.PerformHealthCheckAsync();
Console.WriteLine($"Health check result: {result.Status}");

// Example 2: Retrieving the health check history and calculating the success rate
var healthCheckService = new HealthCheckService();
var history = await healthCheckService.GetServiceHealthHistoryAsync();
var successRate = await healthCheckService.GetServiceSuccessRateAsync();
Console.WriteLine($"Health check history: {history.Count()} results");
Console.WriteLine($"Success rate: {successRate:P2}");
```

## Notes
The `HealthCheckService` class is designed to be thread-safe, allowing multiple threads to access and use the class concurrently. However, it is essential to note that the `PerformHealthCheckAsync` method may throw exceptions if the health check fails or if there is an issue with the underlying service. Additionally, the `CleanupOldResultsAsync` method may throw exceptions if there is an issue with cleaning up the results. It is recommended to handle these exceptions properly to ensure the reliability and stability of the service.
