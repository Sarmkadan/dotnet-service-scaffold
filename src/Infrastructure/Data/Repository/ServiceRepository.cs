#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Service-specific repository with health check and metric queries.
/// </summary>
public class ServiceRepository : Repository<ServiceRegistration>, IServiceRepository
{
    public ServiceRepository(ServiceScaffoldDbContext context) : base(context)
    {
    }

    public async Task<ServiceRegistration?> GetByNameAsync(string serviceName)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.ServiceName == serviceName);
    }

    public async Task<IEnumerable<ServiceRegistration>> GetByStatusAsync(ServiceStatus status)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .OrderBy(s => s.ServiceName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceRegistration>> GetEnabledServicesAsync()
    {
        return await _dbSet
            .Where(s => s.IsEnabled)
            .OrderBy(s => s.ServiceName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceRegistration>> GetByOwnerAsync(Guid ownerId)
    {
        return await _dbSet
            .Where(s => s.OwnerId == ownerId)
            .OrderBy(s => s.ServiceName)
            .ToListAsync();
    }

    public async Task<ServiceRegistration?> GetWithMetricsAsync(Guid serviceId, int metricsCount = 10)
    {
        return await _dbSet
            .Include(s => s.Metrics.OrderByDescending(m => m.RecordedAt).Take(metricsCount))
            .FirstOrDefaultAsync(s => s.Id == serviceId);
    }

    public async Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync()
    {
        return await _dbSet
            .Where(s => s.IsEnabled &&
                        (s.Status == ServiceStatus.Unhealthy ||
                         s.Status == ServiceStatus.Degraded))
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceRegistration>> GetServicesWithoutRecentHealthCheckAsync(int minutesThreshold = 5)
    {
        var threshold = DateTime.UtcNow.AddMinutes(-minutesThreshold);

        return await _dbSet
            .Where(s => s.IsEnabled &&
                        (s.LastHealthCheckAt is null ||
                         s.LastHealthCheckAt < threshold))
            .OrderBy(s => s.LastHealthCheckAt)
            .ToListAsync();
    }
}
