#nullable enable
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
    /// <summary>
    /// Logs an action for auditing purposes.
    /// </summary>
    /// <param name="userId">The ID of the user performing the action</param>
    /// <param name="action">The name of the action</param>
    /// <param name="entityType">The type of the affected entity</param>
    /// <param name="entityId">The ID of the affected entity</param>
    /// <param name="description">Additional details about the action</param>
    /// <returns>A task representing the operation</returns>
    Task LogActionAsync(Guid? userId, string action, string entityType, Guid? entityId, string? description = null);

    /// <summary>
    /// Retrieves an audit log by ID.
    /// </summary>
    /// <param name="logId">The ID of the audit log</param>
    /// <returns>The audit log if found, otherwise null</returns>
    Task<AuditLog?> GetAuditLogAsync(Guid logId);

    Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(Guid userId, int count = 50);

    Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, Guid entityId);

    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100);

    Task<IEnumerable<AuditLog>> GetFailedActionsAsync(int count = 50);

    Task LogFailedActionAsync(Guid? userId, string action, string entityType, string reason);

    Task CleanupOldLogsAsync(int daysToKeep = 90);
}
