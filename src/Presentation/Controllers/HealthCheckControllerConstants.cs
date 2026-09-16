#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Centralised constants for <see cref="HealthCheckController"/> to avoid magic values.
/// </summary>
internal static class HealthCheckControllerConstants
{
    // Route templates and response content types
    public const string RouteTemplate = "api/[controller]";
    public const string ResponseContentType = "application/json";
    public const string SummaryRoute = "summary";
    public const string CheckRoute = "{serviceId}/check";
    public const string HistoryRoute = "{serviceId}/history";
    public const string StatusRoute = "{serviceId}/status";
    public const string FailuresRoute = "{serviceId}/failures";

    // Log messages
    public const string LogGetHealthSummaryCalled = "GetHealthSummary called";
    public const string LogGetHealthSummaryCompleted = "GetHealthSummary completed for {ServiceCount} services";
    public const string LogGetHealthSummaryFailed = "Failed to get aggregated health summary";
    public const string LogCheckServiceHealthCalled = "CheckServiceHealth called with {ServiceId}";
    public const string LogCheckServiceHealthCompleted = "CheckServiceHealth completed for {ServiceId}";
    public const string LogServiceNotFound = "Service not found: {ServiceId}";
    public const string LogCheckServiceHealthFailed = "Failed to check health for service {ServiceId}";
    public const string LogHealthCheckError = "Health check error for service {ServiceId}";
    public const string LogGetHealthHistoryCalled = "GetHealthHistory called with {ServiceId} and {Count}";
    public const string LogGetHealthHistoryCompleted = "GetHealthHistory completed for {ServiceId} with requested count {Count}";
    public const string LogHealthHistoryServiceNotFound = "Health history unavailable because service {ServiceId} was not found";
    public const string LogGetHealthHistoryFailed = "Failed to get health history for service {ServiceId} with requested count {Count}";
    public const string LogGetHealthStatusCalled = "GetHealthStatus called with {ServiceId}";
    public const string LogGetHealthStatusCompleted = "GetHealthStatus completed for {ServiceId}";
    public const string LogHealthStatusServiceNotFound = "Health status unavailable because service {ServiceId} was not found";
    public const string LogGetHealthStatusFailed = "Failed to get health status for service {ServiceId}";
    public const string LogGetFailedChecksCalled = "GetFailedChecks called with {ServiceId} and {HoursBack}";
    public const string LogGetFailedChecksCompleted = "GetFailedChecks completed for {ServiceId} with {HoursBack} hours back";
    public const string LogFailedChecksServiceNotFound = "Failed checks unavailable because service {ServiceId} was not found";
    public const string LogGetFailedChecksFailed = "Failed to get failed checks for service {ServiceId} with {HoursBack} hours back";

    // Error response messages
    public const string ErrorFailedRetrieveHealthSummary = "Failed to retrieve health summary";

    // Defaults and formatting
    public const int LatestHealthCheckCount = 1;
    public const int DefaultHistoryCount = 20;
    public const int DefaultHoursBack = 24;
    public const string SuccessRateFormat = "F2";
    public const string PercentSymbol = "%";

    // Health status severity values
    public const int ErrorSeverity = 5;
    public const int TimeoutSeverity = 4;
    public const int UnhealthySeverity = 3;
    public const int DegradedSeverity = 2;
    public const int UnknownSeverity = 1;
    public const int HealthySeverity = 0;
}
