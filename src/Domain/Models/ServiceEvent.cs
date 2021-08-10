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
/// Records significant events that occur on a service (restarts, status changes, errors, etc).
/// </summary>
public class ServiceEvent
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Service))]
    public Guid ServiceId { get; set; }

    public ServiceRegistration? Service { get; set; }

    public ServiceEventType EventType { get; set; }

    [StringLength(2000)]
    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(50)]
    public string? Severity { get; set; }

    [StringLength(255)]
    public string? SourceHost { get; set; }

    [StringLength(500)]
    public string? StackTrace { get; set; }

    public bool AcknowledgedAt { get; set; }

    public DateTime? AcknowledgedBy { get; set; }

    /// <summary>
    /// Determines if this event requires immediate attention.
    /// </summary>
    public bool IsAlertWorthy()
    {
        return Severity == "Critical" ||
               EventType == ServiceEventType.ServiceDown ||
               EventType == ServiceEventType.HealthCheckFailed;
    }

    /// <summary>
    /// Marks the event as acknowledged by an operator.
    /// </summary>
    public void Acknowledge()
    {
        AcknowledgedAt = true;
        AcknowledgedBy = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a descriptive name for the event type.
    /// </summary>
    public string GetEventTypeDescription()
    {
        return EventType switch
        {
            ServiceEventType.ServiceUp => "Service Started",
            ServiceEventType.ServiceDown => "Service Stopped",
            ServiceEventType.ServiceRestarted => "Service Restarted",
            ServiceEventType.HealthCheckFailed => "Health Check Failed",
            ServiceEventType.HealthCheckPassed => "Health Check Passed",
            ServiceEventType.ConfigurationChanged => "Configuration Updated",
            ServiceEventType.ServiceDisabled => "Service Disabled",
            ServiceEventType.ServiceEnabled => "Service Enabled",
            ServiceEventType.ErrorOccurred => "Error Occurred",
            ServiceEventType.DeploymentStarted => "Deployment Started",
            ServiceEventType.DeploymentCompleted => "Deployment Completed",
            _ => "Unknown Event"
        };
    }
}
