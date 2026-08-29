#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Health check result repository with analytics and historical queries.
/// </summary>
public class HealthCheckRepository : Repository<HealthCheckResult>, IHealthCheckRepository
{
    public HealthCheckRepository(ServiceScaffoldDbContext context, ILogger<HealthCheckRepository> logger) : base(context, logger)
    {
    }

    public async Task<IEnumerable<HealthCheckResult>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.ServiceId == serviceId)
            .OrderByDescending(h => h.CheckedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HealthCheckResult>> GetRecentResultsAsync(Guid serviceId, int count = 20, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.ServiceId == serviceId)
            .OrderByDescending(h => h.CheckedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<HealthCheckResult?> GetLatestResultAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.ServiceId == serviceId)
            .OrderByDescending(h => h.CheckedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<HealthCheckResult>> GetFailedResultsAsync(Guid serviceId, int hoursBack = 24, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var threshold = DateTime.UtcNow.AddHours(-hoursBack);

        return await _dbSet
            .Where(h => h.ServiceId == serviceId &&
                        h.CheckedAt >= threshold &&
                        !h.IsHealthy())
            .OrderByDescending(h => h.CheckedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetAverageResponseTimeAsync(Guid serviceId, int minutesBack = 60, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var threshold = DateTime.UtcNow.AddMinutes(-minutesBack);

        var result = await _dbSet
            .Where(h => h.ServiceId == serviceId && h.CheckedAt >= threshold)
            .AverageAsync(h => (decimal)h.ResponseTimeMs, cancellationToken);

        return result;
    }

    public async Task<int> GetFailureCountAsync(Guid serviceId, int minutesBack = 60)
    {
        var threshold = DateTime.UtcNow.AddMinutes(-minutesBack);

        return await _dbSet
            .CountAsync(h => h.ServiceId == serviceId &&
                            h.CheckedAt >= threshold &&
                            !h.IsHealthy());
    }

    public async Task DeleteOldResultsAsync(Guid serviceId, int daysToKeep = 30)
    {
        var threshold = DateTime.UtcNow.AddDays(-daysToKeep);

        var oldResults = await _dbSet
            .Where(h => h.ServiceId == serviceId && h.CheckedAt < threshold)
            .ToListAsync();

        foreach (var result in oldResults)
        {
            _dbSet.Remove(result);
        }

        await SaveChangesAsync();
    }
}
