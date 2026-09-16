#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// API endpoints for health check management and service monitoring.
/// </summary>
[ApiController]
[Route(HealthCheckControllerConstants.RouteTemplate)]
[Produces(HealthCheckControllerConstants.ResponseContentType)]
public class HealthCheckController : ControllerBase, IHealthCheckController
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly IServiceManagementService? _serviceManagementService;
    private readonly ILogger<HealthCheckController> _logger;

    public HealthCheckController(
        IHealthCheckService healthCheckService,
        ILogger<HealthCheckController> logger,
        IServiceManagementService? serviceManagementService = null)
    {
        ArgumentNullException.ThrowIfNull(healthCheckService);
        ArgumentNullException.ThrowIfNull(logger);
        _healthCheckService = healthCheckService;
        _logger = logger;
        _serviceManagementService = serviceManagementService;
    }

    /// <summary>
    /// Gets an aggregated health summary for all registered services.
    /// </summary>
    [HttpGet(HealthCheckControllerConstants.SummaryRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHealthSummary(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation(HealthCheckControllerConstants.LogGetHealthSummaryCalled);

        try
        {
            var serviceManagementService = _serviceManagementService
                ?? HttpContext.RequestServices.GetRequiredService<IServiceManagementService>();
            var services = (await serviceManagementService.GetAllServicesAsync(cancellationToken)).ToList();
            var latestChecks = await Task.WhenAll(services.Select(async service =>
                (await _healthCheckService.GetServiceHealthHistoryAsync(service.Id, HealthCheckControllerConstants.LatestHealthCheckCount)
                    .WaitAsync(cancellationToken))
                .OrderByDescending(result => result.CheckedAt)
                .FirstOrDefault()));

            var statuses = latestChecks
                .Select(check => check?.Status ?? HealthStatus.Unknown)
                .ToList();
            var statusCounts = Enum.GetValues<HealthStatus>()
                .ToDictionary(status => status.ToString(), status => statuses.Count(value => value == status));
            var overallStatus = statuses
                .OrderByDescending(GetHealthStatusSeverity)
                .FirstOrDefault();

            _logger.LogInformation(HealthCheckControllerConstants.LogGetHealthSummaryCompleted, services.Count);
            return Ok(new
            {
                success = true,
                data = new
                {
                    statusCounts,
                    overallStatus,
                    latestCheckAt = latestChecks
                        .Where(check => check is not null)
                        .Select(check => (DateTime?)check!.CheckedAt)
                        .Max()
                }
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, HealthCheckControllerConstants.LogGetHealthSummaryFailed);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                data = new { error = HealthCheckControllerConstants.ErrorFailedRetrieveHealthSummary }
            });
        }
    }

    private static int GetHealthStatusSeverity(HealthStatus status) => status switch
    {
        HealthStatus.Error => HealthCheckControllerConstants.ErrorSeverity,
        HealthStatus.Timeout => HealthCheckControllerConstants.TimeoutSeverity,
        HealthStatus.Unhealthy => HealthCheckControllerConstants.UnhealthySeverity,
        HealthStatus.Degraded => HealthCheckControllerConstants.DegradedSeverity,
        HealthStatus.Unknown => HealthCheckControllerConstants.UnknownSeverity,
        HealthStatus.Healthy => HealthCheckControllerConstants.HealthySeverity,
        _ => HealthCheckControllerConstants.UnknownSeverity
    };

    /// <summary>
    /// Performs an immediate health check on a service.
    /// </summary>
    [HttpPost(HealthCheckControllerConstants.CheckRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckServiceHealth(Guid serviceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation(HealthCheckControllerConstants.LogCheckServiceHealthCalled, serviceId);

        try
        {
            var result = await _healthCheckService.PerformHealthCheckAsync(serviceId).WaitAsync(cancellationToken);
            _logger.LogInformation(HealthCheckControllerConstants.LogCheckServiceHealthCompleted, serviceId);
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
            _logger.LogWarning(HealthCheckControllerConstants.LogServiceNotFound, serviceId);
            _logger.LogError(ex, HealthCheckControllerConstants.LogCheckServiceHealthFailed, serviceId);
            return NotFound(new { error = ex.Message });
        }
        catch (ServiceScaffoldException ex)
        {
            _logger.LogError(ex, HealthCheckControllerConstants.LogHealthCheckError, serviceId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves recent health check history for a service.
    /// </summary>
    [HttpGet(HealthCheckControllerConstants.HistoryRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHealthHistory(Guid serviceId, [FromQuery] int count = HealthCheckControllerConstants.DefaultHistoryCount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation(HealthCheckControllerConstants.LogGetHealthHistoryCalled, serviceId, count);

        try
        {
            var history = await _healthCheckService.GetServiceHealthHistoryAsync(serviceId, count).WaitAsync(cancellationToken);
            _logger.LogInformation(HealthCheckControllerConstants.LogGetHealthHistoryCompleted, serviceId, count);
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
            _logger.LogWarning(HealthCheckControllerConstants.LogHealthHistoryServiceNotFound, serviceId);
            _logger.LogError(ex, HealthCheckControllerConstants.LogGetHealthHistoryFailed, serviceId, count);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the health status summary for a service.
    /// </summary>
    [HttpGet(HealthCheckControllerConstants.StatusRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHealthStatus(Guid serviceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation(HealthCheckControllerConstants.LogGetHealthStatusCalled, serviceId);

        try
        {
            var status = await _healthCheckService.GetServiceHealthStatusAsync(serviceId).WaitAsync(cancellationToken);
            var successRate = await _healthCheckService.GetServiceSuccessRateAsync(serviceId).WaitAsync(cancellationToken);

            _logger.LogInformation(HealthCheckControllerConstants.LogGetHealthStatusCompleted, serviceId);
            return Ok(new
            {
                success = true,
                data = new
                {
                    status,
                    successRate = successRate.ToString(HealthCheckControllerConstants.SuccessRateFormat) + HealthCheckControllerConstants.PercentSymbol,
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (ServiceNotFoundException ex)
        {
            _logger.LogWarning(HealthCheckControllerConstants.LogHealthStatusServiceNotFound, serviceId);
            _logger.LogError(ex, HealthCheckControllerConstants.LogGetHealthStatusFailed, serviceId);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves failed health check results for a service.
    /// </summary>
    [HttpGet(HealthCheckControllerConstants.FailuresRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFailedChecks(Guid serviceId, [FromQuery] int hoursBack = HealthCheckControllerConstants.DefaultHoursBack, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation(HealthCheckControllerConstants.LogGetFailedChecksCalled, serviceId, hoursBack);

        try
        {
            var failures = await _healthCheckService.GetFailedChecksAsync(serviceId, hoursBack).WaitAsync(cancellationToken);
            _logger.LogInformation(HealthCheckControllerConstants.LogGetFailedChecksCompleted, serviceId, hoursBack);
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
            _logger.LogWarning(HealthCheckControllerConstants.LogFailedChecksServiceNotFound, serviceId);
            _logger.LogError(ex, HealthCheckControllerConstants.LogGetFailedChecksFailed, serviceId, hoursBack);
            return NotFound(new { error = ex.Message });
        }
    }
}
