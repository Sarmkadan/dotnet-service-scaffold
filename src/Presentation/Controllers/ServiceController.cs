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
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServiceController : ControllerBase
{
    private readonly IServiceManagementService _serviceManagementService;
    private readonly ILogger<ServiceController> _logger;

    public ServiceController(
        IServiceManagementService serviceManagementService,
        ILogger<ServiceController> logger)
    {
        _serviceManagementService = serviceManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new service for monitoring.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterService([FromBody] RegisterServiceRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var service = await _serviceManagementService.RegisterServiceAsync(
                request.ServiceName,
                request.Endpoint,
                request.HealthCheckUrl,
                request.OwnerId,
                cancellationToken);

            return CreatedAtAction(nameof(GetService), new { serviceId = service.Id }, new
            {
                success = true,
                data = new
                {
                    service.Id,
                    service.ServiceName,
                    service.Endpoint,
                    service.Version,
                    service.CreatedAt
                }
            });
        }
        catch (ServiceValidationException ex)
        {
            _logger.LogWarning("Service registration validation error: {Errors}", string.Join(", ", ex.Errors));
            return BadRequest(new { error = "Validation failed", details = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service registration error");
            return StatusCode(500, new { error = "Registration failed" });
        }
    }

    /// <summary>
    /// Retrieves service information by ID.
    /// </summary>
    [HttpGet("{serviceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetService(Guid serviceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var service = await _serviceManagementService.GetServiceAsync(serviceId, cancellationToken);

            if (service is null)
            {
                _logger.LogWarning("Service not found: {ServiceId}", serviceId);
                return NotFound(new { error = "Service not found" });
            }

            var successRate = await _serviceManagementService.GetServiceSuccessRateAsync(serviceId);

            return Ok(new
            {
                success = true,
                data = new
                {
                    service.Id,
                    service.ServiceName,
                    service.Endpoint,
                    service.Status,
                    service.Version,
                    service.IsEnabled,
                    successRate = $"{successRate:F2}%",
                    service.LastHealthCheckAt,
                    service.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Error retrieving service" });
        }
    }

    /// <summary>
    /// Lists all registered services.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListServices(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var services = await _serviceManagementService.GetAllServicesAsync(cancellationToken);

            return Ok(new
            {
                success = true,
                count = services.Count(),
                data = services.Select(s => new
                {
                    s.Id,
                    s.ServiceName,
                    s.Status,
                    s.IsEnabled,
                    s.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing services");
            return StatusCode(500, new { error = "Error listing services" });
        }
    }

    /// <summary>
    /// Gets all services owned by a specific user.
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServicesByOwner(Guid ownerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var services = await _serviceManagementService.GetServicesByOwnerAsync(ownerId, cancellationToken);

            return Ok(new
            {
                success = true,
                count = services.Count(),
                data = services.Select(s => new
                {
                    s.Id,
                    s.ServiceName,
                    s.Status,
                    s.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving services for owner {OwnerId}", ownerId);
            return StatusCode(500, new { error = "Error retrieving services" });
        }
    }

    /// <summary>
    /// Disables a service from monitoring.
    /// </summary>
    [HttpPost("{serviceId}/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableService(Guid serviceId, [FromBody] DisableServiceRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var service = await _serviceManagementService.DisableServiceAsync(serviceId, request.Reason, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    service.Id,
                    service.ServiceName,
                    service.Status
                }
            });
        }
        catch (ServiceNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Error disabling service" });
        }
    }

    /// <summary>
    /// Enables a previously disabled service.
    /// </summary>
    [HttpPost("{serviceId}/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableService(Guid serviceId)
    {
        try
        {
            var service = await _serviceManagementService.EnableServiceAsync(serviceId);

            return Ok(new
            {
                success = true,
                data = new
                {
                    service.Id,
                    service.ServiceName,
                    service.Status
                }
            });
        }
        catch (ServiceNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Error enabling service" });
        }
    }

    /// <summary>
    /// Deregisters a service by removing it from monitoring.
    /// </summary>
    [HttpDelete("{serviceId}/registration")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeregisterService(Guid serviceId)
    {
        try
        {
            await _serviceManagementService.UnregisterServiceAsync(serviceId);

            return NoContent();
        }
        catch (ServiceNotFoundException ex)
        {
            _logger.LogWarning("Service not found for deregistration: {ServiceId}", serviceId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deregistering service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Error deregistering service" });
        }
    }

    /// <summary>
    /// Gets unhealthy services that need attention.
    /// </summary>
    [HttpGet("health/unhealthy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnhealthyServices()
    {
        try
        {
            var services = await _serviceManagementService.GetUnhealthyServicesAsync();

            return Ok(new
            {
                success = true,
                count = services.Count(),
                data = services.Select(s => new
                {
                    s.Id,
                    s.ServiceName,
                    s.Status,
                    s.ConsecutiveFailures,
                    s.LastHealthCheckAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unhealthy services");
            return StatusCode(500, new { error = "Error retrieving unhealthy services" });
        }
    }
}

public record RegisterServiceRequest(string ServiceName, string Endpoint, string HealthCheckUrl, Guid OwnerId);
public record DisableServiceRequest(string Reason);
