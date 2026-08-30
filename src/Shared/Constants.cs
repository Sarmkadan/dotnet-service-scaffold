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
    public const string ApplicationName = ConstantsConstants.ApplicationName;
    public const string ApplicationVersion = ConstantsConstants.ApplicationVersion;
    public const string Author = ConstantsConstants.Author;
    public const string AuthorWebsite = ConstantsConstants.AuthorWebsite;

    public const string Admin = ConstantsConstants.Admin;
    public const string ServiceOwner = ConstantsConstants.ServiceOwner;
    public const string Operator = ConstantsConstants.Operator;
    public const string Viewer = ConstantsConstants.Viewer;

    public const string ValidationError = ConstantsConstants.ValidationError;
    public const string NotFound = ConstantsConstants.NotFound;
    public const string Unauthorized = ConstantsConstants.Unauthorized;
    public const string InternalError = ConstantsConstants.InternalError;
    public const string ServiceDisabled = ConstantsConstants.ServiceDisabled;
    public const string HealthCheckFailed = ConstantsConstants.HealthCheckFailed;

    public const string HealthCheckIntervalKey = ConstantsConstants.HealthCheckIntervalKey;
    public const string HealthCheckTimeoutKey = ConstantsConstants.HealthCheckTimeoutKey;
    public const string MaxConcurrentHealthChecksKey = ConstantsConstants.MaxConcurrentHealthChecksKey;
    public const string LoggingLevelKey = ConstantsConstants.LoggingLevelKey;
    public const string MaintenanceModeKey = ConstantsConstants.MaintenanceModeKey;
    public const string ApiRateLimitKey = ConstantsConstants.ApiRateLimitKey;

    public static class Routes
    {
    }

    public static class CacheKeys
    {
    }

    public static class Limits
    {
        public const int HealthCheckIntervalSeconds = ConstantsConstants.HealthCheckIntervalSeconds;
        public const int HealthCheckTimeoutSeconds = ConstantsConstants.HealthCheckTimeoutSeconds;
        public const int HealthCheckRetries = ConstantsConstants.HealthCheckRetries;
        public const int MaxFailedLoginAttempts = ConstantsConstants.MaxFailedLoginAttempts;
        public const int AccountLockoutDurationMinutes = ConstantsConstants.AccountLockoutDurationMinutes;
        public const int PasswordMinimumLength = ConstantsConstants.PasswordMinimumLength;
        public const int PasswordMaximumLength = ConstantsConstants.PasswordMaximumLength;
        public const int ApiKeyRotationDays = ConstantsConstants.ApiKeyRotationDays;
        public const int AuditLogRetentionDays = ConstantsConstants.AuditLogRetentionDays;
        public const int HealthCheckResultRetentionDays = ConstantsConstants.HealthCheckResultRetentionDays;
    }

    public static class Headers
    {
        public const string ApiKeyHeader = ConstantsConstants.ApiKeyHeader;
        public const string UserAgentHeader = ConstantsConstants.UserAgentHeader;
        public const string TraceIdHeader = ConstantsConstants.TraceIdHeader;
    }
}