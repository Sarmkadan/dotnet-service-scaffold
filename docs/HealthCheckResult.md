# HealthCheckResult

A data transfer object (DTO) used to encapsulate the outcome of a health check operation in a .NET service. It captures service identity, status metrics, resource usage, and diagnostic details to support monitoring, alerting, and automated recovery workflows.

## API

### `Id`
A unique identifier for this health check result. Used to correlate results across distributed systems or logging pipelines.

### `ServiceId`
The unique identifier of the service being checked. Enables mapping results to specific service instances in a fleet.

### `Service`
A reference to the service registration associated with this health check. May be `null` if the service is unknown or unregistered.

### `Status`
The overall health status of the service, represented as a `HealthStatus` enum. Indicates whether the service is healthy, degraded, or unhealthy.

### `HttpStatusCode`
The HTTP status code returned by the service during the health check, if applicable. May be `null` if the check did not involve an HTTP request.

### `ResponseTimeMs`
The time taken for the service to respond to the health check request, in milliseconds. May be `null` if the check did not measure response time.

### `ErrorMessage`
A descriptive error message if the health check failed. Contains details about the failure cause, if any.

### `ResponseBody`
The raw response body received from the service during the health check, if applicable. May be `null` if no body was returned or the check did not involve a response.

### `CheckedAt`
The timestamp when the health check was performed. Used to track the recency of the result.

### `CheckMethod`
The method or protocol used to perform the health check (e.g., "HTTP", "TCP", "GRPC"). May be `null` if the method is unknown.

### `CheckEndpoint`
The endpoint or address targeted by the health check (e.g., "https://api.example.com/health"). May be `null` if the endpoint is not applicable.

### `CpuUsagePercent`
The CPU usage percentage of the service process during the health check. May be `null` if CPU metrics were not collected.

### `MemoryUsagePercent`
The memory usage percentage of the service process during the health check. May be `null` if memory metrics were not collected.

### `DiskUsageBytes`
The disk usage in bytes for the service process or associated storage during the health check. May be `null` if disk metrics were not collected.

### `IsHealthy`
A computed property indicating whether the service is considered healthy based on `Status` and other criteria. Returns `true` if the service is fully operational.

### `IsResponseTimeAcceptable`
A computed property indicating whether the service's response time is within acceptable bounds. Returns `true` if `ResponseTimeMs` is within predefined thresholds.

### `AreResourcesHealthy`
A computed property indicating whether the service's resource usage (CPU, memory, disk) is within acceptable bounds. Returns `true` if all resource metrics are within predefined thresholds.

### `GetSummary()`
Generates a human-readable summary of the health check result, including status, response time, resource usage, and any errors. Returns a formatted string suitable for logging or display.

## Usage

### Example 1: Basic Health Check Result
