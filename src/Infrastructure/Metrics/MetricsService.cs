// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// In-process metrics service for tracking application performance.
/// Accumulates counters, gauges, and timing data. For production use, integrate with
/// Prometheus, Application Insights, or similar monitoring system.
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly ConcurrentDictionary<string, MetricValue> _metrics;
    private readonly ILogger<MetricsService> _logger;

    // FrozenDictionary is built once at class-load time; lookup uses a perfect hash
    // with no locking — optimal for a static read-only table that is hit on every metric record.
    private static readonly FrozenDictionary<MetricType, string> _typeNames =
        new Dictionary<MetricType, string>
        {
            [MetricType.Counter] = "counter",
            [MetricType.Gauge]   = "gauge",
            [MetricType.Timer]   = "timer"
        }.ToFrozenDictionary();

    public MetricsService(ILogger<MetricsService> logger)
    {
        _metrics = new ConcurrentDictionary<string, MetricValue>();
        _logger = logger;
    }

    /// <summary>
    /// Increments a counter metric.
    /// </summary>
    public void IncrementCounter(string metricName, long value = 1, IDictionary<string, string>? tags = null)
    {
        var key = BuildMetricKey(metricName, tags);

        _metrics.AddOrUpdate(key,
            new MetricValue { Type = MetricType.Counter, Value = value },
            (_, existing) =>
            {
                existing.Value += value;
                return existing;
            });

        _logger.LogDebug("Counter metric {MetricName} incremented by {Value}", metricName, value);
    }

    /// <summary>
    /// Records a gauge (current value) metric, overwriting the previous value.
    /// </summary>
    public void RecordGauge(string metricName, double value, IDictionary<string, string>? tags = null)
    {
        var key = BuildMetricKey(metricName, tags);

        _metrics.AddOrUpdate(key,
            new MetricValue { Type = MetricType.Gauge, Value = value },
            (_, _) => new MetricValue { Type = MetricType.Gauge, Value = value });

        _logger.LogDebug("Gauge metric {MetricName} set to {Value}", metricName, value);
    }

    /// <summary>
    /// Records a timing metric in milliseconds.
    /// </summary>
    public void RecordTiming(string metricName, long elapsedMs, IDictionary<string, string>? tags = null)
    {
        var key = BuildMetricKey(metricName, tags);

        _metrics.AddOrUpdate(key,
            new MetricValue { Type = MetricType.Timer, Value = elapsedMs, Count = 1, Min = elapsedMs, Max = elapsedMs },
            (_, existing) =>
            {
                existing.Value += elapsedMs;
                existing.Count++;
                if (elapsedMs < existing.Min) existing.Min = elapsedMs;
                if (elapsedMs > existing.Max) existing.Max = elapsedMs;
                return existing;
            });

        _logger.LogDebug("Timer metric {MetricName} recorded {ElapsedMs}ms", metricName, elapsedMs);
    }

    /// <summary>
    /// Records timing of an operation using a stopwatch pattern.
    /// </summary>
    public async Task<T> MeasureAsync<T>(string metricName, Func<Task<T>> operation, IDictionary<string, string>? tags = null)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            return await operation();
        }
        finally
        {
            sw.Stop();
            RecordTiming(metricName, sw.ElapsedMilliseconds, tags);
        }
    }

    /// <summary>
    /// Gets all recorded metrics as a dictionary suitable for serialization.
    /// </summary>
    public Task<Dictionary<string, object>> GetMetricsAsync()
    {
        var result = new Dictionary<string, object>(_metrics.Count);

        foreach (var kvp in _metrics)
        {
            var metric = kvp.Value;
            var typeName = _typeNames.GetValueOrDefault(metric.Type, "unknown");

            object metricData = metric.Type == MetricType.Timer
                ? new
                {
                    type    = typeName,
                    totalMs = metric.Value,
                    count   = metric.Count,
                    avgMs   = metric.Count > 0 ? metric.Value / metric.Count : 0,
                    minMs   = metric.Min,
                    maxMs   = metric.Max
                }
                : new { type = typeName, value = metric.Value };

            result[kvp.Key] = metricData;
        }

        return Task.FromResult(result);
    }

    /// <summary>
    /// Resets all metrics.
    /// </summary>
    public Task ResetAsync()
    {
        _metrics.Clear();
        _logger.LogInformation("All metrics reset");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Builds a unique key for a metric including tags.
    /// Uses string.Create to compute the final string in one allocation — no LINQ,
    /// no intermediate arrays, no string.Join overhead.
    /// </summary>
    private static string BuildMetricKey(string metricName, IDictionary<string, string>? tags)
    {
        if (tags == null || tags.Count == 0)
            return metricName;

        // Pre-calculate the exact output length to avoid any buffer resizing.
        int len = metricName.Length + 2; // '[' + ']'
        int idx = 0;
        foreach (var kvp in tags)
        {
            if (idx++ > 0) len++; // ',' separator
            len += kvp.Key.Length + 1 + kvp.Value.Length; // "key=value"
        }

        return string.Create(len, (metricName, tags), static (span, state) =>
        {
            var (name, t) = state;
            name.AsSpan().CopyTo(span);
            int pos = name.Length;
            span[pos++] = '[';
            bool first = true;
            foreach (var kvp in t)
            {
                if (!first) span[pos++] = ',';
                first = false;
                kvp.Key.AsSpan().CopyTo(span.Slice(pos));
                pos += kvp.Key.Length;
                span[pos++] = '=';
                kvp.Value.AsSpan().CopyTo(span.Slice(pos));
                pos += kvp.Value.Length;
            }
            span[pos] = ']';
        });
    }
}

/// <summary>
/// Internal class representing a metric value with type and statistics.
/// </summary>
internal class MetricValue
{
    public MetricType Type { get; set; }
    public double Value { get; set; }
    public long Count { get; set; }
    public long Min { get; set; } = long.MaxValue;
    public long Max { get; set; }
}

/// <summary>
/// Enum for metric types.
/// </summary>
internal enum MetricType
{
    Counter,
    Gauge,
    Timer
}
