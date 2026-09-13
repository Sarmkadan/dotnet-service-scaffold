#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Enums;

/// <summary>
/// Extension methods for HealthStatus enum.
/// </summary>
public static class HealthStatusExtensions
{
    /// <summary>
    /// Determines whether the health status represents a healthy state.
    /// </summary>
    /// <param name="status">The health status to evaluate.</param>
    /// <returns>True if the status is Healthy; otherwise, false.</returns>
    public static bool IsHealthy(this HealthStatus status)
    {
        return status == HealthStatus.Healthy;
    }
}