#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
// Constants for Health Check Monitor Example
// =============================================================================

internal static class HealthCheckMonitorExampleConstants
{
    // HTTP Headers
    public const string ApiKeyHeader = "X-API-Key";

    // API Endpoints
    public const string HealthCheckEndpoint = "/api/healthcheck/{0}/check";
    public const string HealthHistoryEndpoint = "/api/healthcheck/{0}/history?days={1}&limit={2}";
    public const string HealthFailuresEndpoint = "/api/healthcheck/{0}/failures?limit={1}";

    // DateTime Formats
    public const string DateTimeFormatFull = "yyyy-MM-dd HH:mm:ss";
    public const string DateTimeFormatTimeOnly = "HH:mm:ss";

    // Default Values
    public const int DefaultHistoryDays = 7;
    public const int DefaultHistoryLimit = 1000;
    public const int DefaultFailuresLimit = 50;
    public const int DefaultMonitorIntervalSeconds = 60;
    public const int ReportFailureSampleSize = 10;

    // Alert Messages
    public const string AlertCriticalFormat = "CRITICAL: {0} is UNHEALTHY!";
    public const string AlertWarningFormat = "WARNING: {0} is degraded (response: {1}ms)";
    public const string AlertRecoveryFormat = "RECOVERY: {0} is healthy again!";

    // Log Prefixes
    public const string LogAlertPrefix = "[ALERT]";
    public const string LogErrorPrefix = "[ERROR]";

    // Error Messages
    public const string ErrorHealthCheckFailed = "Health check failed: {0}";
    public const string ErrorFailedToGetHistory = "Failed to get history: {0}";
    public const string ErrorFailedToGetFailures = "Failed to get failures: {0}";

    // Example Values (for Main method)
    public const string ExampleApiKey = "sk_live_your_api_key_here";
    public const string ExampleServiceName = "UserService";
    public const string ExampleServiceId = "svc-12345678";
}