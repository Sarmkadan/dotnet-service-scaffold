// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service interface for audit logging and compliance tracking.
/// </summary>
public interface IAuditService
{
    Task LogActionAsync(Guid? userId, string action, string entityType, Guid? entityId, string? description = null);

    Task<AuditLog?> GetAuditLogAsync(Guid logId);

    Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(Guid userId, int count = 50);

    Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, Guid entityId);

    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100);

    Task<IEnumerable<AuditLog>> GetFailedActionsAsync(int count = 50);

    Task LogFailedActionAsync(Guid? userId, string action, string entityType, string reason);

    Task CleanupOldLogsAsync(int daysToKeep = 90);
}
