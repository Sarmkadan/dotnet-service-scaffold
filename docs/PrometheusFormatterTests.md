# PrometheusFormatterTests

Unit tests for the PrometheusFormatter class, verifying correct serialization of Prometheus metrics into the exposition format. The tests validate behavior for counter, gauge, and timer metrics, including edge cases like null inputs, sanitization of metric names, and handling of tagged keys.

## API

### `Format_ShouldEmitCounter_ForCounterMetric`
Verifies that a counter metric is correctly serialized into the Prometheus exposition format. The output must include the metric name, labels (if any), and the counter value with the `# TYPE` and `# HELP` directives.

### `Format_ShouldEmitGauge_ForGaugeMetric`
Ensures that a gauge metric is serialized correctly, including the metric name, labels, and value. The output must adhere to the Prometheus exposition format, with proper `# TYPE` and `# HELP` directives.

### `Format_ShouldEmitTimerSeries_ForTimerMetric`
Validates that a timer metric (histogram or summary) is serialized into the expected Prometheus exposition format. The output must include the base metric name, labels, and the timer's histogram buckets or summary statistics, with correct `# TYPE` and `# HELP` directives.

### `Format_ShouldReturnEmpty_WhenNoMetrics`
Checks that the formatter returns an empty string when no metrics are provided. This test ensures the formatter handles empty input gracefully without throwing exceptions.

### `Format_ShouldThrow_WhenMetricsIsNull`
Confirms that the formatter throws an `ArgumentNullException` when the input metrics collection is null. This test validates the formatter's input validation.

### `Format_ShouldSanitizeMetricNames_WithSpecialChars`
Ensures that metric names containing special characters (e.g., spaces, hyphens, or non-alphanumeric characters) are sanitized into valid Prometheus metric names. The output must replace or remove invalid characters while preserving the metric's semantic meaning.

### `Format_ShouldHandleTaggedKeys`
Validates that metrics with tagged keys (labels) are serialized correctly, including proper escaping of label values and adherence to the Prometheus exposition format. The output must include the metric name, labels, and value in the correct order.

## Usage

### Example 1: Basic Metric Formatting
