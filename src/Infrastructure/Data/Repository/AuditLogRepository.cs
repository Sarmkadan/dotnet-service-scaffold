#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq.Expressions;
using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Audit log repository with query support for compliance and auditing.
/// </summary>
public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogRepository"/> class.
    /// </summary>
    /// <param name="context">The database context used to access audit logs.</param>
    /// <param name="logger">The logger used to record repository operations.</param>
    public AuditLogRepository(ServiceScaffoldDbContext context, ILogger<AuditLogRepository> logger) : base(context, logger)
    {
    }

    /// <summary>
    /// Gets the most recent audit logs associated with a user.
    /// </summary>
    /// <param name="userId">The identifier of the user whose audit logs to retrieve.</param>
    /// <param name="count">The maximum number of audit logs to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching audit logs in descending creation order.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int count = 50, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting audit logs for user {UserId} with count {Count}", userId, count);
        var result = await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Retrieved {Count} audit logs for user {UserId}", result.Count(), userId);
        return result;
    }

    /// <summary>
    /// Gets audit logs associated with a specific entity.
    /// </summary>
    /// <param name="entityType">The type of the entity whose audit logs to retrieve.</param>
    /// <param name="entityId">The identifier of the entity whose audit logs to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching audit logs in descending creation order.</returns>
    /// <exception cref="ArgumentException"><paramref name="entityType"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(entityType);
        _logger.LogInformation("Getting audit logs for entity {EntityType} with id {EntityId}", entityType, entityId);
        var result = await _dbSet
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Retrieved {Count} audit logs for entity {EntityType} with id {EntityId}", result.Count(), entityType, entityId);
        return result;
    }

    /// <summary>
    /// Gets the most recently created audit logs.
    /// </summary>
    /// <param name="count">The maximum number of audit logs to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the audit logs in descending creation order.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recent audit logs with count {Count}", count);
        var result = await _dbSet
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Retrieved {Count} recent audit logs", result.Count());
        return result;
    }

    /// <summary>
    /// Gets the most recent audit logs for failed actions.
    /// </summary>
    /// <param name="count">The maximum number of audit logs to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains failed audit logs in descending creation order.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task<IEnumerable<AuditLog>> GetFailedActionsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting failed audit logs with count {Count}", count);
        var result = await _dbSet
            .Where(a => a.Status == "Failure")
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Retrieved {Count} failed audit logs", result.Count());
        return result;
    }

    /// <summary>
    /// Deletes audit logs created before the configured retention period.
    /// </summary>
    /// <param name="daysToKeep">The number of days of audit logs to retain.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task DeleteOldLogsAsync(int daysToKeep = 90, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting old audit logs older than {DaysToKeep} days", daysToKeep);
        try
        {
            var threshold = DateTime.UtcNow.AddDays(-daysToKeep);

            var oldLogs = await _dbSet
                .Where(a => a.CreatedAt < threshold)
                .ToListAsync(cancellationToken);

            foreach (var log in oldLogs)
            {
                _dbSet.Remove(log);
            }

            await SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} old audit logs", oldLogs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete old audit logs older than {DaysToKeep} days", daysToKeep);
            throw;
        }
    }

    /// <summary>
    /// Gets a page of audit logs that satisfy an optional filter.
    /// </summary>
    /// <param name="predicate">An optional expression used to filter audit logs.</param>
    /// <param name="page">The one-based page number to retrieve.</param>
    /// <param name="pageSize">The maximum number of audit logs in the page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the requested page and pagination metadata.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task<PagedResult<AuditLog>> GetFilteredAsync(
        Expression<Func<AuditLog, bool>>? predicate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting filtered audit logs for page {Page} with page size {PageSize}; predicate supplied: {HasPredicate}",
            page,
            pageSize,
            predicate != null);
        var query = _dbSet.AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {ItemCount} filtered audit logs for page {Page} with total count {TotalCount}",
            items.Count,
            page,
            totalCount);

        return new PagedResult<AuditLog>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    /// <summary>
    /// Gets a page of audit logs created within an inclusive date range.
    /// </summary>
    /// <param name="from">The start of the date range, inclusive.</param>
    /// <param name="to">The end of the date range, inclusive.</param>
    /// <param name="page">The one-based page number to retrieve.</param>
    /// <param name="pageSize">The maximum number of audit logs in the page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the requested page and pagination metadata.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task<PagedResult<AuditLog>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting audit logs from {From} to {To} for page {Page} with page size {PageSize}",
            from,
            to,
            page,
            pageSize);
        var query = _dbSet
            .Where(a => a.CreatedAt >= from.UtcDateTime)
            .Where(a => a.CreatedAt <= to.UtcDateTime);

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {ItemCount} audit logs from {From} to {To} for page {Page} with total count {TotalCount}",
            items.Count,
            from,
            to,
            page,
            totalCount);

        return new PagedResult<AuditLog>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    /// <summary>
    /// Gets a page of audit logs associated with an entity type.
    /// </summary>
    /// <param name="entityType">The entity type by which to filter audit logs.</param>
    /// <param name="page">The one-based page number to retrieve.</param>
    /// <param name="pageSize">The maximum number of audit logs in the page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the requested page and pagination metadata.</returns>
    /// <exception cref="ArgumentException"><paramref name="entityType"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task<PagedResult<AuditLog>> GetByEntityTypeAsync(
        string entityType,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(entityType);
        _logger.LogInformation(
            "Getting audit logs for entity type {EntityType} for page {Page} with page size {PageSize}",
            entityType,
            page,
            pageSize);
        var query = _dbSet
            .Where(a => a.EntityType == entityType)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {ItemCount} audit logs for entity type {EntityType} for page {Page} with total count {TotalCount}",
            items.Count,
            entityType,
            page,
            totalCount);

        return new PagedResult<AuditLog>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    /// <summary>
    /// Gets a page of audit logs associated with a user.
    /// </summary>
    /// <param name="userId">The identifier of the user whose audit logs to retrieve.</param>
    /// <param name="page">The one-based page number to retrieve.</param>
    /// <param name="pageSize">The maximum number of audit logs in the page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the requested page and pagination metadata.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was canceled.</exception>
    public async Task<PagedResult<AuditLog>> GetByUserIdPagedAsync(
        Guid userId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting audit logs for user {UserId} for page {Page} with page size {PageSize}",
            userId,
            page,
            pageSize);
        var query = _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {ItemCount} audit logs for user {UserId} for page {Page} with total count {TotalCount}",
            items.Count,
            userId,
            page,
            totalCount);

        return new PagedResult<AuditLog>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
}
