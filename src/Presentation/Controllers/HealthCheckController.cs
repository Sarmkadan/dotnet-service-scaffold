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
        _logger.LogInformation("CheckServiceHealth called with {ServiceId}", serviceId);

        try
        {
            var result = await _healthCheckService.PerformHealthCheckAsync(serviceId);
            _logger.LogInformation("CheckServiceHealth completed for {ServiceId}", serviceId);
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
            _logger.LogError(ex, "Failed to check health for service {ServiceId}", serviceId);
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
        _logger.LogInformation("GetHealthHistory called with {ServiceId} and {Count}", serviceId, count);

        try
        {
            var history = await _healthCheckService.GetServiceHealthHistoryAsync(serviceId, count);
            _logger.LogInformation("GetHealthHistory completed for {ServiceId} with requested count {Count}", serviceId, count);
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
            _logger.LogWarning("Health history unavailable because service {ServiceId} was not found", serviceId);
            _logger.LogError(ex, "Failed to get health history for service {ServiceId} with requested count {Count}", serviceId, count);
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
        _logger.LogInformation("GetHealthStatus called with {ServiceId}", serviceId);

        try
        {
            var status = await _healthCheckService.GetServiceHealthStatusAsync(serviceId);
            var successRate = await _healthCheckService.GetServiceSuccessRateAsync(serviceId);

            _logger.LogInformation("GetHealthStatus completed for {ServiceId}", serviceId);
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
            _logger.LogWarning("Health status unavailable because service {ServiceId} was not found", serviceId);
            _logger.LogError(ex, "Failed to get health status for service {ServiceId}", serviceId);
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
        _logger.LogInformation("GetFailedChecks called with {ServiceId} and {HoursBack}", serviceId, hoursBack);

        try
        {
            var failures = await _healthCheckService.GetFailedChecksAsync(serviceId, hoursBack);
            _logger.LogInformation("GetFailedChecks completed for {ServiceId} with {HoursBack} hours back", serviceId, hoursBack);
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
            _logger.LogWarning("Failed checks unavailable because service {ServiceId} was not found", serviceId);
            _logger.LogError(ex, "Failed to get failed checks for service {ServiceId} with {HoursBack} hours back", serviceId, hoursBack);
            return NotFound(new { error = ex.Message });
        }
    }
}
