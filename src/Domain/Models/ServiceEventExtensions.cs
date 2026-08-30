#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Extension methods for <see cref="ServiceEvent"/> providing additional utility functionality.
/// </summary>
public static class ServiceEventExtensions
{
    /// <summary>
    /// Determines if the event occurred within the last specified time span.
    /// </summary>
    /// <param name="serviceEvent">The service event to check.</param>
    /// <param name="timeSpan">The time span to check against (e.g., <see cref="TimeSpan.FromHours(double)"/>).</param>
    /// <returns>True if the event occurred within the specified time span, otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceEvent"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeSpan"/> is negative.</exception>
    public static bool IsRecent(this ServiceEvent serviceEvent, TimeSpan timeSpan)
    {
        ArgumentNullException.ThrowIfNull(serviceEvent);
        if (timeSpan < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeSpan), "Time span cannot be negative.");
        }

        return DateTime.UtcNow - serviceEvent.CreatedAt <= timeSpan;
    }

    /// <summary>
    /// Gets a formatted display string for the event including severity and type.
    /// </summary>
    /// <param name="serviceEvent">The service event.</param>
    /// <returns>Formatted string suitable for display in logs or UI.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceEvent"/> is <see langword="null"/>.</exception>
    public static string GetDisplayString(this ServiceEvent serviceEvent)
    {
        ArgumentNullException.ThrowIfNull(serviceEvent);

        return $"[{serviceEvent.CreatedAt:yyyy-MM-dd HH:mm:ss}] [{serviceEvent.Severity ?? ServiceEventExtensionsConstants.DefaultSeverityWhenNull}] {serviceEvent.GetEventTypeDescription()}: {serviceEvent.Message ?? ServiceEventExtensionsConstants.DefaultMessageWhenNull}";
    }

    /// <summary>
    /// Determines if the event is from a specific service by service ID.
    /// </summary>
    /// <param name="serviceEvent">The service event.</param>
    /// <param name="serviceId">The service ID to check against.</param>
    /// <returns>True if the event belongs to the specified service, otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceEvent"/> is <see langword="null"/>.</exception>
    public static bool BelongsToService(this ServiceEvent serviceEvent, Guid serviceId)
    {
        ArgumentNullException.ThrowIfNull(serviceEvent);

        return serviceEvent.ServiceId == serviceId;
    }

    /// <summary>
    /// Gets a priority level (0-5) for the event based on severity and type.
    /// Higher values indicate higher priority for alerting and processing.
    /// </summary>
    /// <param name="serviceEvent">The service event.</param>
    /// <returns>Priority level from 0 (lowest) to 5 (highest).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceEvent"/> is <see langword="null"/>.</exception>
    public static int GetPriorityLevel(this ServiceEvent serviceEvent)
    {
        ArgumentNullException.ThrowIfNull(serviceEvent);

        // Map severity to base priority
        int severityPriority = ServiceEventExtensionsConstants.DefaultSeverityPriority;
        if (serviceEvent.Severity != null)
        {
            var severityLower = serviceEvent.Severity.ToLowerInvariant();
            severityPriority = severityLower switch
            {
                ServiceEventExtensionsConstants.CriticalSeverity => ServiceEventExtensionsConstants.CriticalSeverityPriority,
                ServiceEventExtensionsConstants.ErrorSeverity => ServiceEventExtensionsConstants.ErrorSeverityPriority,
                ServiceEventExtensionsConstants.WarningSeverity => ServiceEventExtensionsConstants.WarningSeverityPriority,
                ServiceEventExtensionsConstants.InfoSeverity => ServiceEventExtensionsConstants.InfoSeverityPriority,
                _ => ServiceEventExtensionsConstants.DefaultSeverityPriority
            };
        }

        // Add additional priority based on event type
        var typePriority = serviceEvent.EventType switch
        {
            ServiceEventType.ServiceDown => ServiceEventExtensionsConstants.ServiceDownPriority,
            ServiceEventType.HealthCheckFailed => ServiceEventExtensionsConstants.HealthCheckFailedPriority,
            ServiceEventType.ErrorOccurred => ServiceEventExtensionsConstants.ErrorOccurredPriority,
            ServiceEventType.ServiceRestarted => ServiceEventExtensionsConstants.ServiceRestartedPriority,
            ServiceEventType.DeploymentStarted => ServiceEventExtensionsConstants.DeploymentStartedPriority,
            ServiceEventType.DeploymentCompleted => ServiceEventExtensionsConstants.DeploymentCompletedPriority,
            ServiceEventType.ConfigurationChanged => ServiceEventExtensionsConstants.ConfigurationChangedPriority,
            ServiceEventType.ServiceUp => ServiceEventExtensionsConstants.ServiceUpPriority,
            ServiceEventType.HealthCheckPassed => ServiceEventExtensionsConstants.HealthCheckPassedPriority,
            ServiceEventType.ServiceDisabled => ServiceEventExtensionsConstants.ServiceDisabledPriority,
            ServiceEventType.ServiceEnabled => ServiceEventExtensionsConstants.ServiceEnabledPriority,
            _ => ServiceEventExtensionsConstants.UnknownEventTypePriority
        };

        return Math.Min(ServiceEventExtensionsConstants.MaxPriorityLevel, Math.Max(ServiceEventExtensionsConstants.MinPriorityLevel, severityPriority + typePriority));
    }
}