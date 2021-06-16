#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service for managing service registrations and lifecycle operations.
/// </summary>
public class ServiceManagementService : IServiceManagementService
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<ServiceManagementService> _logger;

    public ServiceManagementService(
        IServiceRepository serviceRepository,
        IUserRepository userRepository,
        IAuditService auditService,
        ILogger<ServiceManagementService> logger)
    {
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new service with validation, uniqueness checks, and owner verification.
    /// Creates an audit trail entry on success.
    /// </summary>
    /// <param name="serviceName">Unique name for the service.</param>
    /// <param name="endpoint">Absolute URL of the service endpoint.</param>
    /// <param name="healthCheckUrl">Absolute URL for health check polling.</param>
    /// <param name="ownerId">ID of the user who owns this service.</param>
    /// <returns>The newly created <see cref="ServiceRegistration"/>.</returns>
    /// <exception cref="ServiceValidationException">Thrown when input validation fails or name is already taken.</exception>
    /// <exception cref="ServiceScaffoldException">Thrown when the owner user is not found.</exception>
    public async Task<ServiceRegistration> RegisterServiceAsync(
        string serviceName,
        string endpoint,
        string healthCheckUrl,
        Guid ownerId)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(serviceName))
            errors.Add("Service name is required");

        if (string.IsNullOrWhiteSpace(endpoint))
            errors.Add("Service endpoint is required");

        if (string.IsNullOrWhiteSpace(healthCheckUrl))
            errors.Add("Health check URL is required");

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out _))
            errors.Add("Invalid service endpoint URL");

        if (!Uri.TryCreate(healthCheckUrl, UriKind.Absolute, out _))
            errors.Add("Invalid health check URL");

        if (errors.Count > 0)
            throw new ServiceValidationException(errors);

        var owner = await _userRepository.GetByIdAsync(ownerId);
        if (owner is null)
            throw new ServiceScaffoldException("Service owner not found", "OWNER_NOT_FOUND");

        var existingService = await _serviceRepository.GetByNameAsync(serviceName);
        if (existingService is not null)
            throw new ServiceValidationException("Service name already registered");

        var service = new ServiceRegistration
        {
            Id = Guid.NewGuid(),
            ServiceName = serviceName,
            Endpoint = endpoint,
            HealthCheckUrl = healthCheckUrl,
            OwnerId = ownerId,
            Version = "1.0.0",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!service.IsValid())
            throw new ServiceValidationException("Service configuration is invalid");

        var registered = await _serviceRepository.AddAsync(service);

        await _auditService.LogActionAsync(ownerId, "Create", "ServiceRegistration", service.Id,
            $"Registered service {serviceName}");

        _logger.LogInformation("Service registered: {ServiceName} by user {UserId}", serviceName, ownerId);
        return registered;
    }

    public async Task<ServiceRegistration?> GetServiceAsync(Guid serviceId)
    {
        return await _serviceRepository.GetByIdAsync(serviceId);
    }

    public async Task<ServiceRegistration?> GetServiceByNameAsync(string serviceName)
    {
        return await _serviceRepository.GetByNameAsync(serviceName);
    }

    public async Task<IEnumerable<ServiceRegistration>> GetServicesByOwnerAsync(Guid ownerId)
    {
        return await _serviceRepository.GetByOwnerAsync(ownerId);
    }

    public async Task<IEnumerable<ServiceRegistration>> GetAllServicesAsync()
    {
        return await _serviceRepository.GetAllAsync();
    }

    public async Task<ServiceRegistration> UpdateServiceAsync(ServiceRegistration service)
    {
        if (!service.IsValid())
            throw new ServiceValidationException("Service configuration is invalid");

        service.UpdatedAt = DateTime.UtcNow;
        var updated = await _serviceRepository.UpdateAsync(service);

        _logger.LogInformation("Service updated: {ServiceId}", service.Id);
        return updated;
    }

    /// <summary>
    /// Permanently removes a service registration and logs the action to the audit trail.
    /// </summary>
    /// <exception cref="ServiceNotFoundException">Thrown when the service ID does not exist.</exception>
    public async Task UnregisterServiceAsync(Guid serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        await _serviceRepository.DeleteAsync(serviceId);

        await _auditService.LogActionAsync(null, "Delete", "ServiceRegistration", serviceId,
            $"Unregistered service {service.ServiceName}");

        _logger.LogInformation("Service unregistered: {ServiceId}", serviceId);
    }

    public async Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync()
    {
        return await _serviceRepository.GetUnhealthyServicesAsync();
    }

    /// <summary>
    /// Disables a service with a specified reason. Disabled services are excluded from routing
    /// but their registration is preserved for re-enabling.
    /// </summary>
    public async Task<ServiceRegistration> DisableServiceAsync(Guid serviceId, string reason)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        service.Disable(reason);
        await _serviceRepository.UpdateAsync(service);

        await _auditService.LogActionAsync(null, "Update", "ServiceRegistration", serviceId,
            $"Disabled service: {reason}");

        _logger.LogInformation("Service disabled: {ServiceId} - {Reason}", serviceId, reason);
        return service;
    }

    public async Task<ServiceRegistration> EnableServiceAsync(Guid serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        service.Enable();
        await _serviceRepository.UpdateAsync(service);

        await _auditService.LogActionAsync(null, "Update", "ServiceRegistration", serviceId,
            "Re-enabled service");

        _logger.LogInformation("Service enabled: {ServiceId}", serviceId);
        return service;
    }

    public async Task<decimal> GetServiceSuccessRateAsync(Guid serviceId, int minutesBack = 60)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        if (service.TotalRequests == 0)
            return 100m;

        return service.GetSuccessRate();
    }
}
