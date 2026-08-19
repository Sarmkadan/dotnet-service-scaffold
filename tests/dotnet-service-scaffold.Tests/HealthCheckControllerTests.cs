#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Presentation.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class HealthCheckControllerTests
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<HealthCheckController> _logger;
    private readonly HealthCheckController _controller;

    public HealthCheckControllerTests()
    {
        _healthCheckService = Substitute.For<IHealthCheckService>();
        _logger = Substitute.For<ILogger<HealthCheckController>>();
        _controller = new HealthCheckController(_healthCheckService, _logger);
    }

    [Fact]
    public async Task CheckServiceHealth_ReturnsOk_WhenServiceExists()
    {
        var serviceId = Guid.NewGuid();
        var expectedResult = new HealthCheckResult { Id = Guid.NewGuid(), Status = HealthStatus.Healthy, HttpStatusCode = 200 };
        _healthCheckService.PerformHealthCheckAsync(serviceId).Returns(Task.FromResult(expectedResult));

        var result = await _controller.CheckServiceHealth(serviceId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CheckServiceHealth_ReturnsNotFound_WhenServiceDoesNotExist()
    {
        var serviceId = Guid.NewGuid();
        _healthCheckService.PerformHealthCheckAsync(serviceId)
            .Returns(Task.FromException<HealthCheckResult>(new ServiceNotFoundException("Service not found")));

        var result = await _controller.CheckServiceHealth(serviceId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetHealthHistory_ReturnsOk_WhenServiceExists()
    {
        var serviceId = Guid.NewGuid();
        var expectedHistory = new List<HealthCheckResult> { new HealthCheckResult { Id = Guid.NewGuid() } };
        _healthCheckService.GetServiceHealthHistoryAsync(serviceId, Arg.Any<int>()).Returns(Task.FromResult<IEnumerable<HealthCheckResult>>(expectedHistory));

        var result = await _controller.GetHealthHistory(serviceId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetHealthStatus_ReturnsOk_WhenServiceExists()
    {
        var serviceId = Guid.NewGuid();
        _healthCheckService.GetServiceHealthStatusAsync(serviceId).Returns(Task.FromResult("Healthy"));
        _healthCheckService.GetServiceSuccessRateAsync(serviceId, Arg.Any<int>()).Returns(Task.FromResult(100.0m));

        var result = await _controller.GetHealthStatus(serviceId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetFailedChecks_ReturnsOk_WhenServiceExists()
    {
        var serviceId = Guid.NewGuid();
        var expectedFailures = new List<HealthCheckResult> { new HealthCheckResult { Id = Guid.NewGuid() } };
        _healthCheckService.GetFailedChecksAsync(serviceId, Arg.Any<int>()).Returns(Task.FromResult<IEnumerable<HealthCheckResult>>(expectedFailures));

        var result = await _controller.GetFailedChecks(serviceId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CheckServiceHealth_ReturnsStatusCode500_OnScaffoldException()
    {
        var serviceId = Guid.NewGuid();
        _healthCheckService.PerformHealthCheckAsync(serviceId)
            .Returns(Task.FromException<HealthCheckResult>(new ServiceScaffoldException("Internal error")));

        var result = await _controller.CheckServiceHealth(serviceId);

        result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)result).StatusCode.Should().Be(500);
    }
}
