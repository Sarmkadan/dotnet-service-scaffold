#nullable enable

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Interface for HealthCheckServiceTests.
/// </summary>
public interface IHealthCheckServiceTests
{
    Task PerformHealthCheckAsync_ShouldReturnHealthy_WhenAllComponentsAreHealthy();
    Task PerformHealthCheckAsync_ShouldReturnUnhealthy_WhenAnyComponentIsUnhealthy();
    Task GetHealthCheckHistoryAsync_ShouldReturnAllHistory();
    Task GetHealthCheckHistoryAsync_ShouldReturnEmpty_WhenNoHistoryExists();
    Task RecordHealthCheckResultAsync_ShouldCallRepositoryAdd();
    Task RecordHealthCheckResultAsync_ShouldSetTimestamp();
}