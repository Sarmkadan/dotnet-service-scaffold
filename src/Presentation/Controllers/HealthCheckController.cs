#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// API endpoints for health check management and service monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthCheckController : ControllerBase, IHealthCheckController
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<HealthCheckController> _logger;

    public HealthCheckController(IHealthCheckService healthCheckService, ILogger<HealthCheckController> logger)
    {
        ArgumentNullException.ThrowIfNull(healthCheckService);
        ArgumentNullException.ThrowIfNull(logger);
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Performs an immediate health check on a service.
    /// </summary>
    [HttpPost("{serviceId}/check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckServiceHealth(Guid serviceId)
    {
        try
        {
            var result = await _healthCheckService.PerformHealthCheckAsync(serviceId);
            return Ok(new
            {
                success = true,
                data = new
                {
                    result.Id,
                    result.Status,
                    result.HttpStatusCode,
                    result.ResponseTimeMs,
                    result.CheckedAt,
                    result.ErrorMessage
                }
            });
        }
        catch (ServiceNotFoundException ex)
        {
            _logger.LogWarning("Service not found: {ServiceId}", serviceId);
            return NotFound(new { error = ex.Message });
        }
        catch (ServiceScaffoldException ex)
        {
            _logger.LogError(ex, "Health check error for service {ServiceId}", serviceId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves recent health check history for a service.
    /// </summary>
    [HttpGet("{serviceId}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHealthHistory(Guid serviceId, [FromQuery] int count = 20)
    {
        try
        {
            var history = await _healthCheckService.GetServiceHealthHistoryAsync(serviceId, count);
            return Ok(new
            {
                success = true,
                count = history.Count(),
                data = history.Select(h => new
                {
                    h.Id,
                    h.Status,
                    h.HttpStatusCode,
                    h.ResponseTimeMs,
                    h.CheckedAt,
                    h.ErrorMessage
                })
            });
        }
        catch (ServiceNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the health status summary for a service.
    /// </summary>
    [HttpGet("{serviceId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHealthStatus(Guid serviceId)
    {
        try
        {
            var status = await _healthCheckService.GetServiceHealthStatusAsync(serviceId);
            var successRate = await _healthCheckService.GetServiceSuccessRateAsync(serviceId);

            return Ok(new
            {
                success = true,
                data = new
                {
                    status,
                    successRate = $"{successRate:F2}%",
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (ServiceNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves failed health check results for a service.
    /// </summary>
    [HttpGet("{serviceId}/failures")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFailedChecks(Guid serviceId, [FromQuery] int hoursBack = 24)
    {
        try
        {
            var failures = await _healthCheckService.GetFailedChecksAsync(serviceId, hoursBack);
            return Ok(new
            {
                success = true,
                count = failures.Count(),
                data = failures.Select(f => new
                {
                    f.Id,
                    f.Status,
                    f.ResponseTimeMs,
                    f.CheckedAt,
                    f.ErrorMessage
                })
            });
        }
        catch (ServiceNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
