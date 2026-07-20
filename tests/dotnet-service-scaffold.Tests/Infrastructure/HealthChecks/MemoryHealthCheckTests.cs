using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.HealthChecks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.HealthChecks;

public class MemoryHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenMemoryUsageIsLow_ReturnsHealthy()
    {
        // Arrange
        var check = new MemoryHealthCheck(degradedThresholdBytes: 1024 * 1024 * 1024); // 1 GB threshold

        // Act
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("totalMemoryBytes");
        result.Data.Should().ContainKey("totalMemoryMB");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenMemoryUsageIsBetweenDegradedAndUnhealthy_ReturnsDegraded()
    {
        // We cannot easily control GC.GetTotalMemory in a unit test without complex mocking,
        // so we use a very low threshold to trigger the degraded state.
        
        // Arrange
        var check = new MemoryHealthCheck(degradedThresholdBytes: 1); // 1 byte threshold

        // Act
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data.Should().ContainKey("totalMemoryBytes");
    }
}
