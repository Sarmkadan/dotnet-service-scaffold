#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Service-specific repository with health check and metric queries.
/// </summary>
public class ServiceRepository : Repository<ServiceRegistration>, IServiceRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceRepository"/> class.
    /// </summary>
    /// <param name="context">The database context used to access service registrations.</param>
    /// <param name="logger">The logger used to record repository operations.</param>
    public ServiceRepository(ServiceScaffoldDbContext context, ILogger<ServiceRepository> logger) : base(context, logger)
    {
    }

    /// <summary>
    /// Retrieves a service registration by its service name.
    /// </summary>
    /// <param name="serviceName">The service name to search for.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The matching service registration, or <see langword="null"/> if no registration is found.</returns>
    /// <exception cref="ArgumentException"><paramref name="serviceName"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<ServiceRegistration?> GetByNameAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("GetByNameAsync called with {ServiceName}", serviceName);
        return await _dbSet.FirstOrDefaultAsync(s => s.ServiceName == serviceName, cancellationToken);
    }

    /// <summary>
    /// Retrieves service registrations with the specified status, ordered by service name.
    /// </summary>
    /// <param name="status">The service status to match.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A collection of service registrations with the specified status.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<IEnumerable<ServiceRegistration>> GetByStatusAsync(ServiceStatus status, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("GetByStatusAsync called with {Status}", status);
        return await _dbSet
            .Where(s => s.Status == status)
            .OrderBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves enabled service registrations, ordered by service name.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A collection of enabled service registrations.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<IEnumerable<ServiceRegistration>> GetEnabledServicesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("GetEnabledServicesAsync called");
        return await _dbSet
            .Where(s => s.IsEnabled)
            .OrderBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves service registrations owned by the specified owner, ordered by service name.
    /// </summary>
    /// <param name="ownerId">The unique identifier of the owner.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A collection of service registrations owned by the specified owner.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<IEnumerable<ServiceRegistration>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("GetByOwnerAsync called with {OwnerId}", ownerId);
        return await _dbSet
            .Where(s => s.OwnerId == ownerId)
            .OrderBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a service registration and its most recent metrics.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service registration.</param>
    /// <param name="metricsCount">The maximum number of recent metrics to include.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The matching service registration with its recent metrics, or <see langword="null"/> if no registration is found.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    public async Task<ServiceRegistration?> GetWithMetricsAsync(Guid serviceId, int metricsCount = 10, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("GetWithMetricsAsync called with {ServiceId} and {MetricsCount}", serviceId, metricsCount);
        return await _dbSet
            .Include(s => s.Metrics.OrderByDescending(m => m.RecordedAt).Take(metricsCount))
            .FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);
    }

    /// <summary>
    /// Retrieves enabled service registrations that are unhealthy or degraded, ordered by most recent update.
    /// </summary>
    /// <returns>A collection of unhealthy or degraded service registrations.</returns>
    public async Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync()
    {
        _logger.LogInformation("GetUnhealthyServicesAsync called");
        return await _dbSet
            .Where(s => s.IsEnabled &&
                        (s.Status == ServiceStatus.Unhealthy ||
                         s.Status == ServiceStatus.Degraded))
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves enabled service registrations that have not received a health check within the specified interval.
    /// </summary>
    /// <param name="minutesThreshold">The number of minutes used to determine whether a health check is recent.</param>
    /// <returns>A collection of service registrations without a recent health check.</returns>
    public async Task<IEnumerable<ServiceRegistration>> GetServicesWithoutRecentHealthCheckAsync(int minutesThreshold = 5)
    {
        _logger.LogInformation("GetServicesWithoutRecentHealthCheckAsync called with {MinutesThreshold}", minutesThreshold);
        var threshold = DateTime.UtcNow.AddMinutes(-minutesThreshold);

        return await _dbSet
            .Where(s => s.IsEnabled &&
                        (s.LastHealthCheckAt == null ||
                         s.LastHealthCheckAt < threshold))
            .OrderBy(s => s.LastHealthCheckAt)
            .ToListAsync();
    }
}
