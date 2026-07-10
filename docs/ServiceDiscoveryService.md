# ServiceDiscoveryService

The `ServiceDiscoveryService` provides core functionality for dynamic service location and registration within the `dotnet-service-scaffold` ecosystem. It enables applications to discover available service endpoints, select specific instances based on discovery records, and manage the lifecycle of the current service's registration within the discovery infrastructure. All operations return `Result<T>` wrappers to enforce explicit error handling without relying on exceptions for control flow, ensuring robust communication in distributed environments.

## API

### Constructors

#### `public ServiceDiscoveryService()`
Initializes a new instance of the `ServiceDiscoveryService` class. This constructor typically resolves dependencies required for backend communication (such as HTTP clients or consensus agents) via the dependency injection container.

### Instance Methods

#### `public async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> DiscoverAsync(string serviceName, CancellationToken cancellationToken = default)`
Retrieves a list of all currently registered instances for a specified service name.
*   **Parameters**:
    *   `serviceName`: The logical name of the service to discover.
    *   `cancellationToken`: A token to cancel the operation.
*   **Returns**: A `Result` containing a read-only list of `ServiceDiscoveryRecord` objects if successful; otherwise, a failed `Result` containing error details.
*   **Throws**: This method generally does not throw exceptions for business logic failures (e.g., service not found), encapsulating them in the `Result`. Network-level exceptions may still propagate if the underlying transport fails catastrophically.

#### `public async Task<Result<ServiceDiscoveryRecord>> SelectEndpointAsync(string serviceName, CancellationToken cancellationToken = default)`
Selects a single optimal endpoint for the specified service based on internal load-balancing logic or health status.
*   **Parameters**:
    *   `serviceName`: The logical name of the service to target.
    *   `cancellationToken`: A token to cancel the operation.
*   **Returns**: A `Result` containing a single `ServiceDiscoveryRecord` representing the chosen endpoint, or a failed `Result` if no healthy instances are available.
*   **Throws**: Similar to `DiscoverAsync`, logical errors are returned within the `Result`.

#### `public async Task<Result> RegisterSelfAsync(CancellationToken cancellationToken = default)`
Registers the current application instance with the service discovery backend using its configured identity and health check endpoints.
*   **Parameters**:
    *   `cancellationToken`: A token to cancel the operation.
*   **Returns**: A `Result` indicating success or failure.
*   **Throws**: May throw if the service configuration is invalid or the discovery backend is unreachable during the initial handshake.

#### `public async Task<Result> DeregisterSelfAsync(CancellationToken cancellationToken = default)`
Removes the current application instance from the service discovery registry, typically invoked during graceful shutdown.
*   **Parameters**:
    *   `cancellationToken`: A token to cancel the operation.
*   **Returns**: A `Result` indicating success or failure.
*   **Throws**: May throw if the network connection is severed before the deregistration payload can be transmitted.

#### `public async Task<Result<IReadOnlyList<string>>> GetRegisteredServicesAsync(CancellationToken cancellationToken = default)`
Retrieves a list of all unique service names currently known to the discovery system.
*   **Parameters**:
    *   `cancellationToken`: A token to cancel the operation.
*   **Returns**: A `Result` containing a list of service name strings.
*   **Throws**: Propagates only on critical transport failures.

#### `public async Task RefreshAsync(CancellationToken cancellationToken = default)`
Forces an immediate refresh of the local cache or lease renewal with the discovery backend. This is useful for reducing TTL latency in high-churn environments.
*   **Parameters**:
    *   `cancellationToken`: A token to cancel the operation.
*   **Returns**: A `Task` that completes when the refresh operation finishes. Errors are typically logged internally or reflected in subsequent `Discover` calls, depending on implementation specifics.
*   **Throws**: May throw if the refresh mechanism encounters an unrecoverable state.

#### `public async Task<Result<ServiceDiscoveryStats>> GetServiceStatsAsync(CancellationToken cancellationToken = default)`
Retrieves statistical data regarding the service discovery client's performance, including cache hit rates, last sync time, and registration status.
*   **Parameters**:
    *   `cancellationToken`: A token to cancel the operation.
*   **Returns**: A `Result` containing a `ServiceDiscoveryStats` object.
*   **Throws**: Unlikely to throw unless the stats subsystem is uninitialized.

### Static Extension Methods

#### `public static IServiceCollection AddServiceDiscovery(this IServiceCollection services, Action<ServiceDiscoveryOptions>? configure = null)`
Registers the `ServiceDiscoveryService` and its dependencies into the dependency injection container.
*   **Parameters**:
    *   `services`: The `IServiceCollection` to add services to.
    *   `configure`: An optional action to configure `ServiceDiscoveryOptions` (e.g., backend URL, polling intervals).
*   **Returns**: The modified `IServiceCollection` for chaining.
*   **Throws**: Throws `ArgumentNullException` if `services` is null.

#### `public static WebApplication UseServiceDiscovery(this WebApplication app)`
Middleware extension that initializes the service discovery lifecycle hooks for the web application, such as automatic registration on startup and deregistration on shutdown.
*   **Parameters**:
    *   `app`: The `WebApplication` instance.
*   **Returns**: The `WebApplication` instance for chaining.
*   **Throws**: Throws if the service has not been previously added via `AddServiceDiscovery`.

## Usage

### Example 1: Service Registration and Discovery
This example demonstrates how to configure the service in `Program.cs`, register the current instance, and discover downstream dependencies.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register the service discovery client
builder.Services.AddServiceDiscovery(options =>
{
    options.BackendUrl = "http://consul.local:8500";
    options.ServiceName = "order-processing-service";
});

var app = builder.Build();

// Enable automatic registration/deregistration lifecycle
app.UseServiceDiscovery();

// Example usage within a hosted service or controller
public class OrderProcessor
{
    private readonly ServiceDiscoveryService _discovery;

    public OrderProcessor(ServiceDiscoveryService discovery)
    {
        _discovery = discovery;
    }

    public async Task ProcessOrderAsync()
    {
        // Select a healthy instance of the payment service
        var endpointResult = await _discovery.SelectEndpointAsync("payment-service");
        
        if (endpointResult.IsSuccess)
        {
            var endpoint = endpointResult.Value;
            // Proceed to call endpoint.Address
        }
        else
        {
            // Handle lack of available endpoints gracefully
            Console.WriteLine($"Discovery failed: {endpointResult.Error}");
        }
    }
}
```

### Example 2: Manual Lifecycle Management and Stats
In scenarios where automatic middleware hooks are insufficient, manual control over registration and monitoring stats can be utilized.

```csharp
public class MaintenanceWorker : IHostedService
{
    private readonly ServiceDiscoveryService _discovery;
    private readonly ILogger<MaintenanceWorker> _logger;

    public MaintenanceWorker(ServiceDiscoveryService discovery, ILogger<MaintenanceWorker> logger)
    {
        _discovery = discovery;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Explicitly register if not using UseServiceDiscovery middleware
        var registerResult = await _discovery.RegisterSelfAsync(cancellationToken);
        if (!registerResult.IsSuccess)
        {
            _logger.LogError("Failed to register service: {Error}", registerResult.Error);
            return;
        }

        // Periodically refresh and log stats
        await _discovery.RefreshAsync(cancellationToken);
        
        var statsResult = await _discovery.GetServiceStatsAsync(cancellationToken);
        if (statsResult.IsSuccess)
        {
            _logger.LogInformation("Discovery Stats: CacheHits={Hits}, LastSync={Time}", 
                statsResult.Value.CacheHits, 
                statsResult.Value.LastSyncTime);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Explicit deregistration on shutdown
        await _discovery.DeregisterSelfAsync(cancellationToken);
    }
}
```

## Notes

*   **Thread Safety**: The `ServiceDiscoveryService` is designed to be thread-safe for concurrent read operations (`DiscoverAsync`, `SelectEndpointAsync`, `GetServiceStatsAsync`). However, state-modifying operations (`RegisterSelfAsync`, `DeregisterSelfAsync`, `RefreshAsync`) should ideally be serialized by the caller or the hosting environment to prevent race conditions during lease renewals or state transitions.
*   **Error Handling**: All public methods returning `Task<Result<T>>` encapsulate operational failures (e.g., 404 Not Found, 503 Service Unavailable from the backend) within the `Result` object. Consumers must check `IsSuccess` before accessing the `Value` property. Exceptions are reserved for catastrophic failures such as configuration errors, null arguments, or complete network unavailability preventing the request from being formed.
*   **Caching Behavior**: The `DiscoverAsync` and `SelectEndpointAsync` methods may return stale data depending on the configured TTL and the last execution of `RefreshAsync`. In high-churn environments, callers should consider invoking `RefreshAsync` prior to critical discovery operations if strong consistency is required.
*   **Lifecycle Dependencies**: The `UseServiceDiscovery` middleware relies on the application's `IHostApplicationLifetime` events. If `RegisterSelfAsync` is called manually, ensure `DeregisterSelfAsync` is called correspondingly to avoid orphaned entries in the service registry.
