#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Configuration;

/// <summary>
/// Constants for deployment configuration values.
/// </summary>
internal static class DeploymentConfigurationConstants
{
    /// <summary>
    /// The restart delay in seconds for systemd service (on-failure).
    /// </summary>
    public const int SystemdServiceRestartSec = 10;

    /// <summary>
    /// The timeout in seconds for reverse proxy connections.
    /// </summary>
    public const int CaddyReverseProxyTimeoutSeconds = 30;

    /// <summary>
    /// The interval in seconds for reverse proxy health checks.
    /// </summary>
    public const int CaddyReverseProxyIntervalSeconds = 30;

    /// <summary>
    /// The timeout in seconds for health check HTTP requests.
    /// </summary>
    public const int CaddyHealthCheckTimeoutSeconds = 5;

    /// <summary>
    /// The latency threshold in seconds for marking health check as unhealthy.
    /// </summary>
    public const int CaddyHealthCheckUnhealthyLatencySeconds = 5;

    /// <summary>
    /// The log roll size in megabytes for Caddy logs.
    /// </summary>
    public const int CaddyLogRollSizeMb = 100;

    /// <summary>
    /// The number of rolled log files to keep for Caddy logs.
    /// </summary>
    public const int CaddyLogRollKeep = 10;

    /// <summary>
    /// The duration in hours to keep rolled log files for Caddy logs.
    /// </summary>
    public const int CaddyLogRollKeepForHours = 168;
}