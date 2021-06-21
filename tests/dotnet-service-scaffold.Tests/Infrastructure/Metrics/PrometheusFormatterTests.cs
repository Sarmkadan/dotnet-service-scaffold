#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Metrics;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.Metrics;

public class PrometheusFormatterTests
{
    private readonly PrometheusFormatter _formatter = new();

    [Fact]
    public void Format_ShouldEmitCounter_ForCounterMetric()
    {
        var metrics = new Dictionary<string, object>
        {
            ["http.requests"] = new { type = "counter", value = 42.0 }
        };

        var result = _formatter.Format(metrics, "app");

        result.Should().Contain("# TYPE app_http_requests counter");
        result.Should().Contain("app_http_requests_total 42");
    }

    [Fact]
    public void Format_ShouldEmitGauge_ForGaugeMetric()
    {
        var metrics = new Dictionary<string, object>
        {
            ["memory.used"] = new { type = "gauge", value = 128.5 }
        };

        var result = _formatter.Format(metrics, "app");

        result.Should().Contain("# TYPE app_memory_used gauge");
        result.Should().Contain("app_memory_used");
    }

    [Fact]
    public void Format_ShouldEmitTimerSeries_ForTimerMetric()
    {
        var metrics = new Dictionary<string, object>
        {
            ["db.query"] = new { type = "timer", totalMs = 500.0, count = 10L, avgMs = 50.0, minMs = 5L, maxMs = 150L }
        };

        var result = _formatter.Format(metrics, "app");

        result.Should().Contain("app_db_query_sum");
        result.Should().Contain("app_db_query_count");
        result.Should().Contain("app_db_query_min_ms");
        result.Should().Contain("app_db_query_max_ms");
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenNoMetrics()
    {
        var result = _formatter.Format(new Dictionary<string, object>(), "app");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Format_ShouldThrow_WhenMetricsIsNull()
    {
        var act = () => _formatter.Format(null!, "app");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Format_ShouldSanitizeMetricNames_WithSpecialChars()
    {
        var metrics = new Dictionary<string, object>
        {
            ["some-metric.path"] = new { type = "gauge", value = 1.0 }
        };

        var result = _formatter.Format(metrics, "app");

        result.Should().Contain("app_some_metric_path");
        result.Should().NotContain("some-metric.path");
    }

    [Fact]
    public void Format_ShouldHandleTaggedKeys()
    {
        var metrics = new Dictionary<string, object>
        {
            ["http.requests[method=GET,status=200]"] = new { type = "counter", value = 10.0 }
        };

        var result = _formatter.Format(metrics, "svc");

        result.Should().Contain("method=\"GET\"");
        result.Should().Contain("status=\"200\"");
    }
}
