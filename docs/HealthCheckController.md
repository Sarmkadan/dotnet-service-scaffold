# HealthCheckController

The `HealthCheckController` exposes endpoints that allow consumers to query the current health status of the service, retrieve a time-windowed history of health evaluations, and inspect details of any failing checks. It serves as the primary HTTP surface for the application’s built-in health monitoring infrastructure.

## API

### `HealthCheckController`

Constructor. Initializes a new instance of the controller, typically resolved by the dependency injection container with any required health-check services or background storage mechanisms already registered.

- **Parameters:** None exposed publicly; dependencies are injected.
- **Return value:** A new `HealthCheckController` instance.
- **Exceptions:** May throw if required dependencies cannot be resolved (standard DI behaviour).

### `public async Task<IActionResult> CheckServiceHealth()`

Triggers an on-demand evaluation of all registered health checks and persists the resulting snapshot for later retrieval via the history and status endpoints.

- **Parameters:** None.
- **Return value:** An `IActionResult` representing the HTTP response. Commonly returns `200 OK` with a payload describing the overall health conclusion (e.g., `Healthy`, `Degraded`, `Unhealthy`) and per-check details, or `503 Service Unavailable` when the aggregate status is unhealthy.
- **Exceptions:** Propagates unhandled exceptions from individual health checks wrapped in an appropriate HTTP error response (typically `500 Internal Server Error`).

### `public async Task<IActionResult> GetHealthHistory()`

Returns a collection of recent health-check snapshots captured over a predefined time window. The retention period and maximum entry count are controlled by service configuration.

- **Parameters:** None.
- **Return value:** An `IActionResult` containing a list of historical health records, each timestamped and including the overall status and per-check outcomes at that point in time. Returns `200 OK` with the collection (which may be empty if no snapshots exist yet).
- **Exceptions:** Returns `500 Internal Server Error` if the underlying history store is unreachable or corrupted.

### `public async Task<IActionResult> GetHealthStatus()`

Retrieves the most recently recorded health snapshot without triggering a new evaluation. This is a low-cost read of the cached state.

- **Parameters:** None.
- **Return value:** An `IActionResult` with the latest health snapshot. Typically `200 OK` when a snapshot exists, or `204 No Content` / `404 Not Found` if no evaluation has been performed since the service started.
- **Exceptions:** Returns `500 Internal Server Error` on storage access failures.

### `public async Task<IActionResult> GetFailedChecks()`

Filters the most recent health snapshot to return only the checks that did not pass (status `Unhealthy` or `Degraded`, depending on the check’s severity classification).

- **Parameters:** None.
- **Return value:** An `IActionResult` containing a subset of the latest snapshot’s entries where the check outcome is considered failing. Returns `200 OK` with the filtered list (empty if all checks pass), or `204 No Content` / `404 Not Found` when no snapshot is available.
- **Exceptions:** Returns `500 Internal Server Error` if the underlying data source throws.

## Usage

**Example 1: Polling the current health status from a monitoring dashboard**

```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
var response = await client.GetAsync("/health/status");

if (response.IsSuccessStatusCode)
{
    var json = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Current health: {json}");
}
else if (response.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine("No health snapshot available yet.");
}
```

**Example 2: Forcing a fresh evaluation and inspecting only failures**

```csharp
// Trigger a new health check run
var checkResponse = await client.PostAsync("/health/check", null);
checkResponse.EnsureSuccessStatusCode();

// Retrieve only the checks that are currently failing
var failedResponse = await client.GetAsync("/health/failed");
if (failedResponse.IsSuccessStatusCode)
{
    var failedJson = await failedResponse.Content.ReadAsStringAsync();
    if (string.IsNullOrWhiteSpace(failedJson) || failedJson == "[]")
    {
        Console.WriteLine("All checks passed.");
    }
    else
    {
        Console.WriteLine($"Failing checks: {failedJson}");
    }
}
```

## Notes

- `GetHealthStatus` and `GetFailedChecks` depend on a prior evaluation having been performed, either by a background scheduled job or an explicit call to `CheckServiceHealth`. If no snapshot exists, callers should handle `204` or `404` responses gracefully.
- `CheckServiceHealth` executes all registered checks synchronously within the request scope. Under high load, frequent calls may increase latency and resource consumption; prefer relying on background evaluation and using the read-only endpoints for routine monitoring.
- `GetHealthHistory` returns a bounded window of snapshots. Older entries are evicted based on configuration (time- or count-based limits). Callers should not assume infinite retention.
- All public methods are asynchronous and return `Task<IActionResult>`. They are designed to be thread-safe with respect to internal state because the underlying health store is expected to use concurrent or immutable data structures. No explicit locking is performed at the controller level.
- Exceptions from individual health checks do not cause the controller to throw directly; they are captured in the snapshot as failing check entries. Only infrastructure-level failures (e.g., storage unavailability) result in unhandled exceptions surfacing as `500` responses.
