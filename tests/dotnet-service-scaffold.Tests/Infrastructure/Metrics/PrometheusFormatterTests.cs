#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Metrics;
using DotnetServiceScaffold.Tests.Infrastructure.Metrics;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the PrometheusFormatter class.
/// </summary>
public class PrometheusFormatterTests : IPrometheusFormatterTests
{
    private readonly PrometheusFormatter _formatter = new();

    /// <summary>
    /// Verifies that the Format method emits a counter for a counter metric.
    /// </summary>
    [Fact]
    public void Format_ShouldEmitCounter_ForCounterMetric()
    {
        var metrics = new Dictionary<string, object>
        {
            [DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.HttpRequestsKey] = new { type = DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.CounterType, value = 42.0 }
        };

        var result = _formatter.Format(metrics, DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix);

        result.Should().Contain($"# TYPE {DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix}_http_requests {DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.CounterType}");
        result.Should().Contain($"{DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix}_http_requests_total 42");
    }

    /// <summary>
    /// Verifies that the Format method emits a gauge for a gauge metric.
    /// </summary>
    [Fact]
    public void Format_ShouldEmitGauge_ForGaugeMetric()
    {
        var metrics = new Dictionary<string, object>
        {
            [DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.MemoryUsedKey] = new { type = DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.GaugeType, value = 128.5 }
        };

        var result = _formatter.Format(metrics, DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix);

        result.Should().Contain($"# TYPE {DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix}_memory_used {DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.GaugeType}");
        result.Should().Contain($"{DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix}_memory_used");
    }

    /// <summary>
    /// Verifies that the Format method emits a timer series for a timer metric.
    /// </summary>
    [Fact]
    public void Format_ShouldEmitTimerSeries_ForTimerMetric()
    {
        var metrics = new Dictionary<string, object>
        {
            [DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.DbQueryKey] = new { type = DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.TimerType, totalMs = 500.0, count = 10L, avgMs = 50.0, minMs = 5L, maxMs = 150L }
        };

        var result = _formatter.Format(metrics, DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix);

        result.Should().Contain($"{DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix}_db_query_sum");
        result.Should().Contain($"{DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix}_db_query_count");
        result.Should().Contain($"{DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix}_db_query_min_ms");
        result.Should().Contain($"{DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix}_db_query_max_ms");
    }

    /// <summary>
    /// Verifies that the Format method returns an empty string when no metrics are provided.
    /// </summary>
    [Fact]
    public void Format_ShouldReturnEmpty_WhenNoMetrics()
    {
        var result = _formatter.Format(new Dictionary<string, object>(), DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix);

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that the Format method throws an ArgumentNullException when the metrics parameter is null.
    /// </summary>
    [Fact]
    public void Format_ShouldThrow_WhenMetricsIsNull()
    {
        var act = () => _formatter.Format(null!, "app");

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that the Format method sanitizes metric names with special characters.
    /// </summary>
    [Fact]
    public void Format_ShouldSanitizeMetricNames_WithSpecialChars()
    {
        var metrics = new Dictionary<string, object>
        {
            [DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.SomeMetricPathKey] = new { type = DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.GaugeType, value = 1.0 }
        };

        var result = _formatter.Format(metrics, DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix);

        result.Should().Contain($"{DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.AppPrefix}_some_metric_path");
        result.Should().NotContain(DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.SomeMetricPathKey);
    }

    /// <summary>
    /// Verifies that the Format method handles tagged keys.
    /// </summary>
    [Fact]
    public void Format_ShouldHandleTaggedKeys()
    {
        var metrics = new Dictionary<string, object>
        {
            [DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.TaggedHttpRequestsKey] = new { type = DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.CounterType, value = 10.0 }
        };

        var result = _formatter.Format(metrics, DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.SvcPrefix);

        result.Should().Contain(DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.MethodGetTag);
        result.Should().Contain(DotnetServiceScaffold.Tests.Infrastructure.Metrics.PrometheusFormatterTestsConstants.Status200Tag);
    }
}
