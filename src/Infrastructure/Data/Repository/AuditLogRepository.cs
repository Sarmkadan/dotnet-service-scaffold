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
    public AuditLogRepository(ServiceScaffoldDbContext context, ILogger<AuditLogRepository> logger) : base(context, logger)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int count = 50, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetFailedActionsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Status == "Failure")
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteOldLogsAsync(int daysToKeep = 90, CancellationToken cancellationToken = default)
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
    }

    public async Task<PagedResult<AuditLog>> GetFilteredAsync(
        Expression<Func<AuditLog, bool>>? predicate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
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

        return new PagedResult<AuditLog>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<PagedResult<AuditLog>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(a => a.CreatedAt >= from.UtcDateTime)
            .Where(a => a.CreatedAt <= to.UtcDateTime);

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLog>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<PagedResult<AuditLog>> GetByEntityTypeAsync(
        string entityType,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(a => a.EntityType == entityType)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLog>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<PagedResult<AuditLog>> GetByUserIdPagedAsync(
        Guid userId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

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
