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

    public async Task<ServiceRegistration?> GetServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
    }

    public async Task<ServiceRegistration?> GetServiceByNameAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        return await _serviceRepository.GetByNameAsync(serviceName, cancellationToken);
    }

    Task<ServiceRegistration?> IServiceManagementService.GetServiceByNameAsync(string serviceName, CancellationToken cancellationToken) =>
        GetServiceByNameAsync(serviceName, cancellationToken);

    public async Task<IEnumerable<ServiceRegistration>> GetServicesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _serviceRepository.GetByOwnerAsync(ownerId, cancellationToken);
    }

    public async Task<IEnumerable<ServiceRegistration>> GetAllServicesAsync(CancellationToken cancellationToken = default)
    {
        return await _serviceRepository.GetAllAsync(cancellationToken);
    }

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

    public async Task<IEnumerable<ServiceRegistration>> GetUnhealthyServicesAsync()
    {
        return await _serviceRepository.GetUnhealthyServicesAsync();
    }

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
