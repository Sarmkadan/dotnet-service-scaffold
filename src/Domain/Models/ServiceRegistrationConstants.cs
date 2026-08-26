#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Constant values used by <see cref="ServiceRegistration"/>.
/// </summary>
internal static class ServiceRegistrationConstants
{
    /// <summary>
    /// Maximum length of the service name.
    /// </summary>
    public const int ServiceNameMaxLength = 255;

    /// <summary>
    /// Maximum length of the service description.
    /// </summary>
    public const int DescriptionMaxLength = 1000;

    /// <summary>
    /// Maximum length of the health check URL.
    /// </summary>
    public const int HealthCheckUrlMaxLength = 500;

    /// <summary>
    /// Maximum length of the service version.
    /// </summary>
    public const int VersionMaxLength = 50;

    /// <summary>
    /// Maximum length of the service endpoint.
    /// </summary>
    public const int EndpointMaxLength = 255;

    /// <summary>
    /// Maximum length of the systemd service name.
    /// </summary>
    public const int SystemdServiceNameMaxLength = 500;

    /// <summary>
    /// Default interval in seconds between health checks.
    /// </summary>
    public const int DefaultHealthCheckIntervalSeconds = 60;

    /// <summary>
    /// Default timeout in seconds for a health check request.
    /// </summary>
    public const int DefaultTimeoutSeconds = 10;

    /// <summary>
    /// Number of consecutive failures after which a service is marked unhealthy.
    /// </summary>
    public const int UnhealthyFailureThreshold = 3;

    /// <summary>
    /// Percentage used to represent a full success rate.
    /// </summary>
    public const decimal SuccessRatePercentage = 100m;

    /// <summary>
    /// Message recorded when a service is re-enabled.
    /// </summary>
    public const string ServiceReEnabledMessage = "Service re-enabled";
}