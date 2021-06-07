#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Interface for recording application metrics and performance data.
/// Provides a clean API for tracking counters, timings, and gauges.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Increments a counter metric.
    /// </summary>
    void IncrementCounter(string metricName, long value = 1, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Records a gauge (current value) metric.
    /// </summary>
    void RecordGauge(string metricName, double value, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Records a timing metric in milliseconds.
    /// </summary>
    void RecordTiming(string metricName, long elapsedMs, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Records timing of an operation using a stopwatch pattern.
    /// </summary>
    Task<T> MeasureAsync<T>(string metricName, Func<Task<T>> operation, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Gets all recorded metrics as a dictionary.
    /// </summary>
    Task<Dictionary<string, object>> GetMetricsAsync();

    /// <summary>
    /// Resets all metrics.
    /// </summary>
    Task ResetAsync();
}
