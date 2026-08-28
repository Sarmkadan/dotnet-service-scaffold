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
    Task<IActionResult> RegisterService(RegisterServiceRequest request, CancellationToken cancellationToken = default);
    Task<IActionResult> GetService(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IActionResult> ListServices(CancellationToken cancellationToken = default);
    Task<IActionResult> GetServicesByOwner(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IActionResult> DisableService(Guid serviceId, DisableServiceRequest request, CancellationToken cancellationToken = default);
    Task<IActionResult> EnableService(Guid serviceId);
    Task<IActionResult> DeregisterService(Guid serviceId);
    Task<IActionResult> GetUnhealthyServices();
}