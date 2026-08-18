using System;
using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ServiceMetricTests
{
    private ServiceMetric CreateValidMetric()
    {
        return new ServiceMetric
        {
            Id = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            CpuUsagePercent = 10.0m,
            MemoryUsagePercent = 20.0m,
            MemoryUsageBytes = 1024 * 1024 * 100, // 100MB
            DiskUsagePercent = 5.0m,
            DiskUsageBytes = 1024 * 1024 * 1024, // 1GB
            ActiveConnections = 10,
            RequestsPerSecond = 50,
            AverageResponseTimeMs = 100.0m,
            TotalRequests = 1000,
            ErrorCount = 5,
            RecordedAt = DateTime.UtcNow,
            Uptime = 99.99
        };
    }

    [Fact]
    public void HasAnomalies_ReturnsTrue_WhenCpuUsageHigh()
    {
        var metric = CreateValidMetric();
        metric.CpuUsagePercent = 90.0m;

        Assert.True(metric.HasAnomalies());
    }

    [Fact]
    public void HasAnomalies_ReturnsFalse_WhenAllMetricsNormal()
    {
        var metric = CreateValidMetric();

        Assert.False(metric.HasAnomalies());
    }

    [Fact]
    public void GetErrorRate_ReturnsZero_WhenNoRequests()
    {
        var metric = CreateValidMetric();
        metric.TotalRequests = 0;
        metric.ErrorCount = 5;

        var rate = metric.GetErrorRate();

        Assert.Equal(0m, rate);
    }

    [Fact]
    public void GetErrorRate_ReturnsCorrectPercentage_WhenRequestsMade()
    {
        var metric = CreateValidMetric();
        metric.TotalRequests = 200;
        metric.ErrorCount = 10;

        var rate = metric.GetErrorRate();

        Assert.Equal(5.0m, rate);
    }

    [Fact]
    public void GetSeverityRating_ReturnsCritical_WhenCpuUsageVeryHigh()
    {
        var metric = CreateValidMetric();
        metric.CpuUsagePercent = 95.0m;

        var rating = metric.GetSeverityRating();

        Assert.Equal("Critical", rating);
    }

    [Fact]
    public void FormatMetrics_ReturnsExpectedStringFormat()
    {
        var metric = CreateValidMetric();
        metric.CpuUsagePercent = 10.0m;
        metric.MemoryUsagePercent = 20.0m;
        metric.MemoryUsageBytes = 1024 * 1024 * 100;
        metric.DiskUsagePercent = 5.0m;
        metric.RequestsPerSecond = 50;
        metric.AverageResponseTimeMs = 100.0m;
        metric.ErrorCount = 5;
        metric.TotalRequests = 1000;
        metric.Uptime = 99.99;

        var formatted = metric.FormatMetrics();

        Assert.Contains("CPU: 10.0%", formatted);
        Assert.Contains("Memory: 20.0% (100MB)", formatted);
        Assert.Contains("Disk: 5.0%", formatted);
        Assert.Contains("RPS: 50", formatted);
        Assert.Contains("Avg Response: 100ms", formatted);
        Assert.Contains("Errors: 5/1000", formatted);
        Assert.Contains("Uptime: 99.99%", formatted);
    }
}
