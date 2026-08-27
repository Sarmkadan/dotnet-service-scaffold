#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Centralised constants for <see cref="MetricsController"/> to avoid magic values.
/// </summary>
internal static class MetricsControllerConstants
{
    // Route templates
    public const string RouteTemplate = "api/[controller]";
    public const string CategoryRoute = "category/{category}";
    public const string ResetRoute = "reset";
    public const string SummaryRoute = "summary";

    // Authorization
    public const string AdminRole = "Admin";

    // Log messages
    public const string LogRetrievedMetrics = "Retrieved metrics ({MetricCount} metrics)";
    public const string LogRetrievedMetricsByCategory = "Retrieved metrics for category '{Category}' ({MetricCount} metrics)";
    public const string LogMetricsReset = "Metrics reset by user {UserId}";
    public const string LogErrorRetrievingMetrics = "Error retrieving metrics";
    public const string LogErrorRetrievingMetricsByCategory = "Error retrieving metrics for category '{Category}'";
    public const string LogErrorResettingMetrics = "Error resetting metrics";
    public const string LogErrorRetrievingMetricsSummary = "Error retrieving metrics summary";

    // Error response messages
    public const string ErrorFailedRetrieveMetrics = "Failed to retrieve metrics";
    public const string ErrorFailedResetMetrics = "Failed to reset metrics";
    public const string ErrorFailedRetrieveMetricsSummary = "Failed to retrieve metrics summary";

    // HTTP status codes
    public const int StatusCodeInternalServerError = 500;

    // Misc constants
    public const string UnknownUser = "unknown";
    public const string DictionaryKeyType = "type";

    // Metric type identifiers
    public const string CounterMetricType = "counter";
    public const string GaugeMetricType = "gauge";
    public const string TimerMetricType = "timer";
}
