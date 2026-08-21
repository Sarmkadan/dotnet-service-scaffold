namespace DotnetServiceScaffold.Shared.Configuration
{
    public interface IDotnetServiceScaffoldOptions
    {
        int HealthCheckInterval { get; set; }
        int HealthCheckTimeout { get; set; }
        int MaxConcurrentHealthChecks { get; set; }
        bool MaintenanceMode { get; set; }
        int AuditLogRetentionDays { get; set; }
        int HealthCheckResultRetentionDays { get; set; }
        int MaxFailedLoginAttempts { get; set; }
        int AccountLockoutDurationMinutes { get; set; }
        int PasswordMinimumLength { get; set; }
        bool EnableCors { get; set; }
        List<string> AllowedOrigins { get; set; }
        int RateLimitPerMinute { get; set; }
        int MaxServiceRegistrations { get; set; }
        int MaxResponseSize { get; set; }
        bool EnableDetailedErrors { get; set; }
        int DefaultPageSize { get; set; }
        int MaxPageSize { get; set; }
        int CacheDurationSeconds { get; set; }
        bool EnableRequestLogging { get; set; }
        int MaxCollectionSize { get; set; }
        string ApiKeyPrefix { get; set; }
        int ApiKeyLength { get; set; }
        int JwtTokenExpirationMinutes { get; set; }
        string JwtSecret { get; set; }
        string DatabaseMigrationStrategy { get; set; }
        bool EnableDatabaseBackup { get; set; }
        string BackupDirectory { get; set; }
        string MetricsProtectionMode { get; set; }
        string MetricsApiKey { get; set; }
        bool Validate();

}
}
