# AdvancedUsage

The `AdvancedUsage` class serves as the primary entry point for retrieving detailed operational metrics within the `dotnet-service-scaffold` ecosystem. It encapsulates the logic required to aggregate runtime data and expose it through structured response objects, enabling developers to monitor service health and performance characteristics asynchronously without blocking the calling thread.

## API

### `public AdvancedUsage`
Initializes a new instance of the `AdvancedUsage` class. This constructor prepares the internal state required for metric collection and ensures that all necessary dependencies for data aggregation are resolved upon instantiation.

### `public async Task<MetricsResponse> GetServiceMetricsAsync`
Asynchronously retrieves the current service metrics.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous operation, containing a `MetricsResponse` object upon completion. The response object includes the aggregated `MetricsData`.
*   **Exceptions**: Throws an exception if the underlying metric collection subsystem is uninitialized or if a critical I/O error occurs while gathering runtime statistics.

### `public class MetricsResponse`
Represents the envelope for the result of a metrics query. This class contains the status of the request and the payload of collected data (`MetricsData`). It is designed to be immutable once populated to ensure data consistency during transmission.

### `public class MetricsData`
Encapsulates the raw statistical data points collected from the service. Instances of this class hold specific values such as execution counts, latency averages, and error rates. This class acts as the data transfer object (DTO) nested within `MetricsResponse`.

## Usage

### Basic Metrics Retrieval
The following example demonstrates how to instantiate the class and retrieve the current metrics snapshot.

```csharp
using DotNetServiceScaffold;

public async Task MonitorServiceAsync()
{
    var usage = new AdvancedUsage();
    
    try 
    {
        MetricsResponse response = await usage.GetServiceMetricsAsync();
        
        if (response.Data != null)
        {
            Console.WriteLine($"Active Requests: {response.Data.ActiveCount}");
            Console.WriteLine($"Average Latency: {response.Data.AverageLatencyMs}ms");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Failed to retrieve metrics: {ex.Message}");
    }
}
```

### Periodic Polling Pattern
This example illustrates a pattern for polling metrics at regular intervals, utilizing the asynchronous nature of the API to prevent thread pool starvation.

```csharp
using DotNetServiceScaffold;

public async Task StartMetricsLoopAsync(CancellationToken cancellationToken)
{
    var usage = new AdvancedUsage();

    while (!cancellationToken.IsCancellationRequested)
    {
        MetricsResponse response = await usage.GetServiceMetricsAsync();
        
        // Process MetricsData contained in the response
        LogMetrics(response.Data);

        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
    }
}

private void LogMetrics(MetricsData data)
{
    // Implementation of logging logic
}
```

## Notes

*   **Thread Safety**: The `GetServiceMetricsAsync` method is designed to be thread-safe and can be called concurrently from multiple threads. However, the `AdvancedUsage` instance itself should generally be treated as stateless regarding configuration; if internal state is modified in future versions, external synchronization may be required.
*   **Asynchronous Execution**: As the method returns a `Task`, callers must await the result to avoid fire-and-forget scenarios which could lead to unobserved exceptions if the metric collection fails.
*   **Data Consistency**: The `MetricsData` returned within `MetricsResponse` represents a point-in-time snapshot. Rapidly changing metrics may vary between consecutive calls.
*   **Error Handling**: Callers should implement robust try-catch blocks around `GetServiceMetricsAsync`, as transient infrastructure issues (e.g., counter lock contention or log file access issues) may result in runtime exceptions.
