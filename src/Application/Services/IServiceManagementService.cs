#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Defines operations for registering, querying, and managing services.
/// </summary>
public interface IServiceManagementService
{
    /// <summary>
    /// Registers a service with the specified connection and ownership details.
    /// </summary>
    /// <param name="serviceName">The name of the service to register.</param>
    /// <param name="endpoint">The endpoint URL of the service.</param>
    /// <param name="healthCheckUrl">The health check URL of the service.</param>
    /// <param name="ownerId">The ID of the user who owns the service.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the registered service.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serviceName"/>, <paramref name="endpoint"/>, or <paramref name="healthCheckUrl"/> is null or empty.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the service details are invalid or the service name is already registered.</exception>
    /// <exception cref="ServiceScaffoldException">Thrown when the specified owner cannot be found.</exception>
    Task<ServiceRegistration> RegisterServiceAsync(string serviceName, string endpoint, string healthCheckUrl, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a service by its unique identifier.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the service, or <see langword="null"/> if no matching service exists.</returns>
    Task<ServiceRegistration?> GetServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a service by its name.
    /// </summary>
    /// <param name="serviceName">The name of the service to retrieve.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the service, or <see langword="null"/> if no matching service exists.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serviceName"/> is null or empty.</exception>
    Task<ServiceRegistration?> GetServiceByNameAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all services owned by a specific user.
    /// </summary>
    /// <param name="ownerId">The ID of the user who owns the services.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the collection of services owned by the specified user.</returns>
    Task<IEnumerable<ServiceRegistration>> GetServicesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all services.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the collection of all registered services.</returns>
    Task<IEnumerable<ServiceRegistration>> GetAllServicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing service.
    /// </summary>
    /// <param name="service">The service to update.</param>
    /// <returns>A task whose result is the updated service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the service is invalid.</exception>
    Task<ServiceRegistration> UpdateServiceAsync(ServiceRegistration service);

    /// <summary>
    /// Unregisters a service by marking it as disabled and removing it from the repository.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to unregister.</param>
    /// <exception cref="ServiceNotFoundException">Thrown when the service is not found.</exception>
    Task UnregisterServiceAsync(Guid serviceId);

    /// <summary>
    /// Retrieves all unhealthy services.
    /// </summary>
    /// <returns>A task whose result is the collection of unhealthy services.</returns>
    Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync();

    /// <summary>
    /// Disables a service with the specified reason.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to disable.</param>
    /// <param name="reason">The reason for disabling the service.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task whose result is the disabled service.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="reason"/> is null or empty.</exception>
    /// <exception cref="ServiceNotFoundException">Thrown when the service is not found.</exception>
    Task<ServiceRegistration> DisableServiceAsync(Guid serviceId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables a previously disabled service.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to enable.</param>
    /// <returns>A task whose result is the enabled service.</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service is not found.</exception>
    Task<ServiceRegistration> EnableServiceAsync(Guid serviceId);

    /// <summary>
    /// Gets the success rate of a service over a specified time period.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <param name="minutesBack">The number of minutes to look back for calculating the success rate. Defaults to 60 minutes.</param>
    /// <returns>A task whose result is the success rate as a percentage from 0 through 100.</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service is not found.</exception>
    Task<decimal> GetServiceSuccessRateAsync(Guid serviceId, int minutesBack = 60);
}
