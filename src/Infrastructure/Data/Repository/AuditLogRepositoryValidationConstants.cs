#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Centralizes magic values used by <see cref="AuditLogRepositoryValidation"/> so they
/// have descriptive names and a single source of truth.
/// </summary>
internal static class AuditLogRepositoryValidationConstants
{
    /// <summary>Default maximum number of audit logs returned by <c>GetByUserIdAsync</c>.</summary>
    public const int DefaultGetByUserIdCount = 50;

    /// <summary>Default maximum number of audit logs returned by <c>GetRecentLogsAsync</c>.</summary>
    public const int DefaultGetRecentLogsCount = 100;

    /// <summary>Default maximum number of failed actions returned by <c>GetFailedActionsAsync</c>.</summary>
    public const int DefaultGetFailedActionsCount = 50;

    /// <summary>Default number of days audit logs are retained by <c>DeleteOldLogsAsync</c>.</summary>
    public const int DefaultDeleteOldLogsDaysToKeep = 90;

    /// <summary>Maximum allowed value for any "count" style parameter.</summary>
    public const int MaxCount = 1000;

    /// <summary>Maximum allowed length of an entity type name.</summary>
    public const int MaxEntityTypeLength = 100;

    /// <summary>Maximum allowed log retention period in days (~10 years).</summary>
    public const int MaxDeleteOldLogsDaysToKeep = 3650;

    /// <summary>Message reported when a count parameter is not positive.</summary>
    public const string CountMustBeGreaterThanZero = "Count must be greater than zero.";

    /// <summary>Format string reported when a count parameter exceeds <see cref="MaxCount"/>.</summary>
    public const string CountExceedsMaximumAllowedValueFormat = "Count exceeds maximum allowed value of {0}.";

    /// <summary>Message reported when an entity type is null or whitespace.</summary>
    public const string EntityTypeCannotBeNullOrWhiteSpace = "Entity type cannot be null or whitespace.";

    /// <summary>Format string reported when an entity type exceeds <see cref="MaxEntityTypeLength"/>.</summary>
    public const string EntityTypeExceedsMaxLengthFormat = "Entity type exceeds maximum length of {0} characters.";

    /// <summary>Message reported when the log retention period is not positive.</summary>
    public const string DaysToKeepMustBeGreaterThanZero = "Days to keep must be greater than zero.";

    /// <summary>Format string reported when the retention period exceeds <see cref="MaxDeleteOldLogsDaysToKeep"/>.</summary>
    public const string DaysToKeepExceedsMaximumAllowedValueFormat = "Days to keep exceeds maximum allowed value of {0} days (~10 years).";

    /// <summary>Format string for the exception thrown when GetByUserIdAsync parameters are invalid.</summary>
    public const string GetByUserIdParametersInvalidFormat = "GetByUserIdAsync parameters are invalid. Problems: {0}";

    /// <summary>Format string for the exception thrown when GetByEntityAsync parameters are invalid.</summary>
    public const string GetByEntityParametersInvalidFormat = "GetByEntityAsync parameters are invalid. Problems: {0}";

    /// <summary>Format string for the exception thrown when GetRecentLogsAsync parameters are invalid.</summary>
    public const string GetRecentLogsParametersInvalidFormat = "GetRecentLogsAsync parameters are invalid. Problems: {0}";

    /// <summary>Format string for the exception thrown when GetFailedActionsAsync parameters are invalid.</summary>
    public const string GetFailedActionsParametersInvalidFormat = "GetFailedActionsAsync parameters are invalid. Problems: {0}";

    /// <summary>Format string for the exception thrown when DeleteOldLogsAsync parameters are invalid.</summary>
    public const string DeleteOldLogsParametersInvalidFormat = "DeleteOldLogsAsync parameters are invalid. Problems: {0}";
}
