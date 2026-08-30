#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Constants for ServiceRepositoryExtensions to avoid magic values.
/// </summary>
internal static class ServiceRepositoryExtensionsConstants
{
    /// <summary>
    /// Default number of recent metrics to include per service when fetching enabled services with metrics.
    /// </summary>
    public const int DefaultMetricsCount = 10;

    /// <summary>
    /// Default time threshold in minutes for considering a service as without recent health check.
    /// </summary>
    public const int DefaultMinutesThresholdForRecentHealthCheck = 5;

    /// <summary>
    /// Default health check interval in minutes for determining when a service is due for a health check.
    /// </summary>
    public const int DefaultHealthCheckIntervalMinutes = 5;
}