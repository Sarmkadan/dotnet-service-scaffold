#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq.Expressions;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// API controller for querying audit logs. Provides endpoints for retrieving
/// audit trail data for compliance, security investigations, and monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogController> _logger;

    public AuditLogController(
        IAuditLogRepository auditLogRepository,
        ILogger<AuditLogController> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lists audit logs with optional filtering by date range, entity type, and user.
    /// </summary>
    /// <param name="from">Filter logs from this date (inclusive)</param>
    /// <param name="to">Filter logs to this date (inclusive)</param>
    /// <param name="entityType">Filter by entity type (optional)</param>
    /// <param name="userId">Filter by user ID (optional)</param>
    /// <param name="page">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50, max: 1000)</param>
    /// <response code="200">Returns paginated audit logs</response>
    /// <response code="400">If parameters are invalid</response>
    /// <response code="401">If not authenticated</response>
    [HttpGet]
    public async Task<IActionResult> ListAuditLogs(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? entityType,
        [FromQuery] Guid? userId,
        int page = 1,
        int pageSize = 50)
    {
        try
        {
            // Validate pagination
            if (page < 1)
                page = 1;

            if (pageSize < 1 || pageSize > 1000)
                pageSize = 50;

            // Build composable predicate
            Expression<Func<AuditLog, bool>>? predicate = null;
            var predicateList = new List<Expression<Func<AuditLog, bool>>>();

            // Apply date filters
            if (from.HasValue)
            {
                var fromDate = from.Value.UtcDateTime;
                predicateList.Add(log => log.CreatedAt >= fromDate);
            }

            if (to.HasValue)
            {
                var toDate = to.Value.UtcDateTime;
                predicateList.Add(log => log.CreatedAt <= toDate);
            }

            // Apply entity type filter
            if (!string.IsNullOrEmpty(entityType))
            {
                var entityTypeLower = entityType.ToLowerInvariant();
                predicateList.Add(log => log.EntityType != null && log.EntityType.Equals(entityTypeLower, StringComparison.OrdinalIgnoreCase));
            }

            // Apply user ID filter
            if (userId.HasValue)
            {
                predicateList.Add(log => log.UserId == userId.Value);
            }

            // Combine predicates
            if (predicateList.Count > 0)
            {
                predicate = predicateList.Aggregate((current, next) => AndAlso(current, next));
            }

            // Get filtered results with pagination
            var result = await _auditLogRepository.GetFilteredAsync(predicate, page, pageSize);

            var response = new PagedAuditLogResponse
            {
                Data = result.Items.Select(log => new AuditLogDto
                {
                    Id = log.Id,
                    UserId = log.UserId,
                    ActionName = log.ActionName,
                    EntityType = log.EntityType,
                    Description = log.Description,
                    CreatedAt = log.CreatedAt
                }).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            };

            _logger.LogInformation(
                "Retrieved {Count} audit logs (page {Page} of {TotalPages})",
                result.Items.Count, page, response.TotalPages);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, new { error = "Failed to retrieve audit logs" });
        }
    }

    /// <summary>
    /// Gets a specific audit log entry by ID.
    /// </summary>
    /// <param name="id">The audit log ID</param>
    /// <response code="200">Returns the audit log entry</response>
    /// <response code="404">If audit log not found</response>
    /// <response code="401">If not authenticated</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuditLog(Guid id)
    {
        try
        {
            var allLogs = await _auditLogRepository.GetAllAsync();
            var log = allLogs.FirstOrDefault(l => l.Id == id);

            if (log is null)
                return NotFound();

            var response = new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                ActionName = log.ActionName,
                EntityType = log.EntityType,
                Description = log.Description,
                CreatedAt = log.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log {LogId}", id);
            return StatusCode(500, new { error = "Failed to retrieve audit log" });
        }
    }

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    /// <param name="userId">The user ID to filter by</param>
    /// <param name="days">Number of days to look back (default: 30, max: 365)</param>
    /// <response code="200">Returns audit logs for the user</response>
    /// <response code="400">If parameters are invalid</response>
    /// <response code="401">If not authenticated</response>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserAuditLogs(Guid userId, [FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                days = 30;

            var result = await _auditLogRepository.GetByUserIdPagedAsync(userId, 1, 1000);

            var response = result.Items.Select(log => new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                ActionName = log.ActionName,
                EntityType = log.EntityType,
                Description = log.Description,
                CreatedAt = log.CreatedAt
            }).ToList();

            _logger.LogInformation(
                "Retrieved {Count} audit logs for user {UserId}",
                response.Count, userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve audit logs" });
        }
    }

    /// <summary>
    /// Helper method to combine predicates with AND logic.
    /// </summary>
    private static Expression<Func<T, bool>> AndAlso<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(left, right),
            parameter);
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression Visit(Expression? node)
        {
            if (node == _oldValue)
                return _newValue;
            return base.Visit(node)!;
        }
    }
}

/// <summary>
/// DTO for audit log entry.
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? ActionName { get; set; }
    public string? EntityType { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Paginated response for audit logs.
/// </summary>
public class PagedAuditLogResponse
{
    public List<AuditLogDto> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}