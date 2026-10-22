#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Configuration options for metrics collection and Prometheus export.
/// Bind this section from appsettings.json under the "Metrics" key.
/// </summary>
public class MetricsOptions
{
    /// <summary>Configuration section key used in appsettings.json.</summary>
    public const string SectionName = "Metrics";

    /// <summary>
    /// Whether metrics are collected and exported.
    /// </summary>
    public bool Enabled { get; set; } = MetricsOptionsConstants.DefaultEnabled;

    /// <summary>
    /// Port for the metrics listener. Must be between 1024 and 65535.
    /// </summary>
    public int Port { get; set; } = MetricsOptionsConstants.DefaultPort;

    /// <summary>
    /// Prefix added to every exported metric name. Must match <c>^[a-z_][a-z0-9_]*$</c>.
    /// </summary>
    public string Prefix { get; set; } = MetricsOptionsConstants.DefaultPrefix;
}
