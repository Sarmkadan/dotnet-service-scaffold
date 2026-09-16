#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Enums;

/// <summary>
/// Extension methods for ServiceEventType enum.
/// </summary>
public static class ServiceEventTypeExtensions
{
    /// <summary>
    /// Returns a human-readable display name for the service event type.
    /// </summary>
    /// <param name="eventType">The service event type.</param>
    /// <returns>The display name of the event type.</returns>
    public static string ToDisplayName(this ServiceEventType eventType)
    {
        return eventType switch
        {
            ServiceEventType.ServiceUp => "Service Up",
            ServiceEventType.ServiceDown => "Service Down",
            ServiceEventType.ServiceRestarted => "Service Restarted",
            ServiceEventType.HealthCheckFailed => "Health Check Failed",
            ServiceEventType.HealthCheckPassed => "Health Check Passed",
            ServiceEventType.ConfigurationChanged => "Configuration Changed",
            ServiceEventType.ServiceDisabled => "Service Disabled",
            ServiceEventType.ServiceEnabled => "Service Enabled",
            ServiceEventType.ErrorOccurred => "Error Occurred",
            ServiceEventType.DeploymentStarted => "Deployment Started",
            ServiceEventType.DeploymentCompleted => "Deployment Completed",
            _ => eventType.ToString()
        };
    }
}