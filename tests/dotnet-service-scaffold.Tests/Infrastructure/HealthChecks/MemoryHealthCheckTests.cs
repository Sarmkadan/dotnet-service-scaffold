using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.HealthChecks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.HealthChecks;

/// <summary>
/// Test class for <see cref="MemoryHealthCheck"/>.
/// </summary>
public class MemoryHealthCheckTests
{
    /// <summary>
    /// Verifies that the health check returns a healthy status when using default thresholds.
    /// </summary>
    [Fact]
    public async Task CheckHealthAsync_WithDefaultThresholds_ReturnsHealthyStatus()
    {
        // Arrange
        var check = new MemoryHealthCheck();

        // Act
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("memoryLoadBytes");
        result.Data.Should().ContainKey("memoryLoadMB");
        result.Data.Should().ContainKey("totalAvailableMemoryBytes");
        result.Data.Should().ContainKey("totalAvailableMemoryMB");
        result.Data.Should().ContainKey("memoryUsagePercent");
        result.Data.Should().ContainKey("healthyThresholdPercent");
        result.Data.Should().ContainKey("degradedThresholdPercent");
        result.Data.Should().ContainKey("unhealthyThresholdPercent");
    }

    /// <summary>
    /// Verifies that the health check result contains expected memory metrics.
    /// </summary>
    [Fact]
    public async Task CheckHealthAsync_ReturnsDataWithMemoryMetrics()
    {
        // Arrange
        var check = new MemoryHealthCheck();

        // Act
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Data.Should().ContainKey("memoryLoadBytes").WhoseValue.Should().BeOfType<long>();
        result.Data.Should().ContainKey("memoryLoadMB").WhoseValue.Should().BeOfType<long>();
        result.Data.Should().ContainKey("totalAvailableMemoryBytes").WhoseValue.Should().BeOfType<long>();
        result.Data.Should().ContainKey("totalAvailableMemoryMB").WhoseValue.Should().BeOfType<long>();
        result.Data.Should().ContainKey("heapSizeBytes").WhoseValue.Should().BeOfType<long>();
        result.Data.Should().ContainKey("heapSizeMB").WhoseValue.Should().BeOfType<long>();
        result.Data.Should().ContainKey("memoryUsagePercent").WhoseValue.Should().BeOfType<double>();
    }

    /// <summary>
    /// Verifies that the constructor does not throw when provided with valid threshold values.
    /// </summary>
    [Fact]
    public void Constructor_WithValidThresholds_DoesNotThrow()
    {
        // Arrange & Act & Assert - no exception thrown
        var check1 = new MemoryHealthCheck(healthyThresholdPercent: 10, degradedThresholdPercent: 50, unhealthyThresholdPercent: 90);
        var check2 = new MemoryHealthCheck(healthyThresholdPercent: 70, degradedThresholdPercent: 80, unhealthyThresholdPercent: 95);
        var check3 = new MemoryHealthCheck(healthyThresholdPercent: 1, degradedThresholdPercent: 2, unhealthyThresholdPercent: 3);
    }

    /// <summary>
    /// Verifies that the constructor throws an <see cref="ArgumentOutOfRangeException"/> when provided with invalid threshold values.
    /// </summary>
    [Fact]
    public void Constructor_WithInvalidThresholds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryHealthCheck(healthyThresholdPercent: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryHealthCheck(healthyThresholdPercent: 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryHealthCheck(healthyThresholdPercent: 50, degradedThresholdPercent: 40));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryHealthCheck(healthyThresholdPercent: 50, degradedThresholdPercent: 50));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryHealthCheck(healthyThresholdPercent: 50, degradedThresholdPercent: 99, unhealthyThresholdPercent: 99));
    }
}