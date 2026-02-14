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
    Task<ServiceRegistration> RegisterServiceAsync(string serviceName, string endpoint, string healthCheckUrl, Guid ownerId);

    Task<ServiceRegistration?> GetServiceAsync(Guid serviceId);

    Task<ServiceRegistration?> GetServiceByNameAsync(string serviceName);

    Task<IEnumerable<ServiceRegistration>> GetServicesByOwnerAsync(Guid ownerId);

    Task<IEnumerable<ServiceRegistration>> GetAllServicesAsync();

    Task<ServiceRegistration> UpdateServiceAsync(ServiceRegistration service);

    Task UnregisterServiceAsync(Guid serviceId);

    Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync();

    Task<ServiceRegistration> DisableServiceAsync(Guid serviceId, string reason);

    Task<ServiceRegistration> EnableServiceAsync(Guid serviceId);

    Task<decimal> GetServiceSuccessRateAsync(Guid serviceId, int minutesBack = 60);
}
