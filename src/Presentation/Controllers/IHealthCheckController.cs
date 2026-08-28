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
    Task<IActionResult> CheckServiceHealth(Guid serviceId);
    Task<IActionResult> GetHealthHistory(Guid serviceId, int count = 20);
    Task<IActionResult> GetHealthStatus(Guid serviceId);
    Task<IActionResult> GetFailedChecks(Guid serviceId, int hoursBack = 24);
}