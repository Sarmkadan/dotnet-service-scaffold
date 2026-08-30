namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Constants for AuditLogController to avoid magic values.
/// </summary>
internal static class AuditLogControllerConstants
{
    /// <summary>
    /// Default page number for pagination.
    /// </summary>
    public const int DefaultPage = 1;

    /// <summary>
    /// Default page size for pagination.
    /// </summary>
    public const int DefaultPageSize = 50;

    /// <summary>
    /// Maximum allowed page size for pagination.
    /// </summary>
    public const int MaxPageSize = 1000;

    /// <summary>
    /// Default number of days to look back for user audit logs.
    /// </summary>
    public const int DefaultDays = 30;

    /// <summary>
    /// Maximum allowed number of days to look back.
    /// </summary>
    public const int MaxDays = 365;

    /// <summary>
    /// Error message for failed audit log retrieval (plural).
    /// </summary>
    public const string FailedToRetrieveAuditLogs = "Failed to retrieve audit logs";

    /// <summary>
    /// Error message for failed audit log retrieval (singular).
    /// </summary>
    public const string FailedToRetrieveAuditLog = "Failed to retrieve audit log";
}