# HealthCheckRepositoryIntegrationTestsExtensions
The `HealthCheckRepositoryIntegrationTestsExtensions` class provides a set of extension methods for integration testing of health check repositories. It offers functionality to create and manage health check results, as well as assert their correctness. This class is designed to simplify the process of writing integration tests for health check repositories, making it easier to ensure the reliability and accuracy of health check data.

## API
* `CreateAndAddHealthCheckResultAsync`: Creates a new health check result and adds it to the repository. This method is asynchronous and returns a `HealthCheckResult` object representing the newly created result.
* `CreateMultipleHealthCheckResultsAsync`: Creates multiple health check results and adds them to the repository. This method is asynchronous and returns a list of `HealthCheckResult` objects representing the newly created results.
* `AssertHealthCheckResultMatches`: Asserts that a health check result matches the expected values. This method takes no parameters and does not return a value. It throws an exception if the assertion fails.
* `GetAllHealthCheckResultsAsync`: Retrieves all health check results from the repository. This method is asynchronous and returns a list of `HealthCheckResult` objects.
* `CountHealthCheckResultsForServiceAsync`: Retrieves the number of health check results for a given service. This method is asynchronous and returns an integer representing the count of health check results.

## Usage
```csharp
// Example 1: Creating a health check result and asserting its correctness
var healthCheckResult = await HealthCheckRepositoryIntegrationTestsExtensions.CreateAndAddHealthCheckResultAsync();
HealthCheckRepositoryIntegrationTestsExtensions.AssertHealthCheckResultMatches(healthCheckResult, "ExpectedService", "ExpectedStatus");

// Example 2: Creating multiple health check results and retrieving all results
var healthCheckResults = await HealthCheckRepositoryIntegrationTestsExtensions.CreateMultipleHealthCheckResultsAsync(5);
var allResults = await HealthCheckRepositoryIntegrationTestsExtensions.GetAllHealthCheckResultsAsync();
Assert.IsTrue(allResults.Count >= healthCheckResults.Count);
```

## Notes
The `HealthCheckRepositoryIntegrationTestsExtensions` class is designed for use in integration tests, where thread-safety is not a primary concern. However, the asynchronous nature of the methods means that they can be safely used in multi-threaded environments. It is worth noting that the `AssertHealthCheckResultMatches` method will throw an exception if the assertion fails, which can be used to fail the test. Additionally, the `CountHealthCheckResultsForServiceAsync` method may return an inaccurate count if the repository is modified concurrently, so it should be used with caution in multi-threaded environments.
