#nullable enable
using FluentAssertions;
using Xunit;
using System.Collections.Generic;

public interface IPrometheusFormatterTests
{
    void Format_ShouldEmitCounter_ForCounterMetric();
    void Format_ShouldEmitGauge_ForGaugeMetric();
    void Format_ShouldEmitTimerSeries_ForTimerMetric();
    void Format_ShouldReturnEmpty_WhenNoMetrics();
    void Format_ShouldThrow_WhenMetricsIsNull();
    void Format_ShouldSanitizeMetricNames_WithSpecialChars();
    void Format_ShouldHandleTaggedKeys();
}