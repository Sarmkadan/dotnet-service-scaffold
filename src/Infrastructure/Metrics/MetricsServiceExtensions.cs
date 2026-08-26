#nullable enable

using System.Collections.Generic;
using System.Globalization;

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Extension methods for <see cref="IMetricsService"/> that provide additional metric recording capabilities
/// and convenience overloads for common scenarios.
/// </summary>
public static class MetricsServiceExtensions
{
    /// <summary>
    /// Increments a counter metric with a specific value and optional tags.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="metricName">Name of the metric to increment.</param>
    /// <param name="value">Amount to increment by (default: 1).</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metricName"/> is <see langword="null"/>.</exception>
    public static void IncrementCounter(
        this MetricsService service,
        string metricName,
        long value,
        IDictionary<string, string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(metricName);

        service.IncrementCounter(metricName, value, tags);
    }

    /// <summary>
    /// Records a gauge metric with a specific value and optional tags.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="metricName">Name of the gauge metric.</param>
    /// <param name="value">The gauge value to record.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metricName"/> is <see langword="null"/>.</exception>
    public static void RecordGauge(
        this MetricsService service,
        string metricName,
        double value,
        IDictionary<string, string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(metricName);

        service.RecordGauge(metricName, value, tags);
    }

    /// <summary>
    /// Records a timing metric in milliseconds with optional tags.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="metricName">Name of the timing metric.</param>
    /// <param name="elapsedMs">Elapsed time in milliseconds.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metricName"/> is <see langword="null"/>.</exception>
    public static void RecordTiming(
        this MetricsService service,
        string metricName,
        long elapsedMs,
        IDictionary<string, string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(metricName);

        service.RecordTiming(metricName, elapsedMs, tags);
    }

    /// <summary>
    /// Measures the execution time of an asynchronous operation and records it as a timing metric.
    /// </summary>
    /// <typeparam name="T">Return type of the operation.</typeparam>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="metricName">Name of the timing metric.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metricName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is <see langword="null"/>.</exception>
    public static async Task<T> MeasureAsync<T>(
        this MetricsService service,
        string metricName,
        Func<Task<T>> operation,
        IDictionary<string, string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(metricName);
        ArgumentNullException.ThrowIfNull(operation);

        return await service.MeasureAsync(metricName, operation, tags);
    }

    /// <summary>
    /// Increments a counter metric with value 1 and optional tags.
    /// Convenience overload for common case of incrementing by 1.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="metricName">Name of the metric to increment.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metricName"/> is <see langword="null"/>.</exception>
    public static void Increment(this MetricsService service, string metricName, IDictionary<string, string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(metricName);

        service.IncrementCounter(metricName, MetricsServiceExtensionsConstants.DefaultCounterIncrement, tags);
    }

    /// <summary>
    /// Records a gauge metric with value 0, useful for tracking presence of a resource.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="metricName">Name of the gauge metric.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metricName"/> is <see langword="null"/>.</exception>
    public static void RecordGaugeZero(this MetricsService service, string metricName, IDictionary<string, string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(metricName);

        service.RecordGauge(metricName, MetricsServiceExtensionsConstants.ZeroGaugeValue, tags);
    }

    /// <summary>
    /// Measures the execution time of an asynchronous operation and records it as a timing metric.
    /// Non-generic version for void operations.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="metricName">Name of the timing metric.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metricName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is <see langword="null"/>.</exception>
    public static async Task MeasureAsync(
        this MetricsService service,
        string metricName,
        Func<Task> operation,
        IDictionary<string, string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(metricName);
        ArgumentNullException.ThrowIfNull(operation);

        await service.MeasureAsync<object>(metricName, async () =>
        {
            await operation();
            return default!;
        }, tags);
    }

    /// <summary>
    /// Records a timing metric using a stopwatch to measure the operation duration.
    /// </summary>
    /// <param name="service">The metrics service instance.</param>
    /// <param name="metricName">Name of the timing metric.</param>
    /// <param name="action">The action to measure.</param>
    /// <param name="tags">Optional tags to associate with the metric.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metricName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <see langword="null"/>.</exception>
    public static void RecordActionTime(
        this MetricsService service,
        string metricName,
        Action action,
        IDictionary<string, string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(metricName);
        ArgumentNullException.ThrowIfNull(action);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            action();
        }
        finally
        {
            sw.Stop();
            var elapsedMs = sw.ElapsedMilliseconds;
            if (elapsedMs < MetricsServiceExtensionsConstants.MinimumElapsedMilliseconds)
            {
                // Defensive check: Stopwatch should never return negative values, but validate anyway
                elapsedMs = MetricsServiceExtensionsConstants.MinimumElapsedMilliseconds;
            }
            service.RecordTiming(metricName, elapsedMs, tags);
        }
    }
}
