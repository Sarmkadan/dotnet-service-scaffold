#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="ServiceStatus"/>.
/// </summary>
public static class ServiceStatusExtensions
{
    /// <summary>
    /// Returns true when the service is considered healthy and fully operational.
    /// </summary>
    public static bool IsHealthy(this ServiceStatus status)
        => status == ServiceStatus.Healthy;

    /// <summary>
    /// Returns true when the service is in a terminal state that will not
    /// recover on its own without intervention.
    /// </summary>
    public static bool IsTerminal(this ServiceStatus status)
        => status is ServiceStatus.Unhealthy or ServiceStatus.Disabled;
}