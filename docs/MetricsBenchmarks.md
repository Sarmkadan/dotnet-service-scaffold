# MetricsBenchmarks

A utility class designed to benchmark and validate the performance and behavior of metrics collection in .NET applications. It provides methods to simulate common metric recording scenarios (counters, timings, gauges) with varying tag configurations, enabling consistent measurement across different instrumentation strategies.

## API

### `void Setup()`

Initializes the metrics collection system for benchmarking. This method should be called once before executing any benchmarking methods to ensure a clean state. It configures the underlying metric exporters and clears any existing metrics.

- **Parameters**: None
- **Return value**: None
- **Throws**: May throw if the metrics system fails to initialize (e.g., due to configuration errors or resource constraints).

---

### `void IncrementCounterNoTags()`

Increments a counter metric without any associated tags. This simulates a simple, high-frequency increment operation used to track occurrences of an event (e.g., "requests received").

- **Parameters**: None
- **Return value**: None
- **Throws**: May throw if the counter metric is not registered or if the underlying storage fails.

---

### `void IncrementCounterOneTag()`

Increments a counter metric with a single tag. This simulates a tagged counter operation, where the tag provides contextual information (e.g., "requests received by endpoint `/api/users`").

- **Parameters**: None
- **Return value**: None
- **Throws**: May throw if the counter or tag schema is invalid, or if the storage backend fails.

---
### `void IncrementCounterThreeTags()`

Increments a counter metric with three distinct tags. This simulates a multi-dimensional metric operation, useful for tracking events with rich contextual data (e.g., "requests received by endpoint `/api/users`, method `POST`, and status `200`).

- **Parameters**: None
- **Return value**: None
- **Throws**: May throw if the counter or tag schema is invalid, or if the storage backend fails.

---
### `void RecordTimingNoTags()`

Records a timing metric without any associated tags. This simulates a simple duration measurement (e.g., tracking the duration of a single operation in milliseconds).

- **Parameters**: None
- **Return value**: None
- **Throws**: May throw if the timing metric is not registered or if the underlying storage fails.

---
### `void RecordTimingThreeTags()`

Records a timing metric with three distinct tags. This simulates a multi-dimensional timing measurement (e.g., tracking the duration of an operation by endpoint, method, and region).

- **Parameters**: None
- **Return value**: None
- **Throws**: May throw if the timing metric or tag schema is invalid, or if the storage backend fails.

---
### `void RecordGauge()`

Records a gauge metric. Gauges represent a value that can arbitrarily go up and down (e.g., "current memory usage" or "active connections").

- **Parameters**: None
- **Return value**: None
- **Throws**: May throw if the gauge metric is not registered or if the underlying storage fails.

---
### `Task<Dictionary<string, object>> GetMetrics()`

Asynchronously retrieves all recorded metrics as a dictionary of metric names to their serialized values. This method is used to validate the state of metrics after benchmarking operations.

- **Parameters**: None
- **Return value**: A `Task<Dictionary<string, object>>` containing the aggregated metrics. The dictionary keys are metric names, and the values are the serialized metric data.
- **Throws**: May throw if the metrics retrieval fails (e.g., due to serialization errors or backend issues).

## Usage

### Example 1: Basic Benchmarking Workflow
