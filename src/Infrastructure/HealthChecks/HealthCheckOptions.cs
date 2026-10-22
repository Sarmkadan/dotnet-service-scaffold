#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Configuration options for <c>HealthCheckService</c> scheduling and the health endpoint.
/// Bind this section from appsettings.json under the "HealthCheck" key.
/// </summary>
public class HealthCheckOptions
{
    /// <summary>Configuration section key used in appsettings.json.</summary>
    public const string SectionName = "HealthCheck";

    /// <summary>
    /// Time between two health check runs. Must be greater than zero.
    /// Bound from a <c>hh:mm:ss</c> string, for example <c>00:01:00</c>.
    /// </summary>
    public TimeSpan Interval { get; set; } = HealthCheckOptionsConstants.DefaultInterval;

    /// <summary>
    /// Maximum time a single health check may run. Must be less than <see cref="Interval"/>.
    /// Bound from a <c>hh:mm:ss</c> string, for example <c>00:00:10</c>.
    /// </summary>
    public TimeSpan Timeout { get; set; } = HealthCheckOptionsConstants.DefaultTimeout;

    /// <summary>
    /// Path of the health endpoint. Must start with <c>/</c>.
    /// </summary>
    public string Endpoint { get; set; } = HealthCheckOptionsConstants.DefaultEndpoint;
}
