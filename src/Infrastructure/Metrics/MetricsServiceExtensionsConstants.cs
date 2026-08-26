#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Constants used by <see cref="MetricsServiceExtensions"/>.
/// </summary>
internal static class MetricsServiceExtensionsConstants
{
    /// <summary>
    /// Default amount by which a counter is incremented when no explicit value is provided.
    /// </summary>
    public const long DefaultCounterIncrement = 1L;

    /// <summary>
    /// Gauge value recorded to indicate presence of a resource.
    /// </summary>
    public const double ZeroGaugeValue = 0D;

    /// <summary>
    /// Lower bound for measured elapsed milliseconds; stopwatch durations are never negative.
    /// </summary>
    public const long MinimumElapsedMilliseconds = 0L;
}
