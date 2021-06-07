#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Health check result repository with analytics and historical queries.
/// </summary>
public class HealthCheckRepository : Repository<HealthCheckResult>, IHealthCheckRepository
{
    public HealthCheckRepository(ServiceScaffoldDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<HealthCheckResult>> GetByServiceIdAsync(Guid serviceId)
    {
        return await _dbSet
            .Where(h => h.ServiceId == serviceId)
            .OrderByDescending(h => h.CheckedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<HealthCheckResult>> GetRecentResultsAsync(Guid serviceId, int count = 20)
    {
        return await _dbSet
            .Where(h => h.ServiceId == serviceId)
            .OrderByDescending(h => h.CheckedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<HealthCheckResult?> GetLatestResultAsync(Guid serviceId)
    {
        return await _dbSet
            .Where(h => h.ServiceId == serviceId)
            .OrderByDescending(h => h.CheckedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<HealthCheckResult>> GetFailedResultsAsync(Guid serviceId, int hoursBack = 24)
    {
        var threshold = DateTime.UtcNow.AddHours(-hoursBack);

        return await _dbSet
            .Where(h => h.ServiceId == serviceId &&
                        h.CheckedAt >= threshold &&
                        !h.IsHealthy())
            .OrderByDescending(h => h.CheckedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageResponseTimeAsync(Guid serviceId, int minutesBack = 60)
    {
        var threshold = DateTime.UtcNow.AddMinutes(-minutesBack);

        var result = await _dbSet
            .Where(h => h.ServiceId == serviceId && h.CheckedAt >= threshold)
            .AverageAsync(h => (decimal)h.ResponseTimeMs);

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
