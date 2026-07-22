#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DotnetServiceScaffold.Application.Services;

namespace DotnetServiceScaffold.Application.Extensions;

/// <summary>
/// Extension methods for <see cref="IAuditService"/> to provide convenience methods
/// for audit logging without requiring changes to the original interface definition.
/// </summary>
public static class AuditServiceExtensions
{
    /// <summary>
    /// Logs a simple audit message with default values.
    /// </summary>
    /// <param name="auditService">The audit service instance. Must not be null.</param>
    /// <param name="message">The message to log. Must not be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="auditService"/> or <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is empty.</exception>
    public static Task LogAsync(
        this IAuditService auditService,
        [DisallowNull] string message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditService);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return auditService.LogActionAsync(
            userId: null,
            action: "MessageLogged",
            entityType: "System",
            entityId: null,
            description: message);
    }

    /// <summary>
    /// Logs an audit action with user context.
    /// </summary>
    /// <param name="auditService">The audit service instance. Must not be null.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <param name="action">The name of the action. Must not be null or empty.</param>
    /// <param name="entityType">The type of the affected entity. Must not be null or empty.</param>
    /// <param name="entityId">The ID of the affected entity.</param>
    /// <param name="description">Additional details about the action. Can be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="auditService"/>, <paramref name="action"/>, or <paramref name="entityType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="action"/> or <paramref name="entityType"/> is empty.</exception>
    public static Task LogActionAsync(
        this IAuditService auditService,
        Guid? userId,
        [DisallowNull] string action,
        [DisallowNull] string entityType,
        Guid? entityId,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditService);
        ArgumentException.ThrowIfNullOrEmpty(action);
        ArgumentException.ThrowIfNullOrEmpty(entityType);

        return auditService.LogActionAsync(userId, action, entityType, entityId, description);
    }

    /// <summary>
    /// Logs a failed action with user context.
    /// </summary>
    /// <param name="auditService">The audit service instance. Must not be null.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <param name="action">The name of the action. Must not be null or empty.</param>
    /// <param name="entityType">The type of the affected entity. Must not be null or empty.</param>
    /// <param name="reason">The reason for the failure. Must not be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="auditService"/>, <paramref name="action"/>, <paramref name="entityType"/>, or <paramref name="reason"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="action"/>, <paramref name="entityType"/>, or <paramref name="reason"/> is empty.</exception>
    public static Task LogFailedActionAsync(
        this IAuditService auditService,
        Guid? userId,
        [DisallowNull] string action,
        [DisallowNull] string entityType,
        [DisallowNull] string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditService);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(reason);

        return auditService.LogFailedActionAsync(userId, action, entityType, reason);
    }
}
