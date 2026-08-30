#nullable enable

namespace DotnetServiceScaffold.Shared.Configuration;

/// <summary>
/// Constants for DotnetServiceScaffoldOptions.
/// </summary>
internal static class DotnetServiceScaffoldOptionsConstants
{
    // Health check interval constants
    public const int HealthCheckIntervalMin = 5;
    public const int HealthCheckIntervalMax = 3600;
    public const int DefaultHealthCheckInterval = 60;

    // Health check timeout constants
    public const int HealthCheckTimeoutMin = 1;
    public const int HealthCheckTimeoutMax = 300;
    public const int DefaultHealthCheckTimeout = 10;

    // Max concurrent health checks constants
    public const int MaxConcurrentHealthChecksMin = 1;
    public const int MaxConcurrentHealthChecksMax = 100;
    public const int DefaultMaxConcurrentHealthChecks = 5;

    // Audit log retention constants
    public const int DefaultAuditLogRetentionDays = 90;
    public const int AuditLogRetentionDaysMin = 1;
    public const int AuditLogRetentionDaysMax = 3650;

    // Health check result retention constants
    public const int DefaultHealthCheckResultRetentionDays = 30;
    public const int HealthCheckResultRetentionDaysMin = 1;
    public const int HealthCheckResultRetentionDaysMax = 365;

    // Max failed login attempts constants
    public const int DefaultMaxFailedLoginAttempts = 5;
    public const int MaxFailedLoginAttemptsMin = 1;
    public const int MaxFailedLoginAttemptsMax = 20;

    // Account lockout duration constants
    public const int DefaultAccountLockoutDurationMinutes = 30;
    public const int AccountLockoutDurationMinutesMin = 1;
    public const int AccountLockoutDurationMinutesMax = 1440;

    // Password minimum length constants
    public const int DefaultPasswordMinimumLength = 8;
    public const int PasswordMinimumLengthMin = 4;
    public const int PasswordMinimumLengthMax = 128;

    // CORS settings
    public const bool DefaultEnableCors = false;

    // Error handling
    public const bool DefaultEnableDetailedErrors = true;

    // Rate limiting
    public const int DefaultRateLimitPerMinute = 60;
    public const int RateLimitPerMinuteMin = 10;
    public const int RateLimitPerMinuteMax = 10000;

    // Service registration
    public const int DefaultMaxServiceRegistrations = 100;
    public const int MaxServiceRegistrationsMin = 1;
    public const int MaxServiceRegistrationsMax = 1000;

    // Response limits
    public const int DefaultMaxResponseSize = 1048576; // 1MB
    public const int MaxResponseSizeMin = 1024; // 1KB
    public const int MaxResponseSizeMax = 10485760; // 10MB

    // Pagination
    public const int DefaultPageSize = 50;
    public const int DefaultPageSizeMin = 1;
    public const int DefaultPageSizeMax = 1000;

    public const int DefaultMaxPageSize = 200;
    public const int MaxPageSizeMin = 10;
    public const int MaxPageSizeMax = 10000;

    // Caching
    public const int DefaultCacheDurationSeconds = 300; // 5 minutes
    public const int CacheDurationSecondsMin = 1;
    public const int CacheDurationSecondsMax = 86400; // 24 hours

    // Logging
    public const bool DefaultEnableRequestLogging = true;

    // Collection limits
    public const int DefaultMaxCollectionSize = 1000;
    public const int MaxCollectionSizeMin = 10;
    public const int MaxCollectionSizeMax = 10000;

    // API key settings
    public const string DefaultApiKeyPrefix = "sk_live_";
    public const string ApiKeyPrefixRegexPattern = @"^[a-zA-Z0-9_]+_$";
    public const int DefaultApiKeyLength = 32;
    public const int ApiKeyLengthMin = 16;
    public const int ApiKeyLengthMax = 64;

    // JWT settings
    public const int DefaultJwtTokenExpirationMinutes = 60;
    public const int JwtTokenExpirationMinutesMin = 5;
    public const int JwtTokenExpirationMinutesMax = 1440;

    // Database settings
    public const string DefaultDatabaseMigrationStrategy = "Auto";
    public const bool DefaultEnableDatabaseBackup = false;
    public const string DefaultBackupDirectory = "/app/backups";

    // Metrics settings
    public const string DefaultMetricsProtectionMode = "ApiKey";
    public const string DefaultMetricsApiKey = "";
    public const string MetricsApiKeyRegexPattern = @"^[a-zA-Z0-9_]+$";
}