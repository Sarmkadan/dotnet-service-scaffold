#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Metrics;

/// <summary>
/// Constants for <see cref="MetricsServiceValidation"/>.
/// </summary>
internal static class MetricsServiceValidationConstants
{
    public const string MetricNamePattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
    public const int MaxMetricNameLength = 100;
    public const double MinGaugeValue = -1_000_000_000;
    public const double MaxGaugeValue = 1_000_000_000;
    public const long MinCounterValue = 0;
    public const long MaxCounterValue = 1_000_000_000;
    public const long MinTimingValue = 0;
    public const long MaxTimingValue = 3_600_000_000; // 1 hour in ms
    public const long MinTimerCount = 0;
    public const long DefaultMinTimerValue = long.MaxValue;
    public const long DefaultMaxTimerValue = long.MinValue;
}
