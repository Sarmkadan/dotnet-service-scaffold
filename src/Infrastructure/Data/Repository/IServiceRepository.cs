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
    Task<ServiceRegistration?> GetByNameAsync(string serviceName);

    Task<IEnumerable<ServiceRegistration>> GetByStatusAsync(ServiceStatus status);

    Task<IEnumerable<ServiceRegistration>> GetEnabledServicesAsync();

    Task<IEnumerable<ServiceRegistration>> GetByOwnerAsync(Guid ownerId);

    Task<ServiceRegistration?> GetWithMetricsAsync(Guid serviceId, int metricsCount = 10);

    Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync();

    Task<IEnumerable<ServiceRegistration>> GetServicesWithoutRecentHealthCheckAsync(int minutesThreshold = 5);
}
