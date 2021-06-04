// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotnetServiceScaffold.Infrastructure.Metrics;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Benchmarks for in-process metric collection. Covers the two most-called paths:
/// plain counter increments (tagless, hot path on every handled request) and
/// tagged timing records (one per tracked operation). Also covers snapshot reads
/// used by the /api/metrics endpoint.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class MetricsBenchmarks
{
    private MetricsService _metrics = default!;

    private static readonly Dictionary<string, string> _singleTag = new()
    {
        { "service", "UserService" }
    };

    private static readonly Dictionary<string, string> _threeTags = new()
    {
        { "service", "UserService" },
        { "region", "eu-west-1" },
        { "env", "production" }
    };

    [GlobalSetup]
    public void Setup()
    {
        _metrics = new MetricsService(NullLogger<MetricsService>.Instance);

        // Pre-populate some counters so GetMetricsAsync has data to aggregate
        for (int i = 0; i < 50; i++)
        {
            _metrics.IncrementCounter("requests.total");
            _metrics.RecordTiming("request.duration_ms", 10 + i % 200);
            _metrics.RecordGauge("memory.mb", 128 + i * 0.5);
        }
    }

    [Benchmark(Description = "IncrementCounter — no tags (hot path)")]
    public void IncrementCounterNoTags()
        => _metrics.IncrementCounter("requests.total");

    [Benchmark(Description = "IncrementCounter — 1 tag")]
    public void IncrementCounterOneTag()
        => _metrics.IncrementCounter("requests.total", 1, _singleTag);

    [Benchmark(Description = "IncrementCounter — 3 tags (key build)")]
    public void IncrementCounterThreeTags()
        => _metrics.IncrementCounter("requests.total", 1, _threeTags);

    [Benchmark(Description = "RecordTiming — no tags")]
    public void RecordTimingNoTags()
        => _metrics.RecordTiming("request.duration_ms", 42);

    [Benchmark(Description = "RecordTiming — 3 tags")]
    public void RecordTimingThreeTags()
        => _metrics.RecordTiming("request.duration_ms", 42, _threeTags);

    [Benchmark(Description = "RecordGauge — no tags")]
    public void RecordGauge()
        => _metrics.RecordGauge("memory.mb", 256.5);

    [Benchmark(Description = "GetMetricsAsync — read snapshot (50 entries)")]
    public Task<Dictionary<string, object>> GetMetrics()
        => _metrics.GetMetricsAsync();
}
