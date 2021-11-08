// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

# Service Scaffold

## ResultExtensions

The `ResultExtensions` class provides utility methods for working with `Result` and `Result<T>` types, enabling operation chaining, result aggregation, and error handling. These extensions simplify common patterns like transforming successful results, combining multiple results, extracting values safely, and validating conditions.

### Usage Examples

```csharp
// Chain synchronous operations on successful results
var result = Result.Success()
    .Then<int>(_ => 42)
    .Then(value => value * 2);

// Chain asynchronous operations on successful results
var asyncResult = Result.Success()
    .ThenAsync(async _ => 
    {
        await Task.Delay(10);
        return "processed";
    });

// Convert non-generic Result to generic Result<T>
var genericResult = Result.Success().ToGeneric<string>();

// Combine multiple results into a single aggregated result
var combined = Result.Combine(
    Result.Success(),
    Result.Failure("Error 1"),
    Result.Failure("Error 2")
);

// Add validation to a successful result
var validated = Result.Success(25)
    .Also(value => 
    {
        if (value <= 0) 
            return Result.Failure("Value must be positive");
        return Result.Success();
    });

// Extract value or use a default on failure
var valueOrDefault = Result.Failure<int>("Invalid").GetValueOrDefault(0);

// Extract value or throw on failure
try 
{
    var value = Result.Success(42).GetValueOrThrow();
}
catch (Exception ex) 
{
    // Handle exception
}

// Get error details from a failed result
var (errorMessage, errorCode) = Result.Failure("Invalid", "ERR001").GetError();

// Create result based on a condition
var conditionResult = Result.FromCondition(
    42 > 20, 
    "Value must be greater than 20", 
    "VAL001"
);
```

These extensions provide a fluent API for handling success/failure scenarios while maintaining strong type safety and avoiding boilerplate error-checking code.

## ServiceConfigurationExtensions

The `ServiceConfigurationExtensions` class provides helper methods for retrieving and updating service configuration values with type safety and validation. It includes methods for common data types like `double`, `decimal`, `DateTime`, and `Guid`, as well as utilities for checking system configuration flags and updating values conditionally.

### Usage Example

```csharp
var config = GetServiceConfiguration(); // Assume this retrieves a ServiceConfiguration instance

// Retrieve a string value with a default
var apiKey = config.GetValueOrDefault("API_KEY", "default123");

// Check if this is a system-level configuration
if (config.IsSystemConfiguration())
{
    // Safely retrieve an enum value
    var mode = config.GetEnumValue<EnvironmentMode>("ENV_MODE");
    
    // Update a numeric value only if it has changed
    config.UpdateValueIfChanged("MAX_RETRIES", 5);
}
else
{
    // Retrieve a Guid value
    var serviceId = config.GetGuidValue("SERVICE_ID");
    
    // Get a decimal value for a timeout setting
    var timeout = config.GetDecimalValue("REQUEST_TIMEOUT");
}
```

This example demonstrates retrieving configuration values of different types, checking system configuration status, and conditionally updating values while ensuring type safety.

## HealthCheckRepositoryIntegrationTestsExtensions

`HealthCheckRepositoryIntegrationTestsExtensions` supplies a set of helper methods that make integration testing of the health‑check repository straightforward. The methods allow you to create single or multiple `HealthCheckResult` entries, retrieve them, count results for a specific service, and assert that a result matches expected values, all with async support.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Assume `repository` is an instance of the health‑check repository used in tests.
var repository = GetHealthCheckRepository(); // placeholder for test setup
var serviceId = Guid.NewGuid();

// Create and add a single health‑check result
var singleResult = await HealthCheckRepositoryIntegrationTestsExtensions
    .CreateAndAddHealthCheckResultAsync(repository, serviceId);

// Create several health‑check results at once
var multipleResults = await HealthCheckRepositoryIntegrationTestsExtensions
    .CreateMultipleHealthCheckResultsAsync(repository, serviceId, count: 3);

// Verify that the created result matches the expected service identifier
HealthCheckRepositoryIntegrationTestsExtensions.AssertHealthCheckResultMatches(singleResult, serviceId);

// Retrieve all health‑check results from the repository
List<HealthCheckResult> allResults = await HealthCheckRepositoryIntegrationTestsExtensions
    .GetAllHealthCheckResultsAsync(repository);

// Count how many results exist for a particular service
int resultCount = await HealthCheckRepositoryIntegrationTestsExtensions
    .CountHealthCheckResultsForServiceAsync(repository, serviceId);
```

These extension methods streamline the arrangement, execution, and verification phases of integration tests that involve health‑check data.

## DatabaseBenchmarks

The `DatabaseBenchmarks` class provides performance benchmarks for database operations. It measures the execution time of CRUD operations and query performance for common database interactions.

### Usage Example

```csharp
var benchmarks = new DatabaseBenchmarks();

await benchmarks.Setup();

// Run benchmarks
await benchmarks.CreateUser();
await benchmarks.ReadUserByEmail();
await benchmarks.UpdateUser();
await benchmarks.DeleteUser();
await benchmarks.CreateService();
await benchmarks.ListServices();
await benchmarks.BulkCreateUsers();
await benchmarks.TransactionCommit();

await benchmarks.Cleanup();
```

This example demonstrates how to use the `DatabaseBenchmarks` class to measure the performance of database operations.

## MetricsBenchmarks

The `MetricsBenchmarks` class provides performance benchmarks for in-process metric collection. It measures the overhead of incrementing counters, recording timings, and retrieving metrics.

### Usage Example

```csharp
var metrics = new MetricsService(NullLogger<MetricsService>.Instance);

// Pre-populate some counters
for (int i = 0; i < 50; i++)
{
    metrics.IncrementCounter("requests.total");
    metrics.RecordTiming("request.duration_ms", 10 + i % 200);
    metrics.RecordGauge("memory.mb", 128 + i * 0.5);
}

metrics.IncrementCounterNoTags();
metrics.IncrementCounterOneTag();
metrics.IncrementCounterThreeTags();
metrics.RecordTimingNoTags();
metrics.RecordTimingThreeTags();
metrics.RecordGauge();
var metricsSnapshot = metrics.GetMetricsAsync().Result;
```

This example demonstrates how to use the `MetricsBenchmarks` class to measure the performance of metric collection.

## CacheBenchmarks

`CacheBenchmarks` contains a set of BenchmarkDotNet benchmarks that also expose their public members for ad‑hoc usage. It demonstrates typical in‑memory cache operations such as reading, writing, checking existence, and the get‑or‑set pattern against an `InMemoryCacheService`.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Benchmarks;

public static async Task Main()
{
    var benchmarks = new CacheBenchmarks();

    // Initialise the in‑memory cache with some data
    await benchmarks.Setup();

    // Cache hit – should return the pre‑populated list
    var hitResult = await benchmarks.CacheHit();
    Console.WriteLine($"Cache hit returned {hitResult?.Services?.Count ?? 0} services.");

    // Cache miss – returns null
    var missResult = await benchmarks.CacheMiss();
    Console.WriteLine($"Cache miss returned {(missResult == null ? "null" : "data")}.");

    // Write a new entry
    await benchmarks.CacheSet();

    // Check if a key exists
    bool exists = await benchmarks.Exists();
    Console.WriteLine($"Key 'services:all' exists: {exists}");

    // GetOrSet hot path (cache hit, factory not invoked)
    var hotResult = await benchmarks.GetOrSetHit();

    // GetOrSet cold path (cache miss, factory invoked)
    var coldResult = await benchmarks.GetOrSetMiss();

    // Example of iterating over the cached services
    if (hitResult?.Services != null)
    {
        foreach (var svc in hitResult.Services)
        {
            Console.WriteLine($"{svc.Id} - {svc.Name} (Healthy: {svc.IsHealthy})");
        }
    }

    // Clean up resources
    benchmarks.Cleanup();
}
```

The example uses the real public members of `CacheBenchmarks` (`Setup`, `Cleanup`, `CacheHit`, `CacheMiss`, `CacheSet`, `Exists`, `GetOrSetHit`, `GetOrSetMiss`) and the `CachedService` properties (`Id`, `Name`, `IsHealthy`) to illustrate typical cache interactions.


## ExternalApiClient

The `ExternalApiClient` is a generic HTTP client for calling external APIs. It handles JSON serialization, error responses, and provides a clean interface for common HTTP operations (GET, POST, PUT, DELETE) with built-in logging and validation. The client automatically deserializes responses into the requested type and throws appropriate exceptions for failed requests.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Integration;

public class ApiService
{
    private readonly IExternalApiClient _apiClient;

    public ApiService(IExternalApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<User?> GetUserAsync(Guid userId)
    {
        var url = "https://api.example.com/users/{userId}";
        return await _apiClient.GetAsync<User>(url);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var url = "https://api.example.com/products";
        return await _apiClient.PostAsync<Product>(url, product);
    }

    public async Task<Product> UpdateProductAsync(Guid productId, Product product)
    {
        var url = "https://api.example.com/products/{productId}";
        return await _apiClient.PutAsync<Product>(url, product);
    }

    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        var url = "https://api.example.com/products/{productId}";
        return await _apiClient.DeleteAsync(url);
    }
}

// Example usage
var apiClient = new ExternalApiClient(httpClientFactory, logger);
var user = await apiClient.GetAsync<User>("https://api.example.com/users/123");
var createdProduct = await apiClient.PostAsync<Product>(
    "https://api.example.com/products",
    new { Name = "New Product", Price = 99.99 }
);
var updated = await apiClient.PutAsync<Product>(
    "https://api.example.com/products/456",
    new { Name = "Updated Product", Price = 129.99 }
);
var deleted = await apiClient.DeleteAsync("https://api.example.com/products/789");
```

This example demonstrates how to use the `ExternalApiClient` to perform common CRUD operations against external APIs with proper type safety and error handling.


## HttpClientFactory

The `HttpClientFactory` provides a centralized way to create configured `HttpClient` instances with standardized settings for timeouts, headers, and authentication. It wraps the default `IHttpClientFactory` from .NET's dependency injection system and adds convenience methods for common HTTP client configurations including authenticated clients with API keys, Bearer tokens, and custom base URLs.

### Usage Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Integration;
using Microsoft.Extensions.DependencyInjection;

// Setup in DI container
var services = new ServiceCollection();
services.AddHttpClient();
services.AddSingleton<ICustomHttpClientFactory, HttpClientFactory>();
var serviceProvider = services.BuildServiceProvider();

var factory = serviceProvider.GetRequiredService<ICustomHttpClientFactory>();

// Create a basic HTTP client with default configuration
var defaultClient = factory.CreateClient();

// Create an authenticated client with API key
var apiKeyClient = factory.CreateAuthenticatedClient("your-api-key-here");

// Create a Bearer token authenticated client
var bearerClient = factory.CreateBearerClient("your-bearer-token-here");

// Create a client with custom base URL
var customBaseClient = factory.CreateClientWithBaseUrl("https://api.example.com/v1");

// Use the clients for HTTP requests
var response = await defaultClient.GetAsync("/users/123");
var authResponse = await apiKeyClient.GetAsync("/products");
var bearerResponse = await bearerClient.PostAsync("/orders", new StringContent("{}"));
```

This example shows how to configure and use the `HttpClientFactory` to create different types of HTTP clients for various integration scenarios.

## StringBenchmarks

The `StringBenchmarks` class provides performance benchmarks for common string manipulation operations used throughout the application. It measures the execution time of case conversion, slug generation, sensitive data masking, and random string generation utilities that run on every request.

### Usage Example

```csharp
using DotnetServiceScaffold.Benchmarks;

var benchmarks = new StringBenchmarks();

// Convert camelCase to snake_case
string snakeCase = benchmarks.ToSnakeCase();
Console.WriteLine(snakeCase); // "user_account_service_manager"

// Convert PascalCase to snake_case
string pascalSnake = benchmarks.ToSnakeCasePascal();
Console.WriteLine(pascalSnake); // "user_account_service_manager"

// Convert snake_case to camelCase
string camelCase = benchmarks.ToCamelCase();
Console.WriteLine(camelCase); // "userAccountServiceManager"

// Mask sensitive data (keeps last 4 characters visible)
string maskedKey = benchmarks.MaskSensitive();
Console.WriteLine(maskedKey); // "***************beef"

// Generate random strings of different lengths
string random32 = benchmarks.GenerateRandomString32();
string random64 = benchmarks.GenerateRandomString64();
Console.WriteLine($"Random 32: {random32}");
Console.WriteLine($"Random 64: {random64}");

// Convert human-readable text to URL slug
string slug = benchmarks.ToSlug();
Console.WriteLine(slug); // "my-service-name-production-v2"

// Truncate long strings with ellipsis
string truncated = benchmarks.Truncate();
Console.WriteLine(truncated); // "My Service Name - Produc..."
```

This example demonstrates how to use the `StringBenchmarks` class to benchmark and verify the behavior of string utility methods that are commonly used in web applications for URL routing, logging, and data processing.