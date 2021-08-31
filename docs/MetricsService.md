# MetricsService

`MetricsService` is a lightweight metrics collection utility designed to track application performance and usage statistics. It supports counters, gauges, and timing measurements, allowing developers to instrument code for observability. Metrics are aggregated in-memory and can be retrieved or reset as needed, making it suitable for both real-time monitoring and batch reporting scenarios.

## API

### `public MetricsService`

Initializes a new instance of the `MetricsService` with default values. The service starts with no metrics recorded.

### `public void IncrementCounter(string name, long value = 1, string[]? tags = null)`

Increments a counter metric by the specified value.

- **Parameters**:
  - `name` (string): The name of the counter metric.
  - `value` (long, optional): The amount to increment the counter by. Defaults to `1`.
  - `tags` (string[], optional): Key-value pairs (alternating keys and values) to associate with the metric. Must have an even number of elements if provided.
- **Throws**:
  - `ArgumentException`: If `tags` is not `null` and has an odd number of elements.

### `public void RecordGauge(string name, double value, string[]? tags = null)`

Records a gauge metric with the specified value.

- **Parameters**:
  - `name` (string): The name of the gauge metric.
  - `value` (double): The value to record.
  - `tags` (string[], optional): Key-value pairs (alternating keys and values) to associate with the metric. Must have an even number of elements if provided.
- **Throws**:
  - `ArgumentException`: If `tags` is not `null` and has an odd number of elements.

### `public void RecordTiming(string name, long milliseconds, string[]? tags = null)`

Records a timing metric in milliseconds.

- **Parameters**:
  - `name` (string): The name of the timing metric.
  - `milliseconds` (long): The duration to record.
  - `tags` (string[], optional): Key-value pairs (alternating keys and values) to associate with the metric. Must have an even number of elements if provided.
- **Throws**:
  - `ArgumentException`: If `tags` is not `null` and has an odd number of elements.

### `public async Task<T> MeasureAsync<T>(string name, Func<Task<T>> action, string[]? tags = null)`

Measures the execution time of an asynchronous operation and records it as a timing metric.

- **Parameters**:
  - `name` (string): The name of the timing metric.
  - `action` (Func<Task<T>>): The asynchronous operation to measure.
  - `tags` (string[], optional): Key-value pairs (alternating keys and values) to associate with the metric. Must have an even number of elements if provided.
- **Returns**:
  - `Task<T>`: The result of the measured operation.
- **Throws**:
  - `ArgumentException`: If `tags` is not `null` and has an odd number of elements.
  - Propagates exceptions thrown by `action`.

### `public Task<Dictionary<string, object>> GetMetricsAsync()`

Retrieves all recorded metrics as a dictionary of metric names to their aggregated values.

- **Returns**:
  - `Task<Dictionary<string, object>>`: A dictionary where keys are metric names and values are objects representing the metric's aggregated data (e.g., `Count`, `Min`, `Max`, `Value`, or `Type`).

### `public Task ResetAsync()`

Resets all recorded metrics, clearing all counters, gauges, and timings.

- **Returns**:
  - `Task`: A task representing the asynchronous operation.

### `public MetricType Type`

Gets the type of the metric (e.g., `Counter`, `Gauge`, `Timing`).

- **Returns**:
  - `MetricType`: The metric type.

### `public double Value`

Gets the current value of a gauge metric. For counters, this represents the total incremented value. For timings, this is the last recorded duration.

- **Returns**:
  - `double`: The metric value.

### `public long Count`

Gets the number of times a metric has been recorded. For counters, this is the number of increments. For gauges and timings, this is the number of recordings.

- **Returns**:
  - `long`: The count of recordings.

### `public long Min`

Gets the minimum recorded value for a timing metric. For counters and gauges, this value is `0`.

- **Returns**:
  - `long`: The minimum value.

### `public long Max`

Gets the maximum recorded value for a timing metric. For counters and gauges, this value is `0`.

- **Returns**:
  - `long`: The maximum value.

## Usage

### Example 1: Basic Metrics Collection
