using System;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class HealthCheckResultTests
{
    [Fact]
    public void IsHealthy_ReturnsTrue_WhenStatusHealthyAndHttpCodeInRange()
    {
        var result = new HealthCheckResult
        {
            Status = HealthStatus.Healthy,
            HttpStatusCode = 200
        };

        Assert.True(result.IsHealthy());

        result.HttpStatusCode = 299;
        Assert.True(result.IsHealthy());
    }

    [Fact]
    public void IsHealthy_ReturnsFalse_WhenStatusNotHealthyOrHttpCodeOutOfRange()
    {
        var result = new HealthCheckResult
        {
            Status = HealthStatus.Unhealthy,
            HttpStatusCode = 200
        };
        Assert.False(result.IsHealthy());

        result.Status = HealthStatus.Healthy;
        result.HttpStatusCode = 199;
        Assert.False(result.IsHealthy());

        result.HttpStatusCode = 300;
        Assert.False(result.IsHealthy());

        result.HttpStatusCode = null;
        Assert.False(result.IsHealthy());
    }

    [Fact]
    public void IsResponseTimeAcceptable_UsesDefaultThreshold()
    {
        var result = new HealthCheckResult { ResponseTimeMs = 4000 };
        Assert.True(result.IsResponseTimeAcceptable());

        result.ResponseTimeMs = 5000;
        Assert.True(result.IsResponseTimeAcceptable());

        result.ResponseTimeMs = 5001;
        Assert.False(result.IsResponseTimeAcceptable());
    }

    [Fact]
    public void IsResponseTimeAcceptable_RespectsCustomThreshold()
    {
        var result = new HealthCheckResult { ResponseTimeMs = 1500 };
        Assert.True(result.IsResponseTimeAcceptable(1000));
        Assert.False(result.IsResponseTimeAcceptable(1000));
    }

    [Fact]
    public void AreResourcesHealthy_ReturnsTrue_WhenNoValuesOrWithinThresholds()
    {
        var result = new HealthCheckResult();
        Assert.True(result.AreResourcesHealthy());

        result.CpuUsagePercent = 50;
        result.MemoryUsagePercent = 40;
        Assert.True(result.AreResourcesHealthy());
    }

    [Fact]
    public void AreResourcesHealthy_ReturnsFalse_WhenCpuOrMemoryExceedsThreshold()
    {
        var result = new HealthCheckResult { CpuUsagePercent = 95 };
        Assert.False(result.AreResourcesHealthy());

        result.CpuUsagePercent = 85;
        result.MemoryUsagePercent = 90;
        Assert.False(result.AreResourcesHealthy());
    }

    [Fact]
    public void GetSummary_IncludesAllRelevantFields()
    {
        var result = new HealthCheckResult
        {
            Status = HealthStatus.Healthy,
            HttpStatusCode = 201,
            ResponseTimeMs = 123,
            CpuUsagePercent = 12.3m,
            MemoryUsagePercent = 45.6m,
            ErrorMessage = "Something went wrong"
        };

        var summary = result.GetSummary();

        Assert.Contains("Status: Healthy", summary);
        Assert.Contains("HTTP 201", summary);
        Assert.Contains("Response Time: 123ms", summary);
        Assert.Contains("CPU: 12.3%", summary);
        Assert.Contains("Memory: 45.6%", summary);
        Assert.Contains("Error: Something went wrong", summary);
    }

    [Fact]
    public void GetSummary_OmitsOptionalFieldsWhenNull()
    {
        var result = new HealthCheckResult
        {
            Status = HealthStatus.Unhealthy,
            ResponseTimeMs = 999
        };

        var summary = result.GetSummary();

        Assert.DoesNotContain("HTTP", summary);
        Assert.DoesNotContain("CPU:", summary);
        Assert.DoesNotContain("Memory:", summary);
        Assert.DoesNotContain("Error:", summary);
        Assert.Contains("Status: Unhealthy", summary);
        Assert.Contains("Response Time: 999ms", summary);
    }
}
