#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Repository interface for HealthCheckResult entity operations.
/// </summary>
public interface IHealthCheckRepository : IRepository<HealthCheckResult>
{
    Task<IEnumerable<HealthCheckResult>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<IEnumerable<HealthCheckResult>> GetRecentResultsAsync(Guid serviceId, int count = 20, CancellationToken cancellationToken = default);

    Task<HealthCheckResult?> GetLatestResultAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<IEnumerable<HealthCheckResult>> GetFailedResultsAsync(Guid serviceId, int hoursBack = 24, CancellationToken cancellationToken = default);

    Task<decimal> GetAverageResponseTimeAsync(Guid serviceId, int minutesBack = 60, CancellationToken cancellationToken = default);

    Task<int> GetFailureCountAsync(Guid serviceId, int minutesBack = 60);

    Task DeleteOldResultsAsync(Guid serviceId, int daysToKeep = 30);
}
