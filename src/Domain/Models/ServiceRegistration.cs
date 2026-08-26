#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetServiceScaffold.Domain.Enums;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Represents a registered service that is monitored and managed by the scaffold system.
/// </summary>
public class ServiceRegistration
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(ServiceRegistrationConstants.ServiceNameMaxLength)]
    public required string ServiceName { get; set; }

    [StringLength(ServiceRegistrationConstants.DescriptionMaxLength)]
    public string? Description { get; set; }

    [Required]
    [StringLength(ServiceRegistrationConstants.HealthCheckUrlMaxLength)]
    public required string HealthCheckUrl { get; set; }

    [Required]
    [StringLength(ServiceRegistrationConstants.VersionMaxLength)]
    public required string Version { get; set; }

    [Required]
    [StringLength(ServiceRegistrationConstants.EndpointMaxLength)]
    public required string Endpoint { get; set; }

    public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastHealthCheckAt { get; set; }

    [ForeignKey(nameof(Owner))]
    public Guid OwnerId { get; set; }

    public User? Owner { get; set; }

    public int HealthCheckIntervalSeconds { get; set; } = ServiceRegistrationConstants.DefaultHealthCheckIntervalSeconds;

    public int TimeoutSeconds { get; set; } = ServiceRegistrationConstants.DefaultTimeoutSeconds;

    public bool IsEnabled { get; set; } = true;

    public int ConsecutiveFailures { get; set; }

    public int TotalRequests { get; set; }

    public int SuccessfulRequests { get; set; }

    [StringLength(ServiceRegistrationConstants.SystemdServiceNameMaxLength)]
    public string? SystemdServiceName { get; set; }

    // Navigation
    public ICollection<HealthCheckResult> HealthCheckResults { get; set; } = new List<HealthCheckResult>();

    public ICollection<ServiceMetric> Metrics { get; set; } = new List<ServiceMetric>();

    public ICollection<ServiceEvent> Events { get; set; } = new List<ServiceEvent>();

    /// <summary>
    /// Validates the service registration has required configuration.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ServiceName) &&
               !string.IsNullOrWhiteSpace(HealthCheckUrl) &&
               !string.IsNullOrWhiteSpace(Endpoint) &&
               !string.IsNullOrWhiteSpace(Version) &&
               OwnerId != Guid.Empty &&
               HealthCheckIntervalSeconds > 0 &&
               TimeoutSeconds > 0;
    }

    /// <summary>
    /// Calculates the success rate percentage.
    /// </summary>
    public decimal GetSuccessRate()
    {
        if (TotalRequests == 0)
            return ServiceRegistrationConstants.SuccessRatePercentage;

        return (decimal)SuccessfulRequests / TotalRequests * 100;
    }

    /// <summary>
    /// Records a successful health check and resets failure counter.
    /// </summary>
    public void RecordSuccessfulHealthCheck()
    {
        LastHealthCheckAt = DateTime.UtcNow;
        SuccessfulRequests++;
        TotalRequests++;
        ConsecutiveFailures = 0;
        Status = ServiceStatus.Healthy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a failed health check and tracks consecutive failures.
    /// </summary>
    public void RecordFailedHealthCheck()
    {
        LastHealthCheckAt = DateTime.UtcNow;
        TotalRequests++;
        ConsecutiveFailures++;

        if (ConsecutiveFailures >= ServiceRegistrationConstants.UnhealthyFailureThreshold)
            Status = ServiceStatus.Unhealthy;
        else
            Status = ServiceStatus.Degraded;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the service as disabled.
    /// </summary>
    public void Disable(string reason)
    {
        IsEnabled = false;
        Status = ServiceStatus.Disabled;
        UpdatedAt = DateTime.UtcNow;
        Events.Add(new ServiceEvent
        {
            ServiceId = Id,
            EventType = ServiceEventType.ServiceDisabled,
            Message = reason,
            CreatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Re-enables the service.
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        Status = ServiceStatus.Unknown;
        ConsecutiveFailures = 0;
        UpdatedAt = DateTime.UtcNow;
        Events.Add(new ServiceEvent
        {
            ServiceId = Id,
            EventType = ServiceEventType.ServiceEnabled,
            Message = ServiceRegistrationConstants.ServiceReEnabledMessage,
            CreatedAt = DateTime.UtcNow
        });
    }
}
