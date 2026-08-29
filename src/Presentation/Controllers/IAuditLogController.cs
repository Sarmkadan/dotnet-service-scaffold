#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Interface for AuditLogController operations.
/// </summary>
public interface IAuditLogController
{
    Task<IActionResult> ListAuditLogs(
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? entityType,
        Guid? userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<IActionResult> GetAuditLog(Guid id, CancellationToken cancellationToken);

    Task<IActionResult> GetUserAuditLogs(Guid userId, int days, CancellationToken cancellationToken);
}