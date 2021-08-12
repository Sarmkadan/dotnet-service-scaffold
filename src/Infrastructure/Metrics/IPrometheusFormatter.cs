#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Formats application metrics as a Prometheus text exposition document.
/// </summary>
public interface IPrometheusFormatter
{
    /// <summary>
    /// Formats the provided metrics dictionary into Prometheus text format.
    /// </summary>
    /// <param name="metrics">Dictionary of metric name → metric data objects from <see cref="IMetricsService"/>.</param>
    /// <param name="applicationName">Application name used as metric name prefix.</param>
    /// <returns>Prometheus text-format document.</returns>
    string Format(Dictionary<string, object> metrics, string applicationName = "app");
}
