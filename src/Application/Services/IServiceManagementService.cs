#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service interface for service registration and lifecycle management.
/// </summary>
public interface IServiceManagementService
{
    Task<ServiceRegistration> RegisterServiceAsync(string serviceName, string endpoint, string healthCheckUrl, Guid ownerId, CancellationToken cancellationToken = default);

    Task<ServiceRegistration?> GetServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<ServiceRegistration?> GetServiceByNameAsync(string serviceName, CancellationToken cancellationToken = default);

    Task<IEnumerable<ServiceRegistration>> GetServicesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ServiceRegistration>> GetAllServicesAsync(CancellationToken cancellationToken = default);

    Task<ServiceRegistration> UpdateServiceAsync(ServiceRegistration service);

    Task UnregisterServiceAsync(Guid serviceId);

    Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync();

    Task<ServiceRegistration> DisableServiceAsync(Guid serviceId, string reason, CancellationToken cancellationToken = default);

    Task<ServiceRegistration> EnableServiceAsync(Guid serviceId);

    Task<decimal> GetServiceSuccessRateAsync(Guid serviceId, int minutesBack = 60);
}
