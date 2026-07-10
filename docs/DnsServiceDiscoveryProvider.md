# DnsServiceDiscoveryProvider

The `DnsServiceDiscoveryProvider` class implements a service discovery mechanism based on standard DNS protocols, enabling applications to resolve, register, and monitor service endpoints within a network infrastructure. It provides asynchronous operations for querying service records, managing service registration lifecycle, and observing changes in service availability via streaming updates, returning standardized `Result` wrappers to handle success and failure states uniformly.

## API

### `public DnsServiceDiscoveryProvider`
Initializes a new instance of the `DnsServiceDiscoveryProvider` class. This constructor sets up the internal DNS client and configuration required to perform service discovery operations against the configured DNS servers.

### `public async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveAsync`
Asynchronously queries the DNS infrastructure to resolve a specific service name into a list of available endpoints.
*   **Parameters**: Accepts a service identifier (typically a hostname or SRV record name) defined in the implementation context.
*   **Return Value**: Returns a `Task` containing a `Result` wrapper. On success, the result contains a read-only list of `ServiceDiscoveryRecord` objects representing the resolved endpoints. On failure, the result contains error details.
*   **Exceptions**: Throws network-related exceptions if the DNS server is unreachable or if the underlying socket operations fail before a result can be encapsulated.

### `public Task<Result> RegisterAsync`
Asynchronously registers the current service instance with the DNS provider, making it discoverable by other clients.
*   **Parameters**: Accepts service metadata required for registration (e.g., service name, port, payload) as defined by the concrete implementation.
*   **Return Value**: Returns a `Task` containing a `Result`. A successful result indicates the service record has been published; a failed result indicates the registration was rejected or timed out.
*   **Exceptions**: May throw if the service configuration is invalid or if the DNS update protocol fails critically.

### `public Task<Result> DeregisterAsync`
Asynchronously removes the current service instance from the DNS provider, stopping it from being discovered by clients.
*   **Parameters**: Accepts the specific service identifier or context required to locate and remove the existing record.
*   **Return Value**: Returns a `Task` containing a `Result`. Success indicates the record was removed; failure indicates the record could not be found or the removal operation failed.
*   **Exceptions**: May throw if the connection to the DNS server is lost during the deregistration process.

### `public async IAsyncEnumerable<IReadOnlyList<ServiceDiscoveryRecord>> WatchAsync`
Provides a continuous stream of service discovery updates, yielding a new list of records whenever the state of the resolved service changes.
*   **Parameters**: Accepts a service identifier to monitor and an optional cancellation token.
*   **Return Value**: Returns an `IAsyncEnumerable` that yields `IReadOnlyList<ServiceDiscoveryRecord>` items. Each yield represents the current state of the service endpoints at the moment of change.
*   **Exceptions**: Throws if the watch stream encounters an unrecoverable error, such as a permanent loss of connectivity to the DNS server. The enumeration terminates if the provided cancellation token is triggered.

### `public async Task<bool> IsAvailableAsync`
Asynchronously checks the operational status of the DNS service discovery provider itself.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task<bool>`. `true` indicates the provider can successfully communicate with the DNS backend; `false` indicates the provider is currently unavailable or misconfigured.
*   **Exceptions**: Generally does not throw; connectivity issues are captured and returned as `false`. Critical system errors may still propagate.

## Usage

### Resolving and Watching a Service
This example demonstrates resolving a specific service endpoint and then establishing a watch to monitor for changes in availability.

```csharp
var provider = new DnsServiceDiscoveryProvider();

// Initial resolution
var resolveResult = await provider.ResolveAsync("api-gateway.internal");
if (resolveResult.IsSuccess)
{
    foreach (var record in resolveResult.Value)
    {
        Console.WriteLine($"Found endpoint: {record.Host}:{record.Port}");
    }
}

// Start watching for changes
await foreach (var records in provider.WatchAsync("api-gateway.internal"))
{
    Console.WriteLine($"Service update detected. Active endpoints: {records.Count}");
    // Update load balancer or client cache here
}
```

### Service Registration Lifecycle
This example shows how to register a service upon startup and ensure it is deregistered gracefully during shutdown.

```csharp
var provider = new DnsServiceDiscoveryProvider();

// Register the service
var registerResult = await provider.RegisterAsync(new ServiceRegistrationContext
{
    ServiceName = "worker-node-01",
    Port = 8080,
    Payload = "region=us-east-1"
});

if (!registerResult.IsSuccess)
{
    Console.Error.WriteLine($"Failed to register service: {registerResult.Error}");
    return;
}

try
{
    // Simulate service work
    await Task.Delay(TimeSpan.FromHours(1));
}
finally
{
    // Ensure deregistration on exit
    var deregisterResult = await provider.DeregisterAsync("worker-node-01");
    if (!deregisterResult.IsSuccess)
    {
        Console.Error.WriteLine("Warning: Failed to cleanly deregister service.");
    }
}
```

## Notes

*   **Thread Safety**: The `DnsServiceDiscoveryProvider` instance is designed to be thread-safe for concurrent read operations (`ResolveAsync`, `IsAvailableAsync`). However, concurrent calls to `RegisterAsync` and `DeregisterAsync` for the same service identity should be serialized by the caller to prevent race conditions in record state management.
*   **Streaming Behavior**: The `WatchAsync` method maintains a long-lived connection or polling loop. Consumers must ensure the returned `IAsyncEnumerable` is properly enumerated until completion or cancellation to avoid resource leaks. Breaking the enumeration loop without consuming the stream may leave background tasks running.
*   **Result Handling**: All mutation and query methods return a `Result` type rather than throwing exceptions for logical failures (e.g., "service not found" or "registration rejected"). Exceptions are reserved for unexpected infrastructure failures (e.g., socket errors, malformed DNS packets). Callers should inspect the `IsSuccess` property of the returned `Result` before accessing the value.
*   **Availability Checks**: `IsAvailableAsync` performs a lightweight connectivity check. A return value of `false` implies that subsequent `ResolveAsync` or `RegisterAsync` calls will likely fail, but it does not guarantee the state of specific service records.
