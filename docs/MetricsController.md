# MetricsController

The `MetricsController` is an ASP.NET Core API controller that exposes endpoints for querying and resetting application metrics. It maintains a snapshot of metric data as instance properties, allowing callers to retrieve the current count of total metrics, counters, gauges, timers, and the list of categories. The controller is intended to be used in a web application where metrics are collected and served via HTTP.

## API

### `public MetricsController()`

Initializes a new instance of the `MetricsController` class. The constructor sets default values for the metric properties (typically zero or empty collections). No parameters are required.

### `public async Task<IActionResult> GetMetrics()`

Returns a complete snapshot of all current metrics.

- **Parameters**: None.
- **Returns**: An `IActionResult` containing a JSON object with the current values of `Timestamp`, `TotalMetrics`, `Counters`, `Gauges`, `Timers`, and `Categories`.
- **Throws**: `InvalidOperationException` if the underlying metrics store is unavailable or corrupted.

### `public async Task<IActionResult> GetMetricsByCategory()`

Returns metrics organized by category. The response groups the available metrics under their respective category names.

- **Parameters**: None.
- **Returns**: An `IActionResult` containing a JSON object where each key is a category name and the value is the metric data for that category.
- **Throws**: `InvalidOperationException` if the metrics store cannot be read.

### `public async Task<IActionResult> ResetMetrics()`

Resets all metrics to their initial default values (zero counts, empty categories, and the current timestamp).

- **Parameters**: None.
- **Returns**: An `IActionResult` with a success status (e.g., `200 OK`) after the reset completes.
- **Throws**: `InvalidOperationException` if the metrics store cannot be cleared.

### `public async Task<IActionResult> GetMetricsSummary()`

Returns a summary of the current metrics, typically including totals and aggregated values.

- **Parameters**: None.
- **Returns**: An `IActionResult` containing a JSON object with summary fields (e.g., total count, average, min/max if applicable).
- **Throws**: `InvalidOperationException` if the metrics store is inaccessible.

### `public DateTime Timestamp`

Gets or sets the timestamp of the last metrics snapshot. This property is updated whenever metrics are collected or reset.

- **Type**: `DateTime`
- **Default**: `DateTime.UtcNow` at controller instantiation.
- **Does not throw**.

### `public int TotalMetrics`

Gets or sets the total number of metric entries currently tracked.

- **Type**: `int`
- **Default**: `0`
- **Does not throw**.

### `public int Counters`

Gets or sets the number of counter-type metrics.

- **Type**: `int`
- **Default**: `0`
- **Does not throw**.

### `public int Gauges`

Gets or sets the number of gauge-type metrics.

- **Type**: `int`
- **Default**: `0`
- **Does not throw**.

### `public int Timers`

Gets or sets the number of timer-type metrics.

- **Type**: `int`
- **Default**: `0`
- **Does not throw**.

### `public List<string> Categories`

Gets or sets the list of metric category names currently in use.

- **Type**: `List<string>`
- **Default**: An empty list.
- **Does not throw**.

## Usage

The following examples assume the controller is mapped to the route prefix `api/metrics` via attribute routing or endpoint configuration.

### Example 1: Retrieving and resetting metrics via HTTP

```csharp
// Using HttpClient to interact with the MetricsController endpoints
using var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

// Get all metrics
var metricsResponse = await client.GetAsync("/api/metrics");
var metrics = await metricsResponse.Content.ReadAsStringAsync();
Console.WriteLine(metrics);

// Get metrics by category
var categoryResponse = await client.GetAsync("/api/metrics/category");
var categoryMetrics = await categoryResponse.Content.ReadAsStringAsync();
Console.WriteLine(categoryMetrics);

// Get summary
var summaryResponse = await client.GetAsync("/api/metrics/summary");
var summary = await summaryResponse.Content.ReadAsStringAsync();
Console.WriteLine(summary);

// Reset metrics
var resetResponse = await client.PostAsync("/api/metrics/reset", null);
resetResponse.EnsureSuccessStatusCode();
```

### Example 2: Direct invocation in a unit test

```csharp
[Fact]
public async Task GetMetrics_ReturnsCurrentSnapshot()
{
    // Arrange
    var controller = new MetricsController
    {
        Timestamp = DateTime.UtcNow,
        TotalMetrics = 10,
        Counters = 4,
        Gauges = 3,
        Timers = 3,
        Categories = new List<string> { "http", "database" }
    };

    // Act
    var result = await controller.GetMetrics();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    var data = okResult.Value as dynamic;
    Assert.Equal(10, data.TotalMetrics);
}
```

## Notes

- **Thread safety**: The public properties (`Timestamp`, `TotalMetrics`, `Counters`, `Gauges`, `Timers`, `Categories`) are not thread-safe. Concurrent reads and writes from multiple requests can lead to inconsistent state. If the controller is registered as a singleton, external synchronization (e.g., locks or an immutable snapshot pattern) should be applied. For most scenarios, register the controller as scoped or transient to avoid shared state across requests.
- **Empty state**: When no metrics have been collected, `TotalMetrics`, `Counters`, `Gauges`, and `Timers` are zero, and `Categories` is an empty list. The `GetMetrics` and `GetMetricsSummary` endpoints will return valid JSON with these default values.
- **Reset behavior**: Calling `ResetMetrics` sets all numeric properties to zero, clears the `Categories` list, and updates `Timestamp` to the current UTC time. Any in-flight reads that started before the reset may still see the old values.
- **Timestamp precision**: The `Timestamp` property uses `DateTime` with default precision. For high-frequency metric collection, consider using `DateTime.UtcNow` with higher resolution or a custom timestamp provider.
- **Error handling**: The async methods may throw `InvalidOperationException` if the internal metrics store (if any) fails. In a typical implementation, the controller does not catch these exceptions; they propagate to the ASP.NET Core exception handler.
