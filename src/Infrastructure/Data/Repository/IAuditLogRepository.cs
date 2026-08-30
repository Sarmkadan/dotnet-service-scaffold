#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq.Expressions;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Repository interface for AuditLog entity operations.
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int count = IAuditLogRepositoryConstants.DefaultUserIdCount, CancellationToken cancellationToken = default);

    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);

    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = IAuditLogRepositoryConstants.DefaultRecentLogsCount, CancellationToken cancellationToken = default);

    Task<IEnumerable<AuditLog>> GetFailedActionsAsync(int count = IAuditLogRepositoryConstants.DefaultFailedActionsCount, CancellationToken cancellationToken = default);

    Task DeleteOldLogsAsync(int daysToKeep = IAuditLogRepositoryConstants.DefaultDaysToKeep, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs with composable filtering predicates.
    /// </summary>
    /// <param name="predicate">Optional filter predicate</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    Task<PagedResult<AuditLog>> GetFilteredAsync(
        Expression<Func<AuditLog, bool>>? predicate = null,
        int page = IAuditLogRepositoryConstants.DefaultPageNumber,
        int pageSize = IAuditLogRepositoryConstants.DefaultPageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs filtered by date range.
    /// </summary>
    /// <param name="from">Filter logs from this date (inclusive)</param>
    /// <param name="to">Filter logs to this date (inclusive)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    Task<PagedResult<AuditLog>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int page = IAuditLogRepositoryConstants.DefaultPageNumber,
        int pageSize = IAuditLogRepositoryConstants.DefaultPageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs filtered by entity type.
    /// </summary>
    /// <param name="entityType">Entity type to filter by</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    Task<PagedResult<AuditLog>> GetByEntityTypeAsync(
        string entityType,
        int page = IAuditLogRepositoryConstants.DefaultPageNumber,
        int pageSize = IAuditLogRepositoryConstants.DefaultPageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs filtered by user ID.
    /// </summary>
    /// <param name="userId">User ID to filter by</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    Task<PagedResult<AuditLog>> GetByUserIdPagedAsync(
        Guid userId,
        int page = IAuditLogRepositoryConstants.DefaultPageNumber,
        int pageSize = IAuditLogRepositoryConstants.DefaultPageSize,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a paged result set.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
