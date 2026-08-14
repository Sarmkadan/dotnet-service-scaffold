using DotnetServiceScaffold.Infrastructure.Metrics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetServiceScaffold.Tests;

public class MetricsServiceExtensionsTests
{
    private readonly MetricsService _service;

    public MetricsServiceExtensionsTests()
    {
        var logger = new Mock<ILogger<MetricsService>>();
        _service = new MetricsService(logger.Object);
    }

    [Fact]
    public async Task IncrementCounter_AddsValue()
    {
        _service.IncrementCounter("test_counter", 5);

        var metrics = await _service.GetMetricsAsync();
        metrics.Should().ContainKey("test_counter");
        System.Text.Json.JsonSerializer.Serialize(metrics["test_counter"]).Should().Contain("\"value\":5");
    }

    [Fact]
    public async Task RecordGauge_SetsValue()
    {
        _service.RecordGauge("test_gauge", 10.5);

        var metrics = await _service.GetMetricsAsync();
        metrics.Should().ContainKey("test_gauge");
        System.Text.Json.JsonSerializer.Serialize(metrics["test_gauge"]).Should().Contain("\"value\":10.5");
    }

    [Fact]
    public async Task RecordTiming_RecordsElapsedTime()
    {
        _service.RecordTiming("test_timer", 100);

        var metrics = await _service.GetMetricsAsync();
        metrics.Should().ContainKey("test_timer");
        System.Text.Json.JsonSerializer.Serialize(metrics["test_timer"]).Should().Contain("\"type\":\"timer\"");
    }

    [Fact]
    public async Task MeasureAsyncT_RecordsTimeAndReturnsResult()
    {
        var result = await _service.MeasureAsync("test_measure", async () =>
        {
            await Task.Delay(10);
            return 42;
        });

        result.Should().Be(42);
        var metrics = await _service.GetMetricsAsync();
        metrics.Should().ContainKey("test_measure");
        System.Text.Json.JsonSerializer.Serialize(metrics["test_measure"]).Should().Contain("\"type\":\"timer\"");
    }

    [Fact]
    public async Task Increment_AddsOne()
    {
        _service.Increment("test_inc");

        var metrics = await _service.GetMetricsAsync();
        metrics.Should().ContainKey("test_inc");
        System.Text.Json.JsonSerializer.Serialize(metrics["test_inc"]).Should().Contain("\"value\":1");
    }

    [Fact]
    public async Task RecordGaugeZero_SetsZero()
    {
        _service.RecordGaugeZero("test_gauge_zero");

        var metrics = await _service.GetMetricsAsync();
        metrics.Should().ContainKey("test_gauge_zero");
        System.Text.Json.JsonSerializer.Serialize(metrics["test_gauge_zero"]).Should().Contain("\"value\":0");
    }

    [Fact]
    public async Task MeasureAsyncVoid_RecordsTime()
    {
        await _service.MeasureAsync("test_measure_void", async () =>
        {
            await Task.Delay(10);
        });

        var metrics = await _service.GetMetricsAsync();
        metrics.Should().ContainKey("test_measure_void");
        System.Text.Json.JsonSerializer.Serialize(metrics["test_measure_void"]).Should().Contain("\"type\":\"timer\"");
    }

    [Fact]
    public async Task RecordActionTime_RecordsTime()
    {
        _service.RecordActionTime("test_action_time", () =>
        {
            Thread.Sleep(10);
        });

        var metrics = await _service.GetMetricsAsync();
        metrics.Should().ContainKey("test_action_time");
        System.Text.Json.JsonSerializer.Serialize(metrics["test_action_time"]).Should().Contain("\"type\":\"timer\"");
    }
}
