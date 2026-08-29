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
/// API endpoints for health check management and service monitoring.
/// </summary>
public interface IHealthCheckController
{
    Task<IActionResult> CheckServiceHealth(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IActionResult> GetHealthHistory(Guid serviceId, int count = 20, CancellationToken cancellationToken = default);
    Task<IActionResult> GetHealthStatus(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IActionResult> GetFailedChecks(Guid serviceId, int hoursBack = 24, CancellationToken cancellationToken = default);
}