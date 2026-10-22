#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Default values and validation limits for <see cref="MetricsOptions"/>.
/// </summary>
internal static class MetricsOptionsConstants
{
    /// <summary>Metrics are enabled unless configured otherwise.</summary>
    public const bool DefaultEnabled = true;

    /// <summary>Default metrics listener port (the OpenTelemetry Prometheus exporter default).</summary>
    public const int DefaultPort = 9464;

    /// <summary>Default metric name prefix (matches ProgramConstants.MetricsPrefix).</summary>
    public const string DefaultPrefix = "scaffold";

    /// <summary>Lowest accepted port. Ports below 1024 are privileged.</summary>
    public const int MinPort = 1024;

    /// <summary>Highest accepted TCP/UDP port.</summary>
    public const int MaxPort = 65535;

    /// <summary>
    /// Metric prefix pattern: lowercase letters, digits and underscores, not starting with a digit.
    /// Uses <c>\z</c> instead of <c>$</c> so that a trailing newline cannot pass as end of string.
    /// </summary>
    public const string PrefixPattern = @"^[a-z_][a-z0-9_]*\z";
}
