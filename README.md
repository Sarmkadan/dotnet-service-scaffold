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


## ServiceCollectionExtensions

The `ServiceCollectionExtensions` class provides extension methods for registering infrastructure and application services in the dependency injection container. It centralizes service configuration for better maintainability and consistency across the application, including application services, integration services, caching, background services, and API authentication.

### Usage Example

```csharp
using DotnetServiceScaffold.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

// Setup service collection with all required services
var services = new ServiceCollection();

// Register application services
services.AddApplicationServices();

// Register integration services for external API calls and webhooks
services.AddIntegrationServices();

// Register caching services
services.AddCachingServices();

// Register background services for periodic tasks
services.AddBackgroundServices();

// Register API key authentication and rate limiting
services.AddApiAuthentication();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Resolve registered services
var domainEventPublisher = serviceProvider.GetRequiredService<IDomainEventPublisher>();
var cacheService = serviceProvider.GetRequiredService<ICacheService>();
var externalApiClient = serviceProvider.GetRequiredService<IExternalApiClient>();
var httpClientFactory = serviceProvider.GetRequiredService<ICustomHttpClientFactory>();
```

This example demonstrates how to use the `ServiceCollectionExtensions` methods to configure the dependency injection container with all infrastructure and application services needed for a typical ASP.NET Core application.

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

## ResponseFormatterFactory

The `ResponseFormatterFactory` creates and manages response formatters for different media types, implementing the Factory pattern to decouple formatter selection from usage. It maintains a registry of available formatters (JSON, CSV) and selects the appropriate formatter based on the requested media type, falling back to JSON for unsupported types. The factory also allows registering custom formatters at runtime.

### Usage Example

```csharp
using System;
using System.Linq;
using DotnetServiceScaffold.Infrastructure.Formatting;
using DotnetServiceScaffold.Infrastructure.Integration;

// Create the factory with built-in formatters
var factory = new ResponseFormatterFactory();

// Get a formatter for a specific media type
var jsonFormatter = factory.GetFormatter("application/json");
var csvFormatter = factory.GetFormatter("text/csv");

// Check if a media type is supported
bool supportsJson = factory.IsMediaTypeSupported("application/json"); // true
bool supportsXml = factory.IsMediaTypeSupported("application/xml"); // false

// Get all supported media types
var supportedTypes = factory.GetSupportedMediaTypes().ToList();
Console.WriteLine(string.Join(", ", supportedTypes));
// Output: application/json, text/csv, application/csv

// Register a custom formatter for XML responses
factory.RegisterFormatter("application/xml", new XmlResponseFormatter());

// Now XML is supported
bool supportsXmlAfterRegistration = factory.IsMediaTypeSupported("application/xml"); // true
```

This example demonstrates creating a formatter factory, retrieving formatters for different media types, checking support for media types, and registering custom formatters.


## StructuredLoggingOptions

The `StructuredLoggingOptions` class configures the structured logging pipeline for the application. It controls application identification, enrichment with contextual information, correlation ID handling, and minimum log level filtering. These options are typically bound from the `StructuredLogging` section in `appsettings.json`.


### Usage Example

```csharp
using DotnetServiceScaffold.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configure options in appsettings.json
// {
//   "StructuredLogging": {
//     "ApplicationName": "MyProductionService",
//     "EnrichWithMachineName": true,
//     "EnrichWithEnvironment": true,
//     "EnableCorrelationId": true,
//     "CorrelationIdHeader": "X-Request-Id",
//     "EnrichWithRequestContext": true,
//     "MinimumLevel": "Debug"
//   }
// }

// Register logging services with the configured options
var services = new ServiceCollection();

services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConfiguration(configuration.GetSection("StructuredLogging"));
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// Configure structured logging options
services.Configure<StructuredLoggingOptions>(configuration.GetSection("StructuredLogging"));

var serviceProvider = services.BuildServiceProvider();

// Resolve logger and options
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var loggingOptions = serviceProvider.GetRequiredService<IOptions<StructuredLoggingOptions>>().Value;

// Use the configured logging
logger.LogInformation("Application {ApplicationName} started in {EnvironmentName}",
    loggingOptions.ApplicationName,
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development");

// Log with correlation ID (automatically added to HTTP requests if EnableCorrelationId is true)
logger.LogWarning("Request processing failed");
```

This example shows how to configure and use `StructuredLoggingOptions` to customize the logging pipeline with application-specific settings and contextual enrichment.


## ServiceMeshOptions

The `ServiceMeshOptions` class configures the service mesh sidecar proxy integration. It controls connection settings to the sidecar admin API, readiness timeouts, mesh identification, and enables/disables mesh integration. These options are typically bound from the `ServiceMesh` section in `appsettings.json`.

### Usage Example

```csharp
using DotnetServiceScaffold.Infrastructure.ServiceMesh;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure options in appsettings.json
// {
// "ServiceMesh": {
// "AdminEndpoint": "http://localhost:15000",
// "ReadinessTimeoutSeconds": 5,
// "MeshName": "istio",
// "Enabled": true
// }
// }

// Setup service collection with service mesh integration
var services = new ServiceCollection();

// Register service mesh integration with configuration
services.AddServiceMeshIntegration(configuration);

var serviceProvider = services.BuildServiceProvider();

// Resolve the sidecar proxy service
var sidecarProxy = serviceProvider.GetRequiredService<ISidecarProxyService>();

// Check if service mesh is enabled
bool isEnabled = await sidecarProxy.IsServiceMeshEnabledAsync();

// Configure the web application to use service mesh headers
var builder = WebApplication.CreateBuilder();
builder.Services.AddServiceMeshIntegration(builder.Configuration);

var app = builder.Build();

// Add service mesh header propagation middleware
app.UseServiceMeshHeaders();

// Continue with application configuration...
app.Run();
```

This example demonstrates how to configure and use `ServiceMeshOptions` to integrate with a service mesh sidecar proxy, including registering services, checking mesh availability, and enabling header propagation middleware.



## DnsServiceDiscoveryProvider

The `DnsServiceDiscoveryProvider` implements service discovery using DNS SRV records with automatic A-record fallback. It sends raw UDP DNS queries to query SRV records (not supported by `System.Net.Dns`) and falls back to standard DNS A-record lookups when SRV records are unavailable or return no results. The provider supports watching for service instance changes and automatically respects DNS TTL values for polling intervals.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup service collection with DNS service discovery
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.Configure<ServiceDiscoveryOptions>(options =>
{
    options.Dns.PreferSrvRecords = true;
    options.Dns.SearchDomain = "cluster.local";
    options.Dns.DnsServerAddress = "8.8.8.8";
    options.Dns.DefaultPort = 8080;
    options.Dns.DefaultScheme = "https";
});
services.AddSingleton<IServiceDiscoveryProvider, DnsServiceDiscoveryProvider>();

var serviceProvider = services.BuildServiceProvider();

// Resolve a service by name
var dnsProvider = serviceProvider.GetRequiredService<IServiceDiscoveryProvider>();
var resolutionResult = await dnsProvider.ResolveAsync("user-service");

if (resolutionResult.IsSuccess && resolutionResult.Value is { } records)
{
    Console.WriteLine($"Found {records.Count} instances of user-service:");
    foreach (var record in records)
    {
        var endpointUri = record.ToEndpointUri();
        Console.WriteLine($"  - {endpointUri} (Weight: {record.Weight}, Priority: {record.Priority})");
    }
}

// Watch for service instance changes (polls based on DNS TTL)
await foreach (var currentRecords in dnsProvider.WatchAsync("product-service"))
{
    Console.WriteLine($"Service instances updated: {currentRecords.Count} instances");
    foreach (var record in currentRecords)
    {
        Console.WriteLine($"  - {record.Host}:{record.Port}");
    }
}

// Check if DNS provider is available
bool isAvailable = await dnsProvider.IsAvailableAsync();
Console.WriteLine($"DNS provider available: {isAvailable}");
```

This example demonstrates configuring the DNS service discovery provider, resolving service instances by name, watching for changes, and checking provider availability. The provider automatically handles SRV record lookups with A-record fallback and respects DNS TTL values for polling intervals.


## MetricsService

The `MetricsService` provides in-process metrics collection for tracking application performance counters, gauges, and timing data. It's designed for lightweight, low-overhead metric collection within a single process and can be used to monitor application health, track request rates, measure operation durations, and expose metrics for debugging purposes. For production monitoring, integrate with Prometheus, Application Insights, or similar monitoring systems.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Metrics;
using Microsoft.Extensions.Logging;

// Create metrics service with logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var metricsService = new MetricsService(loggerFactory.CreateLogger<MetricsService>());

// Track request counts with different endpoints
metricsService.IncrementCounter("api.requests.total");
metricsService.IncrementCounter("api.requests.total", 1, new Dictionary<string, string> { { "endpoint", "/users" } });
metricsService.IncrementCounter("api.requests.total", 1, new Dictionary<string, string> { { "endpoint", "/products" } });

// Track current memory usage as a gauge
metricsService.RecordGauge("system.memory.used_mb", 1024.5);
metricsService.RecordGauge("system.memory.used_mb", 1536.2, new Dictionary<string, string> { { "process", "worker" } });

// Measure operation duration
var result = await metricsService.MeasureAsync("api.database.query.duration_ms", async () =>
{
    await Task.Delay(150); // Simulate database query
    return "query completed";
});

// Record timing directly
metricsService.RecordTiming("api.external_api.response_time_ms", 250, new Dictionary<string, string> { { "api", "payment" } });

// Get all metrics for inspection or export
var allMetrics = await metricsService.GetMetricsAsync();

// Reset metrics when needed (e.g., during application restart)
await metricsService.ResetAsync();

// Example: Monitor HTTP request handling
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MetricsService _metrics;

    public MetricsMiddleware(RequestDelegate next, MetricsService metrics)
    {
        _next = next;
        _metrics = metrics;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            _metrics.RecordTiming("http.request.duration_ms", sw.ElapsedMilliseconds,
                new Dictionary<string, string>
                {
                    { "method", context.Request.Method },
                    { "path", context.Request.Path },
                    { "status", context.Response.StatusCode.ToString() }
                });
            _metrics.IncrementCounter("http.requests.total");
        }
    }
}
```

This example demonstrates how to use `MetricsService` to track application performance metrics including counters for request counts, gauges for system resources, and timers for operation durations. The service supports tagging metrics with dimensions for detailed analysis and provides methods to retrieve and reset collected metrics.

## ISidecarProxyService

The `ISidecarProxyService` interface provides a contract for interacting with local sidecar proxy admin APIs compatible with Envoy (such as those injected by Istio, Consul Connect, or Linkerd). It exposes methods for checking proxy readiness, retrieving cluster information, managing connection draining during shutdown, and detecting whether the application is running inside a service mesh environment.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.ServiceMesh;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ServiceMeshHealthReporter
{
    private readonly ISidecarProxyService _sidecarProxy;
    private readonly ILogger<ServiceMeshHealthReporter> _logger;

    public ServiceMeshHealthReporter(
        ISidecarProxyService sidecarProxy,
        ILogger<ServiceMeshHealthReporter> logger)
    {
        _sidecarProxy = sidecarProxy;
        _logger = logger;
    }

    public async Task ReportMeshStatusAsync()
    {
        // Check if service mesh integration is enabled and ready
        bool isEnabled = await _sidecarProxy.IsServiceMeshEnabledAsync();
        
        if (!isEnabled)
        {
            _logger.LogInformation("Service mesh integration is disabled or not available");
            return;
        }

        // Get detailed proxy information
        var proxyInfo = await _sidecarProxy.GetProxyInfoAsync();
        
        _logger.LogInformation("Sidecar Proxy Status:");
        _logger.LogInformation("- Version: {Version}", proxyInfo.ProxyVersion);
        _logger.LogInformation("- Status: {Status}", proxyInfo.Status);
        _logger.LogInformation("- Mesh: {MeshName}", proxyInfo.MeshName);
        _logger.LogInformation("- Clusters: {ClusterCount}", proxyInfo.UpstreamClusters.Count);

        // Check readiness status
        bool isReady = await _sidecarProxy.CheckReadinessAsync();
        _logger.LogInformation("Proxy readiness: {IsReady}", isReady);

        // Get upstream clusters information
        var clusters = await _sidecarProxy.GetUpstreamClustersAsync();
        
        foreach (var cluster in clusters)
        {
            _logger.LogInformation(
                "Cluster: {Name} - Healthy: {Healthy}/{Total} hosts",
                cluster.Name,
                cluster.HealthyHosts,
                cluster.TotalHosts);
        }
    }

    public async Task PrepareForShutdownAsync()
    {
        // Gracefully drain connections before application shutdown
        await _sidecarProxy.DrainConnectionsAsync(drainSeconds: 15);
        _logger.LogInformation("Application shutdown sequence initiated with connection draining");
    }
}

// Example usage in DI setup
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddSidecarProxyIntegration(); // Registers ISidecarProxyService

var serviceProvider = services.BuildServiceProvider();

var reporter = serviceProvider.GetRequiredService<ServiceMeshHealthReporter>();
await reporter.ReportMeshStatusAsync();

// During graceful shutdown
// await reporter.PrepareForShutdownAsync();
```

This example demonstrates how to use `ISidecarProxyService` to integrate with a service mesh sidecar proxy for health reporting, readiness checks, cluster monitoring, and graceful shutdown procedures.



## HttpClientFactory

The `HttpClientFactory` provides a centralized way to create configured `HttpClient` instances with standardized settings for timeouts, headers, and authentication. It wraps the default `IHttpClientFactory` from .NET's dependency injection system and adds convenience methods for common HTTP client configurations including authenticated clients with API keys, Bearer tokens, and custom base URLs.

## JsonResponseFormatter

The `JsonResponseFormatter` formats objects as JSON responses with consistent formatting, null handling, and date serialization. It uses camelCase property naming, ignores null values by default, and serializes dates in ISO 8601 UTC format. The formatter supports all JSON-based media types and provides custom date serialization through a dedicated converter.

### Usage Example

```csharp
using System;
using System.Text.Json;
using DotnetServiceScaffold.Infrastructure.Formatting;

// Create the JSON formatter with default options
var formatter = new JsonResponseFormatter();

// Check if the formatter can handle a media type
bool canFormatJson = formatter.CanFormat("application/json");
bool canFormatJsonCustom = formatter.CanFormat("application/json+custom");

// Format a simple object to JSON
var user = new { Id = 1, Name = "John Doe", Email = "john@example.com" };
string json = await formatter.FormatAsync(user);
Console.WriteLine(json);
// Output: {"id":1,"name":"John Doe","email":"john@example.com"}

// Format a null value
string nullJson = await formatter.FormatAsync(null);
Console.WriteLine(nullJson); // null

// Format an object with DateTime
var order = new { 
    Id = 101, 
    CreatedAt = DateTime.UtcNow,
    Status = "Processing"
};
string orderJson = await formatter.FormatAsync(order);
Console.WriteLine(orderJson);
// Output: {"id":101,"createdAt":"2024-07-15T14:30:45.1234567Z","status":"Processing"}

// Check media type support
bool supportsJson = formatter.CanFormat("application/json");
bool supportsXml = formatter.CanFormat("application/xml");
```

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