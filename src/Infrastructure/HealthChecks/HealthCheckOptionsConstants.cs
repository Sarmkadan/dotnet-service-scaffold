#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Default values and literals for <see cref="HealthCheckOptions"/> and its validator.
/// </summary>
internal static class HealthCheckOptionsConstants
{
    /// <summary>Default time between health check runs (matches ApplicationSettings:HealthCheckInterval).</summary>
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(60);

    /// <summary>Default maximum duration of one health check run.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    /// <summary>Default health endpoint path.</summary>
    public const string DefaultEndpoint = "/health";

    /// <summary>Required first character of the endpoint path.</summary>
    public const char EndpointPrefix = '/';
}
