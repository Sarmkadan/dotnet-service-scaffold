#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Provides validation helpers for <see cref="MetricsService"/> instances.
/// Validates metric names, values, and ranges to ensure data integrity.
/// </summary>
public static class MetricsServiceValidation
{
    private const string MetricNamePattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
    private const int MaxMetricNameLength = 100;
    private const double MinGaugeValue = -1_000_000_000;
    private const double MaxGaugeValue = 1_000_000_000;
    private const long MinCounterValue = 0;
    private const long MaxCounterValue = 1_000_000_000;
    private const long MinTimingValue = 0;
    private const long MaxTimingValue = 3_600_000_000; // 1 hour in ms

    /// <summary>
    /// Validates a metrics service instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The metrics service to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MetricsService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // MetricsService itself has no state to validate beyond null check
        // The actual metrics are validated when they're recorded via the public methods
        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a metrics service instance is valid.
    /// </summary>
    /// <param name="value">The metrics service to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this MetricsService? value)
    {
        try
        {
            _ = Validate(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures a metrics service instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The metrics service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the metrics service is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this MetricsService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"MetricsService is invalid. Validation errors: {string.Join("; ", errors)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates a metric name according to naming conventions.
    /// </summary>
    /// <param name="metricName">The metric name to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    public static IReadOnlyList<string> ValidateMetricName(string? metricName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(metricName))
        {
            errors.Add("Metric name cannot be null, empty, or whitespace.");
            return errors.AsReadOnly();
        }

        if (metricName.Length > MaxMetricNameLength)
        {
            errors.Add($"Metric name '{metricName}' exceeds maximum length of {MaxMetricNameLength} characters.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(metricName, MetricNamePattern))
        {
            errors.Add(
                $"Metric name '{metricName}' must start with a letter or underscore and contain only letters, digits, and underscores.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a counter value.
    /// </summary>
    /// <param name="value">The counter value to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    public static IReadOnlyList<string> ValidateCounterValue(long value)
    {
        var errors = new List<string>();

        if (value < MinCounterValue)
        {
            errors.Add($"Counter value {value} is below minimum of {MinCounterValue}.");
        }

        if (value > MaxCounterValue)
        {
            errors.Add($"Counter value {value} exceeds maximum of {MaxCounterValue}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a gauge value.
    /// </summary>
    /// <param name="value">The gauge value to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    public static IReadOnlyList<string> ValidateGaugeValue(double value)
    {
        var errors = new List<string>();

        if (double.IsNaN(value))
        {
            errors.Add("Gauge value cannot be NaN.");
        }
        else if (double.IsInfinity(value))
        {
            errors.Add("Gauge value cannot be infinite.");
        }
        else if (value < MinGaugeValue)
        {
            errors.Add($"Gauge value {value.ToString(CultureInfo.InvariantCulture)} is below minimum of {MinGaugeValue}.");
        }
        else if (value > MaxGaugeValue)
        {
            errors.Add($"Gauge value {value.ToString(CultureInfo.InvariantCulture)} exceeds maximum of {MaxGaugeValue}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a timing value in milliseconds.
    /// </summary>
    /// <param name="elapsedMs">The timing value to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    public static IReadOnlyList<string> ValidateTimingValue(long elapsedMs)
    {
        var errors = new List<string>();

        if (elapsedMs < MinTimingValue)
        {
            errors.Add($"Timing value {elapsedMs}ms is below minimum of {MinTimingValue}ms.");
        }

        if (elapsedMs > MaxTimingValue)
        {
        errors.Add($"Timing value {elapsedMs}ms exceeds maximum of {MaxTimingValue}ms (1 hour).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates metric tags collection.
    /// </summary>
    /// <param name="tags">The tags to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    public static IReadOnlyList<string> ValidateTags(IDictionary<string, string>? tags)
    {
        var errors = new List<string>();

        if (tags is null)
        {
            return errors.AsReadOnly();
        }

        foreach (var kvp in tags)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
            {
                errors.Add("Tag key cannot be null, empty, or whitespace.");
            }

            if (kvp.Value is null)
            {
                errors.Add($"Tag with key '{kvp.Key}' has a null value.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a metric value object containing statistics.
    /// </summary>
    /// <param name="metricType">The type of metric.</param>
    /// <param name="value">The metric value.</param>
    /// <param name="count">The count (for timers).</param>
    /// <param name="min">The minimum value (for timers).</param>
    /// <param name="max">The maximum value (for timers).</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    internal static IReadOnlyList<string> ValidateMetricValue(
        MetricType metricType,
        double value,
        long count = 0,
        long min = long.MaxValue,
        long max = long.MinValue)
    {
        var errors = new List<string>();

        // Validate value based on type
        if (metricType == MetricType.Counter)
        {
            errors.AddRange(ValidateCounterValue((long)value));
        }
        else if (metricType == MetricType.Gauge)
        {
            errors.AddRange(ValidateGaugeValue(value));
        }
        else if (metricType == MetricType.Timer)
        {
            errors.AddRange(ValidateTimingValue((long)value));

            if (count < 0)
            {
                errors.Add("Timer count cannot be negative.");
            }

            if (min != long.MaxValue && min < MinTimingValue)
            {
                errors.Add($"Timer minimum {min}ms is below minimum of {MinTimingValue}ms.");
            }

            if (max != long.MinValue && max < MinTimingValue)
            {
                errors.Add($"Timer maximum {max}ms is below minimum of {MinTimingValue}ms.");
            }

            if (min != long.MaxValue && max != long.MinValue && min > max)
            {
                errors.Add("Timer minimum cannot be greater than maximum.");
            }
        }

        return errors.AsReadOnly();
    }
}