# ServiceMetric

Represents a point-in-time snapshot of operational metrics for a registered service. Each instance captures resource utilization, throughput, error counts, and derived health indicators, enabling monitoring, anomaly detection, and historical trend analysis within the service scaffolding infrastructure.

## API

### Properties

| Member | Type | Description |
|---|---|---|
| `Id` | `Guid` | Unique identifier for this metric record. |
| `ServiceId` | `Guid` | Foreign key linking the metric to its parent service registration. |
| `Service` | `ServiceRegistration?` | Navigation property to the associated service registration. May be null if the relationship is not eagerly loaded. |
| `CpuUsagePercent` | `decimal` | Current CPU utilization expressed as a percentage (0–100). |
| `MemoryUsagePercent` | `decimal` | Current memory utilization expressed as a percentage (0–100). |
| `MemoryUsageBytes` | `long` | Absolute memory consumption in bytes. |
| `DiskUsagePercent` | `decimal` | Current disk utilization expressed as a percentage (0–100). |
| `DiskUsageBytes` | `long` | Absolute disk consumption in bytes. |
| `ActiveConnections` | `int` | Number of concurrently open connections at the time of recording. |
| `RequestsPerSecond` | `long` | Throughput measured in requests handled per second. |
| `AverageResponseTimeMs` | `decimal` | Mean response latency in milliseconds over the sampling window. |
| `TotalRequests` | `long` | Cumulative count of requests processed since the service started or counters were last reset. |
| `ErrorCount` | `int` | Number of failed requests recorded during the sampling window. |
| `RecordedAt` | `DateTime` | Timestamp indicating when this metric snapshot was captured. |
| `Notes` | `string?` | Optional human-readable annotations or context for this record. Nullable. |
| `Uptime` | `double` | Service uptime in seconds at the moment of recording. |
| `HasAnomalies` | `bool` | Flag indicating whether an anomaly detection routine has marked this record as anomalous. |

### Computed Members

| Member | Type | Description |
|---|---|---|
| `GetErrorRate` | `decimal` | Calculates the error rate as a percentage. Returns `ErrorCount / TotalRequests * 100`. If `TotalRequests` is zero, the behavior is division by zero; callers should guard against this. |
| `GetSeverityRating` | `string` | Returns a severity classification string (e.g., `"Healthy"`, `"Warning"`, `"Critical"`) derived from thresholds applied to CPU, memory, error rate, and response time. The exact thresholds are implementation-defined. |
| `FormatMetrics` | `string` | Produces a formatted human-readable summary string containing key metric values. The format is implementation-defined and intended for logging or display purposes. |

### Exceptions

- `GetErrorRate` throws `DivideByZeroException` when `TotalRequests` is zero.
- `GetSeverityRating` and `FormatMetrics` do not throw; they handle boundary values internally.

## Usage

### Example 1: Recording and Evaluating a Metric

```csharp
var metric = new ServiceMetric
{
    Id = Guid.NewGuid(),
    ServiceId = service.Id,
    CpuUsagePercent = 78.3m,
    MemoryUsagePercent = 62.1m,
    MemoryUsageBytes = 536870912,
    ActiveConnections = 142,
    RequestsPerSecond = 850,
    AverageResponseTimeMs = 245.7m,
    TotalRequests = 1_200_000,
    ErrorCount = 15,
    RecordedAt = DateTime.UtcNow,
    Uptime = 86400.0,
    HasAnomalies = false
};

if (metric.TotalRequests > 0)
{
    decimal errorRate = metric.GetErrorRate;
    string severity = metric.GetSeverityRating;

    Console.WriteLine($"Error Rate: {errorRate:F2}%");
    Console.WriteLine($"Severity: {severity}");

    if (severity == "Critical")
    {
        // Trigger alerting pipeline
    }
}
```

### Example 2: Formatting for Log Output

```csharp
var metric = new ServiceMetric
{
    CpuUsagePercent = 91.5m,
    MemoryUsagePercent = 88.3m,
    ActiveConnections = 1200,
    RequestsPerSecond = 2100,
    AverageResponseTimeMs = 520.0m,
    TotalRequests = 5_000_000,
    ErrorCount = 230,
    RecordedAt = DateTime.UtcNow,
    HasAnomalies = true,
    Notes = "Spike observed after deployment v2.4.1"
};

string summary = metric.FormatMetrics();
Console.WriteLine(summary);

// Persist for later analysis
await dbContext.ServiceMetrics.AddAsync(metric);
await dbContext.SaveChangesAsync();
```

## Notes

- **Division by zero**: Always check `TotalRequests > 0` before accessing `GetErrorRate`. In high-throughput scenarios this is rarely zero, but freshly started or idle services can trigger the exception.
- **Null navigation property**: `Service` is a navigation property and may be null if the metric was loaded without an `Include` / eager-loading directive. Use `ServiceId` for reliable relational lookups.
- **Percentage ranges**: `CpuUsagePercent`, `MemoryUsagePercent`, and `DiskUsagePercent` are not clamped by the type itself. Values exceeding 100 or negative values can be stored; validation is the responsibility of the code producing the metric.
- **Thread safety**: This type is a plain data model. It provides no internal synchronization. Concurrent reads and writes to the same instance from multiple threads must be externally synchronized.
- **Anomaly flag**: `HasAnomalies` is a settable boolean. It does not automatically reflect metric values; an external detection process must evaluate and set it.
- **Timestamp precision**: `RecordedAt` uses `DateTime`, which has tick-level precision. For sub-millisecond ordering requirements, ensure the clock source provides sufficient granularity.
- **Computed member stability**: `GetSeverityRating` and `FormatMetrics` rely on internal threshold logic that may change across library versions. Do not depend on exact string values for programmatic decision-making; use the raw numeric properties for comparisons.
