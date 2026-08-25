#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Shared;

/// <summary>
/// Application-wide constants and default values.
/// </summary>
public static class Constants
{
    public const string ApplicationName = "DotnetServiceScaffold";
    public const string ApplicationVersion = "1.0.0";
    public const string Author = "Vladyslav Zaiets";
    public const string AuthorWebsite = "https://sarmkadan.com";

    public const string Admin = "Admin";
    public const string ServiceOwner = "ServiceOwner";
    public const string Operator = "Operator";
    public const string Viewer = "Viewer";

    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string InternalError = "INTERNAL_ERROR";
    public const string ServiceDisabled = "SERVICE_DISABLED";
    public const string HealthCheckFailed = "HEALTH_CHECK_FAILED";

    public const string HealthCheckIntervalKey = "HealthCheckInterval";
    public const string HealthCheckTimeoutKey = "HealthCheckTimeout";
    public const string MaxConcurrentHealthChecksKey = "MaxConcurrentHealthChecks";
    public const string LoggingLevelKey = "LoggingLevel";
    public const string MaintenanceModeKey = "MaintenanceMode";
    public const string ApiRateLimitKey = "ApiRateLimit";

    public static class Routes
    {
    }

    public static class CacheKeys
    {
    }

    public static class Limits
    {
        public const int HealthCheckIntervalSeconds = 60;
        public const int HealthCheckTimeoutSeconds = 10;
        public const int HealthCheckRetries = 3;
        public const int MaxFailedLoginAttempts = 5;
        public const int AccountLockoutDurationMinutes = 30;
        public const int PasswordMinimumLength = 8;
        public const int PasswordMaximumLength = 128;
        public const int ApiKeyRotationDays = 365;
        public const int AuditLogRetentionDays = 90;
        public const int HealthCheckResultRetentionDays = 30;
    }

    public static class Headers
    {
        public const string ApiKeyHeader = "X-API-Key";
        public const string UserAgentHeader = "User-Agent";
        public const string TraceIdHeader = "X-Trace-Id";
    }
}