#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Constants for ServiceDiscoveryRecord class.
/// </summary>
internal static class ServiceDiscoveryRecordConstants
{
    /// <summary>Default URI scheme for service endpoints.</summary>
    public const string DefaultScheme = "https";

    /// <summary>Maximum length for service name.</summary>
    public const int ServiceNameMaxLength = 200;

    /// <summary>Maximum length for version string.</summary>
    public const int VersionMaxLength = 50;

    /// <summary>Maximum length for host name or IP address.</summary>
    public const int HostMaxLength = 253;

    /// <summary>Minimum valid TCP port number.</summary>
    public const int PortMinValue = 1;

    /// <summary>Maximum valid TCP port number.</summary>
    public const int PortMaxValue = 65535;

    /// <summary>Maximum length for URI scheme string.</summary>
    public const int SchemeMaxLength = 10;

    /// <summary>Minimum weight for weighted load balancing.</summary>
    public const int WeightMinValue = 1;

    /// <summary>Maximum weight for weighted load balancing.</summary>
    public const int WeightMaxValue = 100;

    /// <summary>Default weight for weighted load balancing.</summary>
    public const int DefaultWeight = 10;

    /// <summary>Minimum failover priority value.</summary>
    public const int PriorityMinValue = 0;

    /// <summary>Maximum failover priority value.</summary>
    public const int PriorityMaxValue = 65535;

    /// <summary>Default stale threshold in minutes.</summary>
    public const int DefaultStaleThresholdMinutes = 5;

    /// <summary>Default consecutive failures threshold for critical health status.</summary>
    public const int DefaultCriticalFailureThreshold = 3;
}