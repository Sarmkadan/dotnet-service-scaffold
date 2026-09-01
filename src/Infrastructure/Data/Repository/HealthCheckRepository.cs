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
        _logger.LogInformation("Getting health check results for service {ServiceId}", serviceId);
        try
        {
            return await _dbSet
                .Where(h => h.ServiceId == serviceId)
                .OrderByDescending(h => h.CheckedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get health check results for service {ServiceId}", serviceId);
            throw;
        }
    }

    public async Task<IEnumerable<HealthCheckResult>> GetRecentResultsAsync(Guid serviceId, int count = 20, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recent health check results for service {ServiceId} with count {Count}", serviceId, count);
        try
        {
            return await _dbSet
                .Where(h => h.ServiceId == serviceId)
                .OrderByDescending(h => h.CheckedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent health check results for service {ServiceId}", serviceId);
            throw;
        }
    }

    public async Task<HealthCheckResult?> GetLatestResultAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting latest health check result for service {ServiceId}", serviceId);
        try
        {
            return await _dbSet
                .Where(h => h.ServiceId == serviceId)
                .OrderByDescending(h => h.CheckedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest health check result for service {ServiceId}", serviceId);
            throw;
        }
    }

    public async Task<IEnumerable<HealthCheckResult>> GetFailedResultsAsync(Guid serviceId, int hoursBack = 24, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting failed health check results for service {ServiceId} from the last {HoursBack} hours", serviceId, hoursBack);
        cancellationToken.ThrowIfCancellationRequested();
        var threshold = DateTime.UtcNow.AddHours(-hoursBack);

        try
        {
            return await _dbSet
                .Where(h => h.ServiceId == serviceId &&
                            h.CheckedAt >= threshold &&
                            !h.IsHealthy())
                .OrderByDescending(h => h.CheckedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get failed health check results for service {ServiceId}", serviceId);
            throw;
        }
    }

    public async Task<decimal> GetAverageResponseTimeAsync(Guid serviceId, int minutesBack = 60, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting average response time for service {ServiceId} from the last {MinutesBack} minutes", serviceId, minutesBack);
        cancellationToken.ThrowIfCancellationRequested();
        var threshold = DateTime.UtcNow.AddMinutes(-minutesBack);

        try
        {
            var result = await _dbSet
                .Where(h => h.ServiceId == serviceId && h.CheckedAt >= threshold)
                .AverageAsync(h => (decimal)h.ResponseTimeMs, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get average response time for service {ServiceId}", serviceId);
            throw;
        }
    }

    public async Task<int> GetFailureCountAsync(Guid serviceId, int minutesBack = 60)
    {
        _logger.LogInformation("Getting failure count for service {ServiceId} from the last {MinutesBack} minutes", serviceId, minutesBack);
        var threshold = DateTime.UtcNow.AddMinutes(-minutesBack);

        try
        {
            return await _dbSet
                .CountAsync(h => h.ServiceId == serviceId &&
                                h.CheckedAt >= threshold &&
                                !h.IsHealthy());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get failure count for service {ServiceId}", serviceId);
            throw;
        }
    }

    public async Task DeleteOldResultsAsync(Guid serviceId, int daysToKeep = 30)
    {
        _logger.LogInformation("Deleting old health check results for service {ServiceId} older than {DaysToKeep} days", serviceId, daysToKeep);
        var threshold = DateTime.UtcNow.AddDays(-daysToKeep);

        try
        {
            var oldResults = await _dbSet
                .Where(h => h.ServiceId == serviceId && h.CheckedAt < threshold)
                .ToListAsync();

            foreach (var result in oldResults)
            {
                _dbSet.Remove(result);
            }

            await SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete old health check results for service {ServiceId}", serviceId);
            throw;
        }
    }
}