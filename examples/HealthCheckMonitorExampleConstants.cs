#nullable enable
internal static class HealthCheckMonitorExampleConstants
{
    // API Endpoints
    public const string HealthCheckEndpoint = "/api/health/{0}/check";
    public const string HealthHistoryEndpoint = "/api/health/{0}/history";
    public const string HealthFailuresEndpoint = "/api/health/{0}/failures";

    // Headers & Auth
    public const string ApiKeyHeader = "X-API-Key";

    // Error Messages
    public const string ErrorHealthCheckFailed = "Health check failed with status code: {0}";
    public const string ErrorFailedToGetHistory = "Failed to get health history with status code: {0}";
    public const string ErrorFailedToGetFailures = "Failed to get failures with status code: {0}";

    // Alerts
    public const string AlertCriticalFormat = "CRITICAL: Service {0} is unhealthy!";
    public const string AlertWarningFormat = "WARNING: Service {0} is degraded (response time: {1}ms)";
    public const string AlertRecoveryFormat = "RECOVERY: Service {0} is back online.";

    // Date/Time Formats
    public const string DateTimeFormatFull = "yyyy-MM-dd HH:mm:ss.fff";
    public const string DateTimeFormatTimeOnly = "HH:mm:ss";
    public const string DateTimeFormatReport = "yyyy-MM-dd HH:mm:ss";

    // Default Values
    public const string DefaultBaseUrl = "http://localhost:5000";
    public const int DefaultHistoryLimit = 20;
    public const int DefaultHistoryDays = 7;
    public const int DefaultFailuresLimit = 50;
    public const int DefaultMonitorIntervalSeconds = 60;
    public const int ExampleMonitorIntervalSeconds = 30;

    // Status Strings
    public const string StatusHealthy = "Healthy";
    public const string StatusDegraded = "Degraded";
    public const string StatusUnhealthy = "Unhealthy";

    // JSON Property Names
    public const string JsonPropertyData = "data";
    public const string JsonPropertyStatus = "status";
    public const string JsonPropertyResponseTime = "responseTime";
    public const string JsonPropertyId = "id";
    public const string JsonPropertyStatusCode = "statusCode";
    public const string JsonPropertyCheckedAt = "checkedAt";
    public const string JsonPropertyMessage = "message";

    // Example Values
    public const string ExampleApiKey = "your-api-key-here";
    public const string ExampleServiceName = "MyService";
    public const string ExampleServiceId = "00000000-0000-0000-0000-000000000000";
    public const int ExampleFailuresLimit = 10;
}
