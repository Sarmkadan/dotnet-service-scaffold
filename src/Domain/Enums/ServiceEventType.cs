// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Enums;

/// <summary>
/// Types of events that can occur on a service.
/// </summary>
public enum ServiceEventType
{
    ServiceUp = 1,
    ServiceDown = 2,
    ServiceRestarted = 3,
    HealthCheckFailed = 4,
    HealthCheckPassed = 5,
    ConfigurationChanged = 6,
    ServiceDisabled = 7,
    ServiceEnabled = 8,
    ErrorOccurred = 9,
    DeploymentStarted = 10,
    DeploymentCompleted = 11
}
