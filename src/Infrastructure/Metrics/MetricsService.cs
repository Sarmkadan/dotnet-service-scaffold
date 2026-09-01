#nullable enable
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsService"/> class.
    /// </summary>
    public MetricsService(ILogger<MetricsService> logger)
    {
        _metrics = new ConcurrentDictionary<string, MetricValue>();
        _logger = logger;
    }

    public override string ToString() => $"MetricsService {{ MetricCount = {_metrics.Count} }}";

    /// <summary>
    /// Increments a counter metric.
    /// </summary>
    public void IncrementCounter(string metricName, long value = 1, IDictionary<string, string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(metricName);
        _logger.LogInformation("Starting IncrementCounter for metric {MetricName} with value {Value}", metricName, value);

        var key = BuildMetricKey(metricName, tags);

        _metrics.AddOrUpdate(key,
            new MetricValue { Type = MetricType.Counter, Value = value },
            (_, existing) =>
            {
                existing.Value += value;
                return existing;
            });

        _logger.LogDebug("Counter metric {MetricName} incremented by {Value}", metricName, value);
        _logger.LogInformation("Finished IncrementCounter for metric {MetricName}", metricName);
    }

    /// <summary>
    /// Records a gauge (current value) metric, overwriting the previous value.
    /// </summary>
    public void RecordGauge(string metricName, double value, IDictionary<string, string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(metricName);
        _logger.LogInformation("Starting RecordGauge for metric {MetricName} with value {Value}", metricName, value);

        var key = BuildMetricKey(metricName, tags);

        _metrics.AddOrUpdate(key,
            new MetricValue { Type = MetricType.Gauge, Value = value },
            (_, _) => new MetricValue { Type = MetricType.Gauge, Value = value });

        _logger.LogDebug("Gauge metric {MetricName} set to {Value}", metricName, value);
        _logger.LogInformation("Finished RecordGauge for metric {MetricName}", metricName);
    }

    /// <summary>
    /// Records a timing metric in milliseconds.
    /// </summary>
    public void RecordTiming(string metricName, long elapsedMs, IDictionary<string, string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(metricName);
        _logger.LogInformation("Starting RecordTiming for metric {MetricName} with elapsed {ElapsedMs}ms", metricName, elapsedMs);

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
        _logger.LogInformation("Finished RecordTiming for metric {MetricName}", metricName);
    }

    /// <summary>
    /// Records timing of an operation using a stopwatch pattern.
    /// </summary>
    public async Task<T> MeasureAsync<T>(string metricName, Func<Task<T>> operation, IDictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(metricName);
        ArgumentNullException.ThrowIfNull(operation);
        _logger.LogInformation("Starting MeasureAsync for metric {MetricName}", metricName);

        var sw = Stopwatch.StartNew();

        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during MeasureAsync for metric {MetricName}", metricName);
            throw;
        }
        finally
        {
            sw.Stop();
            RecordTiming(metricName, sw.ElapsedMilliseconds, tags);
            _logger.LogInformation("Finished MeasureAsync for metric {MetricName}", metricName);
        }
    }

    /// <summary>
    /// Records a histogram metric with explicit bucket boundaries.
    /// </summary>
    public void RecordHistogram(string metricName, double value, double[] buckets, IDictionary<string, string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(metricName);
        ArgumentNullException.ThrowIfNull(buckets);
        _logger.LogInformation("Starting RecordHistogram for metric {MetricName} with value {Value} and {BucketCount} buckets", metricName, value, buckets.Length);

        var key = BuildMetricKey(metricName, tags);

        _metrics.AddOrUpdate(key,
            new MetricValue { Type = MetricType.Histogram, Value = value, Buckets = buckets, Count = 1 },
            (_, existing) =>
            {
                existing.Value += value;
                existing.Count++;
                return existing;
            });

        _logger.LogDebug("Histogram metric {MetricName} recorded value {Value} with {BucketCount} buckets", metricName, value, buckets.Length);
        _logger.LogInformation("Finished RecordHistogram for metric {MetricName}", metricName);
    }

    /// <summary>
    /// Gets all recorded metrics as a dictionary suitable for serialization.
    /// </summary>
    public Task<Dictionary<string, object>> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting GetMetricsAsync with {MetricCount} recorded metrics", _metrics.Count);
        cancellationToken.ThrowIfCancellationRequested();
        var result = new Dictionary<string, object>(_metrics.Count);

        foreach (var kvp in _metrics)
        {
            var metric = kvp.Value;
            var typeName = _typeNames.GetValueOrDefault(metric.Type, "unknown");

            if (typeName == "unknown")
                _logger.LogWarning("Using fallback type name for metric {MetricKey} with type {MetricType}", kvp.Key, metric.Type);

            if (metric.Type == MetricType.Histogram && (metric.Buckets == null || metric.BucketCounts == null))
                _logger.LogWarning("Using degraded serialization for histogram metric {MetricKey} because bucket data is incomplete", kvp.Key);

        object metricData = metric.Type switch
        {
            MetricType.Timer => new
            {
                type = typeName,
                totalMs = metric.Value,
                count = metric.Count,
                avgMs = metric.Count > 0 ? metric.Value / metric.Count : 0,
                minMs = metric.Min,
                maxMs = metric.Max
            },
            MetricType.Histogram when metric.Buckets != null && metric.BucketCounts != null => new
            {
                type = typeName,
                sum = metric.BucketSum ?? metric.Value,
                count = metric.Count,
                buckets = metric.Buckets,
                bucketCounts = metric.BucketCounts
            },
            _ => new { type = typeName, value = metric.Value }
        };

            result[kvp.Key] = metricData;
        }

        _logger.LogInformation("Finished GetMetricsAsync with {MetricCount} metrics", result.Count);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Resets all metrics.
    /// </summary>
    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting ResetAsync with {MetricCount} recorded metrics", _metrics.Count);
        cancellationToken.ThrowIfCancellationRequested();
        _metrics.Clear();
        _logger.LogInformation("All metrics reset");
        _logger.LogInformation("Finished ResetAsync with {MetricCount} recorded metrics", _metrics.Count);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Builds a unique key for a metric including tags.
    /// Uses string.Create to compute the final string in one allocation — no LINQ,
    /// no intermediate arrays, no string.Join overhead.
    /// </summary>
    private static string BuildMetricKey(string metricName, IDictionary<string, string>? tags)
    {
        if (tags is null || tags.Count == 0)
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
/// Class representing a metric value with type and statistics.
/// </summary>
public class MetricValue
{
    public MetricType Type { get; set; }
    public double Value { get; set; }
    public long Count { get; set; }
    public long Min { get; set; } = long.MaxValue;
    public long Max { get; set; }
    public double[]? Buckets { get; set; }
    public long[]? BucketCounts { get; set; }
    public long? BucketSum { get; set; }

    public override string ToString() =>
        $"{nameof(MetricValue)} {{ Type = {Type}, Value = {Value}, Count = {Count}, Min = {Min}, Max = {Max}, Buckets = {Buckets} }}";
}

/// <summary>
/// Enum for metric types.
/// </summary>
public enum MetricType
{
    Counter,
    Gauge,
    Timer,
    Histogram
}
