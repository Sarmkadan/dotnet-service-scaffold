#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using NSubstitute;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;

namespace DotnetServiceScaffold.Tests.Application.Services;

public class HealthCheckServiceTests
{
    private readonly IHealthCheckRepository _healthCheckRepository;
    private readonly HealthCheckService _healthCheckService;

    public HealthCheckServiceTests()
    {
        _healthCheckRepository = Substitute.For<IHealthCheckRepository>();
        _healthCheckService = new HealthCheckService(_healthCheckRepository);
    }

    [Fact]
    public async Task PerformHealthCheckAsync_ShouldReturnHealthy_WhenAllComponentsAreHealthy()
    {
        // Arrange
        _healthCheckRepository.GetAllHealthChecksAsync().Returns(new List<HealthCheckResult>
        {
            new HealthCheckResult { Component = "DB", Status = HealthStatus.Healthy },
            new HealthCheckResult { Component = "API", Status = HealthStatus.Healthy }
        });

        // Act
        var result = await _healthCheckService.PerformHealthCheckAsync();

        // Assert
        result.OverallStatus.Should().Be(HealthStatus.Healthy);
        result.Results.Should().HaveCount(2);
        result.Results.All(r => r.Status == HealthStatus.Healthy).Should().BeTrue();
    }

    [Fact]
    public async Task PerformHealthCheckAsync_ShouldReturnUnhealthy_WhenAnyComponentIsUnhealthy()
    {
        // Arrange
        _healthCheckRepository.GetAllHealthChecksAsync().Returns(new List<HealthCheckResult>
        {
            new HealthCheckResult { Component = "DB", Status = HealthStatus.Healthy },
            new HealthCheckResult { Component = "API", Status = HealthStatus.Unhealthy }
        });

        // Act
        var result = await _healthCheckService.PerformHealthCheckAsync();

        // Assert
        result.OverallStatus.Should().Be(HealthStatus.Unhealthy);
        result.Results.Should().HaveCount(2);
        result.Results.Any(r => r.Status == HealthStatus.Unhealthy).Should().BeTrue();
    }

    [Fact]
    public async Task GetHealthCheckHistoryAsync_ShouldReturnAllHistory()
    {
        // Arrange
        var history = new List<HealthCheckResult>
        {
            new HealthCheckResult { Component = "DB", Status = HealthStatus.Healthy, Timestamp = DateTime.UtcNow.AddMinutes(-5) },
            new HealthCheckResult { Component = "API", Status = HealthStatus.Unhealthy, Timestamp = DateTime.UtcNow.AddMinutes(-10) }
        };
        _healthCheckRepository.GetAllHealthChecksAsync().Returns(history);

        // Act
        var result = await _healthCheckService.GetHealthCheckHistoryAsync();

        // Assert
        result.Should().BeEquivalentTo(history);
        await _healthCheckRepository.Received(1).GetAllHealthChecksAsync();
    }

    [Fact]
    public async Task GetHealthCheckHistoryAsync_ShouldReturnEmpty_WhenNoHistoryExists()
    {
        // Arrange
        _healthCheckRepository.GetAllHealthChecksAsync().Returns(new List<HealthCheckResult>());

        // Act
        var result = await _healthCheckService.GetHealthCheckHistoryAsync();

        // Assert
        result.Should().BeEmpty();
        await _healthCheckRepository.Received(1).GetAllHealthChecksAsync();
    }

    [Fact]
    public async Task RecordHealthCheckResultAsync_ShouldCallRepositoryAdd()
    {
        // Arrange
        var result = new HealthCheckResult { Component = "Test", Status = HealthStatus.Healthy };
        _healthCheckRepository.AddHealthCheckResultAsync(Arg.Any<HealthCheckResult>()).Returns(Task.CompletedTask);

        // Act
        await _healthCheckService.RecordHealthCheckResultAsync(result.Component, result.Status, result.Message);

        // Assert
        await _healthCheckRepository.Received(1).AddHealthCheckResultAsync(
            Arg.Is<HealthCheckResult>(r => r.Component == result.Component && r.Status == result.Status));
    }

    [Fact]
    public async Task RecordHealthCheckResultAsync_ShouldSetTimestamp()
    {
        // Arrange
        var component = "TestComponent";
        var status = HealthStatus.Healthy;
        var message = "Test Message";

        _healthCheckRepository.AddHealthCheckResultAsync(Arg.Any<HealthCheckResult>()).Returns(Task.CompletedTask);

        // Act
        await _healthCheckService.RecordHealthCheckResultAsync(component, status, message);

        // Assert
        await _healthCheckRepository.Received(1).AddHealthCheckResultAsync(
            Arg.Is<HealthCheckResult>(r => r.Timestamp != default));
    }
}
