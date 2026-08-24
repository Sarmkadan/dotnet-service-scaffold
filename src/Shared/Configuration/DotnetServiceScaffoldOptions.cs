#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace DotnetServiceScaffold.Shared.Configuration;

/// <summary>
/// Root configuration options for the DotnetServiceScaffold application.
/// Binds from the "ApplicationSettings" section in appsettings.json.
/// </summary>
public sealed class DotnetServiceScaffoldOptions : IDotnetServiceScaffoldOptions, IEquatable<DotnetServiceScaffoldOptions>
{
    /// <summary>
    /// Gets or sets the health check interval in seconds.
    /// Determines how often health checks are performed for registered services.
    /// </summary>
    /// <example>60</example>
    [Range(5, 3600, ErrorMessage = "HealthCheckInterval must be between 5 and 3600 seconds")]
    public int HealthCheckInterval { get; set; } = 60;

    /// <summary>
    /// Gets or sets the health check timeout in seconds.
    /// Maximum time allowed for a single health check request to complete.
    /// </summary>
    /// <example>10</example>
    [Range(1, 300, ErrorMessage = "HealthCheckTimeout must be between 1 and 300 seconds")]
    public int HealthCheckTimeout { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of concurrent health checks.
    /// Limits parallel health check execution to prevent resource exhaustion.
    /// </summary>
    /// <example>5</example>
    [Range(1, 100, ErrorMessage = "MaxConcurrentHealthChecks must be between 1 and 100")]
    public int MaxConcurrentHealthChecks { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether maintenance mode is enabled.
    /// When true, health checks return maintenance status and services are not monitored.
    /// </summary>
    public bool MaintenanceMode { get; set; }

    /// <summary>
    /// Gets or sets the number of days to retain audit logs.
    /// Older logs are automatically purged based on this setting.
    /// </summary>
    /// <example>90</example>
    [Range(1, 3650, ErrorMessage = "AuditLogRetentionDays must be between 1 and 3650 days")]
    public int AuditLogRetentionDays { get; set; } = 90;

    /// <summary>
    /// Gets or sets the number of days to retain health check results.
    /// Historical health check data is cleaned up based on this retention policy.
    /// </summary>
    /// <example>30</example>
    [Range(1, 365, ErrorMessage = "HealthCheckResultRetentionDays must be between 1 and 365 days")]
    public int HealthCheckResultRetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of failed login attempts before account lockout.
    /// </summary>
    /// <example>5</example>
    [Range(1, 20, ErrorMessage = "MaxFailedLoginAttempts must be between 1 and 20")]
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets the duration of account lockout in minutes.
    /// </summary>
    /// <example>30</example>
    [Range(1, 1440, ErrorMessage = "AccountLockoutDurationMinutes must be between 1 and 1440 minutes")]
    public int AccountLockoutDurationMinutes { get; set; } = 30;

    /// <summary>
    /// Gets or sets the minimum password length requirement.
    /// </summary>
    /// <example>8</example>
    [Range(4, 128, ErrorMessage = "PasswordMinimumLength must be between 4 and 128 characters")]
    public int PasswordMinimumLength { get; set; } = 8;

    /// <summary>
    /// Gets or sets whether CORS is enabled for cross-origin requests.
    /// Should be disabled in production unless specific cross-origin access is required.
    /// </summary>
    public bool EnableCors { get; set; }

    /// <summary>
    /// Gets or sets the list of allowed origins for CORS.
    /// Only applicable when EnableCors is true.
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new();

    /// <summary>
    /// Gets or sets the rate limit per minute per IP address.
    /// Prevents abuse and brute force attacks.
    /// </summary>
    /// <example>60</example>
    [Range(10, 10000, ErrorMessage = "RateLimitPerMinute must be between 10 and 10000 requests")]
    public int RateLimitPerMinute { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum concurrent service registrations allowed.
    /// Prevents system overload from excessive service registrations.
    /// </summary>
    /// <example>100</example>
    [Range(1, 1000, ErrorMessage = "MaxServiceRegistrations must be between 1 and 1000")]
    public int MaxServiceRegistrations { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum response size in bytes for API responses.
    /// Large responses are truncated to prevent memory issues.
    /// </summary>
    /// <example>1048576</example>
    [Range(1024, 10485760, ErrorMessage = "MaxResponseSize must be between 1KB and 10MB")]
    public int MaxResponseSize { get; set; } = 1048576; // 1MB

    /// <summary>
    /// Gets or sets whether to enable detailed error pages in development.
    /// Should be disabled in production for security.
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = true;

    /// <summary>
    /// Gets or sets the default page size for paginated API responses.
    /// </summary>
    /// <example>50</example>
    [Range(1, 1000, ErrorMessage = "DefaultPageSize must be between 1 and 1000")]
    public int DefaultPageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets the maximum page size for paginated API responses.
    /// Clients cannot request pages larger than this value.
    /// </summary>
    /// <example>200</example>
    [Range(10, 10000, ErrorMessage = "MaxPageSize must be between 10 and 10000")]
    public int MaxPageSize { get; set; } = 200;

    /// <summary>
    /// Gets or sets the cache duration in seconds for frequently accessed data.
    /// </summary>
    /// <example>300</example>
    [Range(1, 86400, ErrorMessage = "CacheDurationSeconds must be between 1 and 86400 seconds")]
    public int CacheDurationSeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    /// Gets or sets whether to enable request logging for all endpoints.
    /// Can impact performance if enabled with high traffic.
    /// </summary>
    public bool EnableRequestLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of items to return in collection responses.
    /// Prevents excessive data transfer.
    /// </summary>
    /// <example>1000</example>
    [Range(10, 10000, ErrorMessage = "MaxCollectionSize must be between 10 and 10000")]
    public int MaxCollectionSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the API key prefix for generated API keys.
    /// Should start with a standard prefix for easy identification.
    /// </summary>
    /// <example>sk_live_</example>
    [RegularExpression(
        "^[a-zA-Z0-9_]+_$",
        ErrorMessage = "ApiKeyPrefix must contain only alphanumeric characters and underscores, ending with underscore")]
    public string ApiKeyPrefix { get; set; } = "sk_live_";

    /// <summary>
    /// Gets or sets the API key length for generated API keys.
    /// </summary>
    /// <example>32</example>
    [Range(16, 64, ErrorMessage = "ApiKeyLength must be between 16 and 64 characters")]
    public int ApiKeyLength { get; set; } = 32;

    /// <summary>
    /// Gets or sets the JWT token expiration time in minutes.
    /// </summary>
    /// <example>60</example>
    [Range(5, 1440, ErrorMessage = "JwtTokenExpirationMinutes must be between 5 and 1440 minutes")]
    public int JwtTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the JWT secret key for token signing.
    /// In production, this should be a long, random string stored in secure configuration.
    /// </summary>
    [Required(ErrorMessage = "JwtSecret is required for authentication")]
    [StringLength(1024, MinimumLength = 32, ErrorMessage = "JwtSecret must be between 32 and 1024 characters")]
    public string JwtSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database migration strategy.
    /// Options: Auto, Manual, None
    /// </summary>
    /// <example>Auto</example>
    public string DatabaseMigrationStrategy { get; set; } = "Auto";

    /// <summary>
    /// Gets or sets whether to enable automatic database backup on startup.
    /// </summary>
    public bool EnableDatabaseBackup { get; set; } = false;

    /// <summary>
    /// Gets or sets the backup directory path.
    /// Only applicable when EnableDatabaseBackup is true.
    /// </summary>
    /// <example>/app/backups</example>
    public string BackupDirectory { get; set; } = "/app/backups";

        /// <summary>
        /// Gets or sets the metrics endpoint protection mode.
        /// Options: Disabled, ApiKey, LocalhostOnly
        /// - Disabled: Metrics endpoint is publicly accessible (INSECURE - not recommended for production)
        /// - ApiKey: Metrics endpoint requires API key authentication (recommended)
        /// - LocalhostOnly: Metrics endpoint only accessible from localhost (127.0.0.1)
        /// </summary>
        /// <example>ApiKey</example>
        public string MetricsProtectionMode { get; set; } = "ApiKey";

        /// <summary>
        /// Gets or sets the metrics API key for authentication.
        /// Only used when MetricsProtectionMode is set to ApiKey.
        /// </summary>
        /// <example>metrics_sk_live_1234567890abcdef</example>
        [RegularExpression(
            "^[a-zA-Z0-9_]+$",
            ErrorMessage = "MetricsApiKey must contain only alphanumeric characters and underscores")]
        public string MetricsApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Validates the configuration options using DataAnnotations.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool Validate()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(this, validationContext, validationResults, true);

        if (!isValid && validationResults.Any())
        {
            var errorMessages = validationResults.Select(v => v.ErrorMessage).Where(m => m != null);
            throw new ValidationException(string.Join("\n", errorMessages));
        }

        return isValid;
    }

    public bool Equals(DotnetServiceScaffoldOptions? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return HealthCheckInterval == other.HealthCheckInterval &&
               HealthCheckTimeout == other.HealthCheckTimeout &&
               MaxConcurrentHealthChecks == other.MaxConcurrentHealthChecks &&
               MaintenanceMode == other.MaintenanceMode &&
               AuditLogRetentionDays == other.AuditLogRetentionDays &&
               HealthCheckResultRetentionDays == other.HealthCheckResultRetentionDays &&
               MaxFailedLoginAttempts == other.MaxFailedLoginAttempts &&
               AccountLockoutDurationMinutes == other.AccountLockoutDurationMinutes;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as DotnetServiceScaffoldOptions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            HealthCheckInterval,
            HealthCheckTimeout,
            MaxConcurrentHealthChecks,
            MaintenanceMode,
            AuditLogRetentionDays,
            HealthCheckResultRetentionDays,
            MaxFailedLoginAttempts,
            AccountLockoutDurationMinutes);
    }

    public static bool operator ==(DotnetServiceScaffoldOptions? left, DotnetServiceScaffoldOptions? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(DotnetServiceScaffoldOptions? left, DotnetServiceScaffoldOptions? right)
    {
        return !Equals(left, right);
    }
}
