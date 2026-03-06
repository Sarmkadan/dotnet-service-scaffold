// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Repository interface for AuditLog entity operations.
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int count = 50);

    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId);

    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100);

    Task<IEnumerable<AuditLog>> GetFailedActionsAsync(int count = 50);

    Task DeleteOldLogsAsync(int daysToKeep = 90);
}
