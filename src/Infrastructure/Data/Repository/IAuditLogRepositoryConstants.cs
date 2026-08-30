#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Constants for IAuditLogRepository.
/// </summary>
internal static class IAuditLogRepositoryConstants
{
    /// <summary>
    /// Default count for user-specific audit logs.
    /// </summary>
    public const int DefaultUserIdCount = 50;

    /// <summary>
    /// Default count for recent audit logs.
    /// </summary>
    public const int DefaultRecentLogsCount = 100;

    /// <summary>
    /// Default count for failed actions audit logs.
    /// </summary>
    public const int DefaultFailedActionsCount = 50;

    /// <summary>
    /// Default number of days to keep audit logs before deletion.
    /// </summary>
    public const int DefaultDaysToKeep = 90;

    /// <summary>
    /// Default page number for paginated queries.
    /// </summary>
    public const int DefaultPageNumber = 1;

    /// <summary>
    /// Default page size for paginated queries.
    /// </summary>
    public const int DefaultPageSize = 50;
}