#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// API endpoints for service registration and management.
/// </summary>
public interface IServiceController
{
    /// <summary>
    /// Registers a new service with the provided request.
    /// </summary>
    /// <param name="request">The service registration details.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>An IActionResult indicating the result of the registration.</returns>
    Task<IActionResult> RegisterService(RegisterServiceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a service by its unique identifier.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>An IActionResult containing the service details or not found.</returns>
    Task<IActionResult> GetService(Guid serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of all registered services.
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>An IActionResult containing the collection of services.</returns>
    Task<IActionResult> ListServices(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all services owned by a specific owner.
    /// </summary>
    /// <param name="ownerId">The unique identifier of the owner.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>An IActionResult containing the services for the owner.</returns>
    Task<IActionResult> GetServicesByOwner(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables a service with the given ID using the provided request.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to disable.</param>
    /// <param name="request">The disable service details.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>An IActionResult indicating the result.</returns>
    Task<IActionResult> DisableService(Guid serviceId, DisableServiceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables a previously disabled service.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to enable.</param>
    /// <returns>An IActionResult indicating the result.</returns>
    Task<IActionResult> EnableService(Guid serviceId);

    /// <summary>
    /// Removes a service from the system.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to deregister.</param>
    /// <returns>An IActionResult indicating the result.</returns>
    Task<IActionResult> DeregisterService(Guid serviceId);

    /// <summary>
    /// Retrieves a list of services that are currently unhealthy.
    /// </summary>
    /// <returns>An IActionResult containing the unhealthy services.</returns>
    Task<IActionResult> GetUnhealthyServices();
}