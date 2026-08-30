#nullable enable

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Constants for MetricsBenchmarksValidation to avoid magic strings.
/// </summary>
internal static class MetricsBenchmarksValidationConstants
{
    /// <summary>
    /// Error message when MetricsBenchmarks instance is not properly initialized.
    /// </summary>
    public const string MetricsNotInitializedError =
        "MetricsBenchmarks instance is not properly initialized. Setup() method must be called before benchmarking.";

    /// <summary>
    /// Header for invalid MetricsBenchmarks instance error message.
    /// </summary>
    public const string MetricsInvalidErrorHeader =
        "MetricsBenchmarks instance is not valid. Problems:\n";

    /// <summary>
    /// Name of the private _metrics field in MetricsBenchmarks class.
    /// </summary>
    public const string MetricsFieldName = "_metrics";
}