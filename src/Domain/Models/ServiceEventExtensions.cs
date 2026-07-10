#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Extension methods for ServiceEvent providing additional utility functionality.
/// </summary>
public static class ServiceEventExtensions
{
    /// <summary>
    /// Determines if the event occurred within the last specified time span.
    /// </summary>
    /// <param name="serviceEvent">The service event to check</param>
    /// <param name="timeSpan">The time span to check against (e.g., TimeSpan.FromHours(1))</param>
    /// <returns>True if the event occurred within the specified time span, otherwise false</returns>
    public static bool IsRecent(this ServiceEvent serviceEvent, TimeSpan timeSpan)
    {
        if (serviceEvent == null)
        {
            throw new ArgumentNullException(nameof(serviceEvent));
        }

        return DateTime.UtcNow - serviceEvent.CreatedAt <= timeSpan;
    }

    /// <summary>
    /// Gets a formatted display string for the event including severity and type.
    /// </summary>
    /// <param name="serviceEvent">The service event</param>
    /// <returns>Formatted string suitable for display in logs or UI</returns>
    public static string GetDisplayString(this ServiceEvent serviceEvent)
    {
        if (serviceEvent == null)
        {
            throw new ArgumentNullException(nameof(serviceEvent));
        }

        return $"[{serviceEvent.CreatedAt:yyyy-MM-dd HH:mm:ss}] [{serviceEvent.Severity ?? "Info"}] {serviceEvent.GetEventTypeDescription()}: {serviceEvent.Message ?? "No message"}";
    }

    /// <summary>
    /// Determines if the event is from a specific service by service ID.
    /// </summary>
    /// <param name="serviceEvent">The service event</param>
    /// <param name="serviceId">The service ID to check against</param>
    /// <returns>True if the event belongs to the specified service, otherwise false</returns>
    public static bool BelongsToService(this ServiceEvent serviceEvent, Guid serviceId)
    {
        if (serviceEvent == null)
        {
            throw new ArgumentNullException(nameof(serviceEvent));
        }

        return serviceEvent.ServiceId == serviceId;
    }

    /// <summary>
    /// Gets a priority level (0-5) for the event based on severity and type.
    /// Higher values indicate higher priority for alerting and processing.
    /// </summary>
    /// <param name="serviceEvent">The service event</param>
    /// <returns>Priority level from 0 (lowest) to 5 (highest)</returns>
    public static int GetPriorityLevel(this ServiceEvent serviceEvent)
    {
        if (serviceEvent == null)
        {
            throw new ArgumentNullException(nameof(serviceEvent));
        }

        // Map severity to base priority
        int severityPriority = 2; // default
        if (serviceEvent.Severity != null)
        {
            var severityLower = serviceEvent.Severity.ToLowerInvariant();
            severityPriority = severityLower switch
            {
                "critical" => 5,
                "error" => 4,
                "warning" => 3,
                "info" => 1,
                _ => 2
            };
        }

        // Add additional priority based on event type
        var typePriority = serviceEvent.EventType switch
        {
            ServiceEventType.ServiceDown => 3,
            ServiceEventType.HealthCheckFailed => 4,
            ServiceEventType.ErrorOccurred => 3,
            ServiceEventType.ServiceRestarted => 2,
            ServiceEventType.DeploymentStarted => 2,
            ServiceEventType.DeploymentCompleted => 1,
            ServiceEventType.ConfigurationChanged => 1,
            ServiceEventType.ServiceUp => 1,
            ServiceEventType.HealthCheckPassed => 0,
            ServiceEventType.ServiceDisabled => 2,
            ServiceEventType.ServiceEnabled => 1,
            _ => 1
        };

        return Math.Min(5, Math.Max(0, severityPriority + typePriority));
    }
}