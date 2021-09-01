#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Extension methods for ServiceRepository providing additional query capabilities
/// and convenience methods for common service management operations.
/// </summary>
public static class ServiceRepositoryExtensions
{
    /// <summary>
    /// Gets a service registration by its name with optional tracking.
    /// </summary>
    /// <param name="repository">The service repository instance</param>
    /// <param name="serviceName">The name of the service to retrieve</param>
    /// <param name="tracking">Whether to enable change tracking</param>
    /// <returns>The service registration or null if not found</returns>
    public static async Task<ServiceRegistration?> GetByNameAsync(this ServiceRepository repository, string serviceName, bool tracking = true)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be null or whitespace", nameof(serviceName));

        IQueryable<ServiceRegistration> query = repository._context.Set<ServiceRegistration>();
        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(s => s.ServiceName == serviceName);
    }

    /// <summary>
    /// Gets all services with a specific status, ordered by name.
    /// </summary>
    /// <param name="repository">The service repository instance</param>
    /// <param name="status">The service status to filter by</param>
    /// <param name="tracking">Whether to enable change tracking</param>
    /// <returns>Collection of service registrations with the specified status</returns>
    public static async Task<IEnumerable<ServiceRegistration>> GetByStatusAsync(this ServiceRepository repository, ServiceStatus status, bool tracking = true)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        IQueryable<ServiceRegistration> query = repository._context.Set<ServiceRegistration>();
        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Where(s => s.Status == status)
            .OrderBy(s => s.ServiceName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all enabled services with their metrics included.
    /// </summary>
    /// <param name="repository">The service repository instance</param>
    /// <param name="metricsCount">Number of recent metrics to include per service</param>
    /// <param name="tracking">Whether to enable change tracking</param>
    /// <returns>Collection of enabled service registrations with metrics</returns>
    public static async Task<IEnumerable<ServiceRegistration>> GetEnabledServicesWithMetricsAsync(this ServiceRepository repository, int metricsCount = 10, bool tracking = true)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        IQueryable<ServiceRegistration> query = repository._context.Set<ServiceRegistration>();
        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Where(s => s.IsEnabled)
            .Include(s => s.Metrics.OrderByDescending(m => m.RecordedAt).Take(metricsCount))
            .OrderBy(s => s.ServiceName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets services by owner with optional status filtering.
    /// </summary>
    /// <param name="repository">The service repository instance</param>
    /// <param name="ownerId">The owner identifier</param>
    /// <param name="status">Optional status to filter by</param>
    /// <param name="tracking">Whether to enable change tracking</param>
    /// <returns>Collection of service registrations for the specified owner</returns>
    public static async Task<IEnumerable<ServiceRegistration>> GetByOwnerAsync(this ServiceRepository repository, Guid ownerId, ServiceStatus? status = null, bool tracking = true)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        IQueryable<ServiceRegistration> query = repository._context.Set<ServiceRegistration>();
        if (!tracking)
            query = query.AsNoTracking();

        query = query.Where(s => s.OwnerId == ownerId);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        return await query
            .OrderBy(s => s.ServiceName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets unhealthy services with optional owner filtering.
    /// </summary>
    /// <param name="repository">The service repository instance</param>
    /// <param name="ownerId">Optional owner identifier to filter by</param>
    /// <param name="tracking">Whether to enable change tracking</param>
    /// <returns>Collection of unhealthy service registrations</returns>
    public static async Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync(this ServiceRepository repository, Guid? ownerId = null, bool tracking = true)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        IQueryable<ServiceRegistration> query = repository._context.Set<ServiceRegistration>();
        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .Where(s => s.IsEnabled &&
                   (s.Status == ServiceStatus.Unhealthy || s.Status == ServiceStatus.Degraded));

        if (ownerId.HasValue)
            query = query.Where(s => s.OwnerId == ownerId.Value);

        return await query
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets services that haven't had a health check within the specified time threshold.
    /// </summary>
    /// <param name="repository">The service repository instance</param>
    /// <param name="minutesThreshold">Time threshold in minutes</param>
    /// <param name="ownerId">Optional owner identifier to filter by</param>
    /// <param name="tracking">Whether to enable change tracking</param>
    /// <returns>Collection of services without recent health checks</returns>
    public static async Task<IEnumerable<ServiceRegistration>> GetServicesWithoutRecentHealthCheckAsync(this ServiceRepository repository, int minutesThreshold = 5, Guid? ownerId = null, bool tracking = true)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var threshold = DateTime.UtcNow.AddMinutes(-minutesThreshold);
        IQueryable<ServiceRegistration> query = repository._context.Set<ServiceRegistration>();
        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .Where(s => s.IsEnabled &&
                   (s.LastHealthCheckAt == null || s.LastHealthCheckAt < threshold));

        if (ownerId.HasValue)
            query = query.Where(s => s.OwnerId == ownerId.Value);

        return await query
            .OrderBy(s => s.LastHealthCheckAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets services that are due for a health check based on their health check interval.
    /// </summary>
    /// <param name="repository">The service repository instance</param>
    /// <param name="defaultIntervalMinutes">Default health check interval in minutes</param>
    /// <param name="ownerId">Optional owner identifier to filter by</param>
    /// <param name="tracking">Whether to enable change tracking</param>
    /// <returns>Collection of services due for health check</returns>
    public static async Task<IEnumerable<ServiceRegistration>> GetServicesDueForHealthCheckAsync(this ServiceRepository repository, int defaultIntervalMinutes = 5, Guid? ownerId = null, bool tracking = true)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var threshold = DateTime.UtcNow.AddMinutes(-defaultIntervalMinutes);
        IQueryable<ServiceRegistration> query = repository._context.Set<ServiceRegistration>();
        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .Where(s => s.IsEnabled &&
                   s.HealthCheckIntervalSeconds > 0 &&
                   s.LastHealthCheckAt != null &&
                   s.LastHealthCheckAt.Value.AddSeconds(s.HealthCheckIntervalSeconds) < DateTime.UtcNow);

        if (ownerId.HasValue)
            query = query.Where(s => s.OwnerId == ownerId.Value);

        return await query
            .OrderBy(s => s.LastHealthCheckAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the count of services by status.
    /// </summary>
    /// <param name="repository">The service repository instance</param>
    /// <returns>Dictionary mapping service status to count</returns>
    public static async Task<Dictionary<ServiceStatus, int>> GetServiceCountsByStatusAsync(this ServiceRepository repository)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        return await repository._dbSet
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }
}