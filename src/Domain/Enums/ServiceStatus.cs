#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Enums;

/// <summary>
/// Represents the operational status of a monitored service.
/// </summary>
public enum ServiceStatus
{
    Unknown = 0,
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3,
    Disabled = 4,
    Maintenance = 5
}
