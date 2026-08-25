#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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
public class MetricsBenchmarks : IMetricsBenchmarks
{
    private MetricsService _metrics = default!;

    private static readonly Dictionary<string, string> _singleTag = new()
    {
        { MetricsBenchmarksConstants.TagKeyService, MetricsBenchmarksConstants.TagValueUserService }
    };

    private static readonly Dictionary<string, string> _threeTags = new()
    {
        { MetricsBenchmarksConstants.TagKeyService, MetricsBenchmarksConstants.TagValueUserService },
        { MetricsBenchmarksConstants.TagKeyRegion, MetricsBenchmarksConstants.TagValueEuWest1 },
        { MetricsBenchmarksConstants.TagKeyEnv, MetricsBenchmarksConstants.TagValueProduction }
    };

    [GlobalSetup]
    public void Setup()
    {
        _metrics = new MetricsService(NullLogger<MetricsService>.Instance);

        // Pre-populate some counters so GetMetricsAsync has data to aggregate
        for (int i = 0; i < MetricsBenchmarksConstants.SetupLoopCount; i++)
        {
            _metrics.IncrementCounter(MetricsBenchmarksConstants.CounterRequestsTotal);
            _metrics.RecordTiming(MetricsBenchmarksConstants.TimingRequestDurationMs, MetricsBenchmarksConstants.SetupTimingBase + i % MetricsBenchmarksConstants.SetupTimingMod);
            _metrics.RecordGauge(MetricsBenchmarksConstants.GaugeMemoryMb, MetricsBenchmarksConstants.SetupMemoryBase + i * MetricsBenchmarksConstants.SetupMemoryIncrement);
        }
    }

    [Benchmark(Description = "IncrementCounter — no tags (hot path)")]
    public void IncrementCounterNoTags()
        => _metrics.IncrementCounter(MetricsBenchmarksConstants.CounterRequestsTotal);

    [Benchmark(Description = "IncrementCounter — 1 tag")]
    public void IncrementCounterOneTag()
        => _metrics.IncrementCounter(MetricsBenchmarksConstants.CounterRequestsTotal, 1, _singleTag);

    [Benchmark(Description = "IncrementCounter — 3 tags (key build)")]
    public void IncrementCounterThreeTags()
        => _metrics.IncrementCounter(MetricsBenchmarksConstants.CounterRequestsTotal, 1, _threeTags);

    [Benchmark(Description = "RecordTiming — no tags")]
    public void RecordTimingNoTags()
        => _metrics.RecordTiming(MetricsBenchmarksConstants.TimingRequestDurationMs, MetricsBenchmarksConstants.TimingValue);

    [Benchmark(Description = "RecordTiming — 3 tags")]
    public void RecordTimingThreeTags()
        => _metrics.RecordTiming(MetricsBenchmarksConstants.TimingRequestDurationMs, MetricsBenchmarksConstants.TimingValue, _threeTags);

    [Benchmark(Description = "RecordGauge — no tags")]
    public void RecordGauge()
        => _metrics.RecordGauge(MetricsBenchmarksConstants.GaugeMemoryMb, MetricsBenchmarksConstants.GaugeValue);

    [Benchmark(Description = "GetMetricsAsync — read snapshot (50 entries)")]
    public Task<Dictionary<string, object>> GetMetrics()
        => _metrics.GetMetricsAsync();
}