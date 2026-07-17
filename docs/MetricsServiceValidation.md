# MetricsServiceValidation

Provides static validation utilities for metrics-related inputs such as metric names, values, and tags used in telemetry and monitoring scenarios. These methods ensure that metrics conform to expected naming conventions, value ranges, and tagging rules before being emitted or processed.

## API

### `public static IReadOnlyList<string> Validate`

Validates a complete set of metric components including name, value, and tags. Returns a list of validation error messages; an empty list indicates the input is valid.

- **Parameters**:
  - `metricName`: The name of the metric to validate.
  - `value`: The numeric value associated with the metric.
  - `tags`: Optional collection of key-value pairs used to annotate the metric.
- **Returns**: An `IReadOnlyList<string>` of error messages. If empty, the input is valid.
- **Throws**: `ArgumentNullException` if `metricName` or `value` is `null`.

---

### `public static bool IsValid`

Determines whether the given metric name, value, and tags are valid without returning detailed error messages. Useful for quick validation checks.

- **Parameters**:
  - `metricName`: The name of the metric to validate.
  - `value`: The numeric value associated with the metric.
  - `tags`: Optional collection of key-value pairs used to annotate the metric.
- **Returns**: `true` if the input is valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `metricName` or `value` is `null`.

---

### `public static void EnsureValid`

Validates the provided metric name, value, and tags, and throws an exception if any validation rule is violated. This method does not return any value and is intended for use in hot paths where immediate failure is preferred.

- **Parameters**:
  - `metricName`: The name of the metric to validate.
  - `value`: The numeric value associated with the metric.
  - `tags`: Optional collection of key-value pairs used to annotate the metric.
- **Throws**: `ArgumentNullException` if `metricName` or `value` is `null`.
- **Throws**: `MetricsValidationException` if validation fails, containing a list of error messages.

---

### `public static IReadOnlyList<string> ValidateMetricName`

Validates the structure and content of a metric name. Ensures it follows naming conventions such as being non-empty, not containing invalid characters, and conforming to length limits.

- **Parameters**:
  - `name`: The metric name to validate.
- **Returns**: An `IReadOnlyList<string>` of error messages. If empty, the name is valid.
- **Throws**: `ArgumentNullException` if `name` is `null`.

---

### `public static IReadOnlyList<string> ValidateCounterValue`

Validates that a numeric value is suitable for a counter metric. Counters typically accept non-negative values and may enforce maximum bounds.

- **Parameters**:
  - `value`: The value to validate.
- **Returns**: An `IReadOnlyList<string>` of error messages. If empty, the value is valid.
- **Throws**: `ArgumentNullException` if `value` is `null`.

---
### `public static IReadOnlyList<string> ValidateGaugeValue`

Validates that a numeric value is suitable for a gauge metric. Gauges may accept any real number within a defined range.

- **Parameters**:
  - `value`: The value to validate.
- **Returns**: An `IReadOnlyList<string>` of error messages. If empty, the value is valid.
- **Throws**: `ArgumentNullException` if `value` is `null`.

---
### `public static IReadOnlyList<string> ValidateTimingValue`

Validates that a numeric value is suitable for a timing metric. Timing values are typically non-negative and may be bounded by maximum duration.

- **Parameters**:
  - `value`: The value to validate.
- **Returns**: An `IReadOnlyList<string>` of error messages. If empty, the value is valid.
- **Throws**: `ArgumentNullException` if `value` is `null`.

---
### `public static IReadOnlyList<string> ValidateTags`

Validates a collection of key-value pairs used as tags for a metric. Ensures keys and values are non-empty, within length limits, and do not contain invalid characters.

- **Parameters**:
  - `tags`: The collection of tags to validate.
- **Returns**: An `IReadOnlyList<string>` of error messages. If empty, the tags are valid.
- **Throws**: `ArgumentNullException` if `tags` is `null`.

## Usage
