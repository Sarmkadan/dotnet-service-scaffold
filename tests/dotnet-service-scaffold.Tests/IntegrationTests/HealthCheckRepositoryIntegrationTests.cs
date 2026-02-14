#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DotnetServiceScaffold.Tests.IntegrationTests;

public class HealthCheckRepositoryIntegrationTests : IntegrationTestBase
{
    private readonly HealthCheckRepository _healthCheckRepository;

    public HealthCheckRepositoryIntegrationTests()
    {
        _healthCheckRepository = new HealthCheckRepository(DbContext);
    }

    [Fact]
    public async Task AddHealthCheckResultAsync_ShouldAddResultToDatabase()
    {
        // Arrange
        var result = new HealthCheckResult
        {
            ServiceId = Guid.NewGuid(),
            Status = HealthStatus.Healthy,
            CheckedAt = DateTime.UtcNow,
            ResponseTimeMs = 100,
            Details = "Service is up"
        };

        // Act
        await _healthCheckRepository.AddHealthCheckResultAsync(result);

        // Assert
        var savedResult = await DbContext.HealthCheckResults.FirstOrDefaultAsync(h => h.ServiceId == result.ServiceId);
        savedResult.Should().NotBeNull();
        savedResult.ServiceId.Should().Be(result.ServiceId);
        savedResult.Status.Should().Be(result.Status);
        savedResult.ResponseTimeMs.Should().Be(result.ResponseTimeMs);
        savedResult.Details.Should().Be(result.Details);
    }

    [Fact]
    public async Task GetHealthCheckResultsForServiceAsync_ShouldReturnResultsForService()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var result1 = new HealthCheckResult { ServiceId = serviceId, Status = HealthStatus.Healthy, CheckedAt = DateTime.UtcNow.AddMinutes(-5) };
        var result2 = new HealthCheckResult { ServiceId = serviceId, Status = HealthStatus.Unhealthy, CheckedAt = DateTime.UtcNow.AddMinutes(-2) };
        var result3 = new HealthCheckResult { ServiceId = Guid.NewGuid(), Status = HealthStatus.Healthy, CheckedAt = DateTime.UtcNow };

        await DbContext.HealthCheckResults.AddRangeAsync(result1, result2, result3);
        await DbContext.SaveChangesAsync();

        // Act
        var results = await _healthCheckRepository.GetHealthCheckResultsForServiceAsync(serviceId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().ContainEquivalentOf(result1);
        results.Should().ContainEquivalentOf(result2);
        results.Should().NotContainEquivalentOf(result3);
    }

    [Fact]
    public async Task GetHealthCheckResultsForServiceAsync_ShouldReturnEmpty_WhenNoResults()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var results = await _healthCheckRepository.GetHealthCheckResultsForServiceAsync(serviceId);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLatestHealthCheckResultForServiceAsync_ShouldReturnLatestResult()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var result1 = new HealthCheckResult { ServiceId = serviceId, Status = HealthStatus.Healthy, CheckedAt = DateTime.UtcNow.AddMinutes(-10) };
        var result2 = new HealthCheckResult { ServiceId = serviceId, Status = HealthStatus.Unhealthy, CheckedAt = DateTime.UtcNow.AddMinutes(-5) };
        var result3 = new HealthCheckResult { ServiceId = serviceId, Status = HealthStatus.Healthy, CheckedAt = DateTime.UtcNow.AddMinutes(-1) };

        await DbContext.HealthCheckResults.AddRangeAsync(result1, result2, result3);
        await DbContext.SaveChangesAsync();

        // Act
        var latestResult = await _healthCheckRepository.GetLatestHealthCheckResultForServiceAsync(serviceId);

        // Assert
        latestResult.Should().NotBeNull();
        latestResult.Should().BeEquivalentTo(result3);
    }

    [Fact]
    public async Task GetLatestHealthCheckResultForServiceAsync_ShouldReturnNull_WhenNoResults()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var latestResult = await _healthCheckRepository.GetLatestHealthCheckResultForServiceAsync(serviceId);

        // Assert
        latestResult.Should().BeNull();
    }

    [Fact]
    public async Task DeleteHealthCheckResultAsync_ShouldRemoveResultFromDatabase()
    {
        // Arrange
        var result = new HealthCheckResult
        {
            ServiceId = Guid.NewGuid(),
            Status = HealthStatus.Healthy,
            CheckedAt = DateTime.UtcNow,
            ResponseTimeMs = 100,
            Details = "Service is up"
        };
        await DbContext.HealthCheckResults.AddAsync(result);
        await DbContext.SaveChangesAsync();
        var resultId = result.Id;

        // Act
        await _healthCheckRepository.DeleteHealthCheckResultAsync(resultId);

        // Assert
        var deletedResult = await DbContext.HealthCheckResults.FindAsync(resultId);
        deletedResult.Should().BeNull();
    }
}
