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

    public AuditService(IAuditLogRepository auditLogRepository, ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task LogActionAsync(
        Guid? userId,
        string action,
        string entityType,
        Guid? entityId,
        string? description = null)
    {
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

    public async Task<AuditLog?> GetAuditLogAsync(Guid logId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetByIdAsync(logId, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(Guid userId, int count = 50, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetByUserIdAsync(userId, count, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetByEntityAsync(entityType, entityId, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetRecentLogsAsync(count, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetFailedActionsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _auditLogRepository.GetFailedActionsAsync(count, cancellationToken);
    }

    public async Task LogFailedActionAsync(Guid? userId, string action, string entityType, string reason)
    {
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

    public async Task CleanupOldLogsAsync(int daysToKeep = 90)
    {
        await _auditLogRepository.DeleteOldLogsAsync(daysToKeep);
        _logger.LogInformation("Cleaned up audit logs older than {DaysToKeep} days", daysToKeep);
    }
}
