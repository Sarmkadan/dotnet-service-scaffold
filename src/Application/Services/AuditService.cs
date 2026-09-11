#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service for audit logging, compliance tracking, and activity monitoring.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditService> _logger;

    /// <summary>
    /// Initializes a new instance of the AuditService class.
    /// </summary>
    /// <param name="auditLogRepository">The audit log repository.</param>
    /// <param name="logger">The logger.</param>
    public AuditService(IAuditLogRepository auditLogRepository, ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Logs a successful action for audit purposes.
    /// </summary>
    /// <param name="userId">The ID of the user performing the action (can be null).</param>
    /// <param name="action">The action being performed (cannot be null or empty).</param>
    /// <param name="entityType">The type of entity the action was performed on (cannot be null or empty).</param>
    /// <param name="entityId">The ID of the entity the action was performed on (can be null).</param>
    /// <param name="description">Optional description of the action.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LogActionAsync(
        Guid? userId,
        string action,
        string entityType,
        Guid? entityId,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);
        ArgumentException.ThrowIfNullOrEmpty(entityType);
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActionName = action,
            EntityType = entityType,
            EntityId = entityId,
            Status = "Success",
            CreatedAt = DateTime.UtcNow,
            Description = description
        };

        await _auditLogRepository.AddAsync(log);
        _logger.LogInformation("Audit log: {Action} on {EntityType} {EntityId}", action, entityType, entityId);
    }

    /// <summary>
    /// Retrieves an audit log by its ID.
    /// </summary>
    /// <param name="logId">The ID of the audit log to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The audit log if found, otherwise null.</returns>
    public async Task<AuditLog?> GetAuditLogAsync(Guid logId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetByIdAsync(logId, cancellationToken);
    }

    /// <summary>
    /// Retrieves audit logs for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose audit logs to retrieve.</param>
    /// <param name="count">The maximum number of audit logs to return (default is 50).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit logs for the specified user.</returns>
    public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(Guid userId, int count = 50, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetByUserIdAsync(userId, count, cancellationToken);
    }

    /// <summary>
    /// Retrieves audit logs for a specific entity.
    /// </summary>
    /// <param name="entityType">The type of entity (cannot be null or empty).</param>
    /// <param name="entityId">The ID of the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit logs for the specified entity.</returns>
    public async Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(entityType);
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetByEntityAsync(entityType, entityId, cancellationToken);
    }

    /// <summary>
    /// Retrieves recent audit logs.
    /// </summary>
    /// <param name="count">The maximum number of recent audit logs to return (default is 100).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of recent audit logs.</returns>
    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetRecentLogsAsync(count, cancellationToken);
    }

    /// <summary>
    /// Retrieves failed actions from the audit log.
    /// </summary>
    /// <param name="count">The maximum number of failed actions to return (default is 50).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of failed audit log entries.</returns>
    public async Task<IEnumerable<AuditLog>> GetFailedActionsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetFailedActionsAsync(count, cancellationToken);
    }

    /// <summary>
    /// Logs a failed action for audit purposes.
    /// </summary>
    /// <param name="userId">The ID of the user performing the action (can be null).</param>
    /// <param name="action">The action being performed (cannot be null or empty).</param>
    /// <param name="entityType">The type of entity the action was performed on (cannot be null or empty).</param>
    /// <param name="reason">The reason for the failure (cannot be null or empty).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LogFailedActionAsync(Guid? userId, string action, string entityType, string reason)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);
        ArgumentException.ThrowIfNullOrEmpty(entityType);
        ArgumentException.ThrowIfNullOrEmpty(reason);
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActionName = action,
            EntityType = entityType,
            Status = "Failure",
            CreatedAt = DateTime.UtcNow,
            Description = reason
        };

        await _auditLogRepository.AddAsync(log);
        _logger.LogWarning("Failed action logged: {Action} on {EntityType} - {Reason}", action, entityType, reason);
    }

    /// <summary>
    /// Cleans up old audit logs.
    /// </summary>
    /// <param name="daysToKeep">The number of days of audit logs to keep (default is 90).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CleanupOldLogsAsync(int daysToKeep = 90)
    {
        await _auditLogRepository.DeleteOldLogsAsync(daysToKeep);
        _logger.LogInformation("Cleaned up audit logs older than {DaysToKeep} days", daysToKeep);
    }
}
