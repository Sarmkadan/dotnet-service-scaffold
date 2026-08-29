#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Repository interface for ServiceRegistration entity operations.
/// </summary>
public interface IServiceRepository : IRepository<ServiceRegistration>
{
    Task<ServiceRegistration?> GetByNameAsync(string serviceName, CancellationToken cancellationToken = default);

    Task<IEnumerable<ServiceRegistration>> GetByStatusAsync(ServiceStatus status, CancellationToken cancellationToken = default);

    Task<IEnumerable<ServiceRegistration>> GetEnabledServicesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<ServiceRegistration>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task<ServiceRegistration?> GetWithMetricsAsync(Guid serviceId, int metricsCount = 10, CancellationToken cancellationToken = default);

    Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync();

    Task<IEnumerable<ServiceRegistration>> GetServicesWithoutRecentHealthCheckAsync(int minutesThreshold = 5);
}
