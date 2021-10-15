# HealthCheckControllerExtensions

`HealthCheckControllerExtensions` provides asynchronous extension methods that encapsulate common health-check query patterns for ASP.NET Core controllers. These methods simplify retrieving time-bound health status, filtered historical results, comprehensive snapshots, and grouped failure information from the underlying health-check infrastructure.

## API

### CheckServiceHealthWithTimeout

```csharp
public static async Task<IActionResult> CheckServiceHealthWithTimeout(
    this ControllerBase controller,
    HealthCheckService healthCheckService,
    TimeSpan timeout)
```

Executes a full health-check evaluation and returns the aggregated report. If the operation does not complete within the specified `timeout`, the method short-circuits and returns a degraded or timeout-indicating result.

**Parameters:**
- `controller` — the extended `ControllerBase` instance.
- `healthCheckService` — the `HealthCheckService` used to run all registered checks.
- `timeout` — maximum wall-clock duration allowed for the entire check run.

**Returns:**  
An `IActionResult` containing the health report when successful, or a timeout status (typically HTTP 503 or 504) when the deadline is exceeded.

**Throws:**  
`ArgumentNullException` when `healthCheckService` is `null`.  
`OperationCanceledException` may propagate if the underlying cancellation triggers before the timeout wrapper handles it.

---

### GetHealthHistoryFiltered

```csharp
public static async Task<IActionResult> GetHealthHistoryFiltered(
    this ControllerBase controller,
    IHealthHistoryRepository repository,
    DateTimeOffset from,
    DateTimeOffset to,
    string? serviceName = null)
```

Queries the health-check history store for entries recorded within the `[from, to]` time window, optionally scoped to a single named service.

**Parameters:**
- `controller` — the extended `ControllerBase` instance.
- `repository` — the history persistence abstraction.
- `from` — inclusive start of the time range.
- `to` — inclusive end of the time range.
- `serviceName` — optional filter; when non-null, only entries for that service are returned.

**Returns:**  
An `IActionResult` with a collection of matching historical health entries, or an empty list when no records satisfy the criteria.

**Throws:**  
`ArgumentNullException` when `repository` is `null`.  
`ArgumentException` when `from` is later than `to`.

---

### GetComprehensiveHealthStatus

```csharp
public static async Task<IActionResult> GetComprehensiveHealthStatus(
    this ControllerBase controller,
    HealthCheckService healthCheckService,
    IHealthHistoryRepository repository,
    bool includeHistory = true)
```

Combines a live health-check execution with optional historical context into a single comprehensive response. When `includeHistory` is `true`, recent history is appended to the live report.

**Parameters:**
- `controller` — the extended `ControllerBase` instance.
- `healthCheckService` — the service that evaluates current health.
- `repository` — the history store queried for recent entries.
- `includeHistory` — when `true`, the response embeds a configurable window of past results.

**Returns:**  
An `IActionResult` whose payload contains both the current `HealthReport` and, if requested, a collection of historical entries.

**Throws:**  
`ArgumentNullException` when `healthCheckService` or `repository` is `null`.

---

### GetFailedChecksGrouped

```csharp
public static async Task<IActionResult> GetFailedChecksGrouped(
    this ControllerBase controller,
    HealthCheckService healthCheckService,
    string groupBy = "name")
```

Runs all health checks and returns only those that report a non-healthy status, grouped by the specified criterion. Supported grouping keys are `"name"` (check registration name) and `"tag"` (user-defined tags).

**Parameters:**
- `controller` — the extended `ControllerBase` instance.
- `healthCheckService` — the service that executes the checks.
- `groupBy` — grouping strategy; defaults to `"name"`.

**Returns:**  
An `IActionResult` containing a dictionary or structured collection where each key represents a group and the value lists the failed entries belonging to that group.

**Throws:**  
`ArgumentNullException` when `healthCheckService` is `null`.  
`ArgumentException` when `groupBy` is neither `"name"` nor `"tag"`.

---

## Usage

### Example 1: Time-Bounded Health Check in a Controller Action

```csharp
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet("timeout")]
    public async Task<IActionResult> GetWithTimeout()
    {
        // Fail fast if checks take longer than 5 seconds
        return await this.CheckServiceHealthWithTimeout(
            _healthCheckService,
            TimeSpan.FromSeconds(5));
    }
}
```

### Example 2: Filtered History and Comprehensive Status

```csharp
[ApiController]
[Route("api/health")]
public class HealthHistoryController : ControllerBase
{
    private readonly IHealthHistoryRepository _historyRepo;
    private readonly HealthCheckService _healthCheckService;

    public HealthHistoryController(
        IHealthHistoryRepository historyRepo,
        HealthCheckService healthCheckService)
    {
        _historyRepo = historyRepo;
        _healthCheckService = healthCheckService;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] string? service = null)
    {
        return await this.GetHealthHistoryFiltered(
            _historyRepo, from, to, service);
    }

    [HttpGet("comprehensive")]
    public async Task<IActionResult> GetComprehensive(
        [FromQuery] bool includeHistory = true)
    {
        return await this.GetComprehensiveHealthStatus(
            _healthCheckService, _historyRepo, includeHistory);
    }
}
```

---

## Notes

- **Timeout handling:** `CheckServiceHealthWithTimeout` uses a cooperative cancellation approach. If the underlying `HealthCheckService` does not respect cancellation tokens, the timeout may only take effect after the checks naturally complete, potentially blocking the caller longer than expected.
- **History repository consistency:** `GetHealthHistoryFiltered` and `GetComprehensiveHealthStatus` assume the repository performs stable, point-in-time reads. If the store is being written concurrently, results may reflect a slightly stale or in-flight state.
- **Grouping stability:** `GetFailedChecksGrouped` relies on the `groupBy` parameter being one of the documented values. Passing an unrecognized string throws `ArgumentException`; callers should validate or use constants.
- **Thread safety:** All methods are stateless extension methods that operate on their supplied arguments. They do not mutate shared state and are safe to invoke concurrently, provided the injected services (`HealthCheckService`, `IHealthHistoryRepository`) are themselves thread-safe.
- **Nullability:** Every method guards against null service/repository arguments with immediate `ArgumentNullException` throws. Controller extension methods do not guard against a null `controller` parameter; calling them on a null `this` reference will result in a `NullReferenceException` at the call site.
- **Return types:** The concrete `IActionResult` implementations (e.g., `OkObjectResult`, `ObjectResult` with status codes) are determined internally. Consumers should rely on the HTTP status code and body shape rather than casting to specific result types.
