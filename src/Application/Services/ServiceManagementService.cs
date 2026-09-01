#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Enums;
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
    /// Registers a new service with the specified details.
    /// </summary>
    /// <param name="serviceName">The name of the service to register.</param>
    /// <param name="endpoint">The endpoint URL of the service.</param>
    /// <param name="healthCheckUrl">The health check URL of the service.</param>
    /// <param name="ownerId">The ID of the user who owns the service.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The registered service.</returns>
    /// <exception cref="ArgumentException">Thrown when serviceName, endpoint, or healthCheckUrl is null or empty.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the service validation fails.</exception>
    /// <exception cref="ServiceScaffoldException">Thrown when the service owner is not found.</exception>
    public async Task<ServiceRegistration> RegisterServiceAsync(
        string serviceName,
        string endpoint,
        string healthCheckUrl,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentException.ThrowIfNullOrEmpty(healthCheckUrl);
        cancellationToken.ThrowIfCancellationRequested();
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(serviceName))
            errors.Add(ServiceManagementServiceConstants.ServiceNameRequired);

        if (string.IsNullOrWhiteSpace(endpoint))
            errors.Add(ServiceManagementServiceConstants.ServiceEndpointRequired);

        if (string.IsNullOrWhiteSpace(healthCheckUrl))
            errors.Add(ServiceManagementServiceConstants.HealthCheckUrlRequired);

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out _))
            errors.Add(ServiceManagementServiceConstants.InvalidServiceEndpointUrl);

        if (!Uri.TryCreate(healthCheckUrl, UriKind.Absolute, out _))
            errors.Add(ServiceManagementServiceConstants.InvalidHealthCheckUrl);

        if (errors.Count > 0)
            throw new ServiceValidationException(errors);

        var owner = await _userRepository.GetByIdAsync(ownerId, cancellationToken);
        if (owner is null)
            throw new ServiceScaffoldException(ServiceManagementServiceConstants.ServiceOwnerNotFound, ServiceManagementServiceConstants.OwnerNotFoundErrorCode);

        var existingService = await _serviceRepository.GetByNameAsync(serviceName, cancellationToken);
        if (existingService is not null)
            throw new ServiceValidationException(ServiceManagementServiceConstants.ServiceNameAlreadyRegistered);

        var service = new ServiceRegistration
        {
            Id = Guid.NewGuid(),
            ServiceName = serviceName,
            Endpoint = endpoint,
            HealthCheckUrl = healthCheckUrl,
            OwnerId = ownerId,
            Version = ServiceManagementServiceConstants.DefaultVersion,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!service.IsValid())
            throw new ServiceValidationException(ServiceManagementServiceConstants.ServiceConfigurationInvalid);

        var registered = await _serviceRepository.AddAsync(service, cancellationToken);

        await _auditService.LogActionAsync(ownerId, ServiceManagementServiceConstants.AuditActionCreate, ServiceManagementServiceConstants.AuditEntityTypeServiceRegistration, service.Id,
            $"Registered service {serviceName}");

        _logger.LogInformation("Service registered: {ServiceName} by user {UserId}", serviceName, ownerId);
        return registered;
    }

    /// <summary>
    /// Retrieves a service by its unique identifier.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The service if found; otherwise, null.</returns>
    public async Task<ServiceRegistration?> GetServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
    }

    /// <summary>
    /// Retrieves a service by its name.
    /// </summary>
    /// <param name="serviceName">The name of the service to retrieve.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The service if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when serviceName is null or empty.</exception>
    public async Task<ServiceRegistration?> GetServiceByNameAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        return await _serviceRepository.GetByNameAsync(serviceName, cancellationToken);
    }

    /// <summary>
    /// Retrieves a service by its name.
    /// </summary>
    /// <param name="serviceName">The name of the service to retrieve.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The service if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when serviceName is null or empty.</exception>
    Task<ServiceRegistration?> IServiceManagementService.GetServiceByNameAsync(string serviceName, CancellationToken cancellationToken) =>
        GetServiceByNameAsync(serviceName, cancellationToken);

    /// <summary>
    /// Retrieves all services owned by a specific user.
    /// </summary>
    /// <param name="ownerId">The ID of the user who owns the services.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A collection of services owned by the user.</returns>
    public async Task<IEnumerable<ServiceRegistration>> GetServicesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _serviceRepository.GetByOwnerAsync(ownerId, cancellationToken);
    }

    /// <summary>
    /// Retrieves all services.
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A collection of all services.</returns>
    public async Task<IEnumerable<ServiceRegistration>> GetAllServicesAsync(CancellationToken cancellationToken = default)
    {
        return await _serviceRepository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing service.
    /// </summary>
    /// <param name="service">The service to update.</param>
    /// <returns>The updated service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null.</exception>
    /// <exception cref="ServiceValidationException">Thrown when the service validation fails.</exception>
    public async Task<ServiceRegistration> UpdateServiceAsync(ServiceRegistration service)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (!service.IsValid())
            throw new ServiceValidationException(ServiceManagementServiceConstants.ServiceConfigurationInvalid);

        service.UpdatedAt = DateTime.UtcNow;
        var updated = await _serviceRepository.UpdateAsync(service);

        _logger.LogInformation("Service updated: {ServiceId}", service.Id);
        return updated;
    }

    /// <summary>
    /// Unregisters a service by marking it as disabled and removing it from the repository.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to unregister.</param>
    /// <exception cref="ServiceNotFoundException">Thrown when the service is not found.</exception>
    public async Task UnregisterServiceAsync(Guid serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        service.Status = ServiceStatus.Disabled;
        service.Events.Add(new ServiceEvent
        {
            ServiceId = service.Id,
            EventType = ServiceEventType.ServiceDown,
            Message = "Service unregistered",
            Severity = "Info",
            CreatedAt = DateTime.UtcNow
        });

        await _serviceRepository.UpdateAsync(service);
        await _serviceRepository.DeleteAsync(serviceId);

        await _auditService.LogActionAsync(null, ServiceManagementServiceConstants.AuditActionDelete, ServiceManagementServiceConstants.AuditEntityTypeServiceRegistration, serviceId,
            $"Unregistered service {service.ServiceName}");

        _logger.LogInformation("Service unregistered: {ServiceId}", serviceId);
    }

    /// <summary>
    /// Retrieves all unhealthy services.
    /// </summary>
    /// <returns>A collection of unhealthy services.</returns>
    public async Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync()
    {
        return await _serviceRepository.GetUnhealthyServicesAsync();
    }

    /// <summary>
    /// Disables a service with the specified reason.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to disable.</param>
    /// <param name="reason">The reason for disabling the service.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The disabled service.</returns>
    /// <exception cref="ArgumentException">Thrown when reason is null or empty.</exception>
    /// <exception cref="ServiceNotFoundException">Thrown when the service is not found.</exception>
    public async Task<ServiceRegistration> DisableServiceAsync(Guid serviceId, string reason, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(reason);
        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        service.Disable(reason);
        await _serviceRepository.UpdateAsync(service, cancellationToken);

        await _auditService.LogActionAsync(null, ServiceManagementServiceConstants.AuditActionUpdate, ServiceManagementServiceConstants.AuditEntityTypeServiceRegistration, serviceId,
            $"Disabled service: {reason}");

        _logger.LogInformation("Service disabled: {ServiceId} - {Reason}", serviceId, reason);
        return service;
    }

    /// <summary>
    /// Enables a previously disabled service.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service to enable.</param>
    /// <returns>The enabled service.</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service is not found.</exception>
    public async Task<ServiceRegistration> EnableServiceAsync(Guid serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        service.Enable();
        await _serviceRepository.UpdateAsync(service);

        await _auditService.LogActionAsync(null, ServiceManagementServiceConstants.AuditActionUpdate, ServiceManagementServiceConstants.AuditEntityTypeServiceRegistration, serviceId,
            ServiceManagementServiceConstants.ReEnabledServiceAuditMessage);

        _logger.LogInformation("Service enabled: {ServiceId}", serviceId);
        return service;
    }

    /// <summary>
    /// Gets the success rate of a service over a specified time period.
    /// </summary>
    /// <param name="serviceId">The unique identifier of the service.</param>
    /// <param name="minutesBack">The number of minutes to look back for calculating the success rate. Defaults to 60 minutes.</param>
    /// <returns>The success rate as a percentage (0-100).</returns>
    /// <exception cref="ServiceNotFoundException">Thrown when the service is not found.</exception>
    public async Task<decimal> GetServiceSuccessRateAsync(Guid serviceId, int minutesBack = ServiceManagementServiceConstants.DefaultMinutesBack)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service is null)
            throw new ServiceNotFoundException(serviceId);

        if (service.TotalRequests == 0)
            return 100m;

        return service.GetSuccessRate();
    }
}
