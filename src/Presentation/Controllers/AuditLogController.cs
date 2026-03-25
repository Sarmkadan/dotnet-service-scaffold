#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
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
    /// Lists audit logs with optional filtering by date range and action type.
    /// </summary>
    /// <param name="fromDate">Filter logs from this date (inclusive)</param>
    /// <param name="toDate">Filter logs to this date (inclusive)</param>
    /// <param name="action">Filter by action type (optional)</param>
    /// <param name="page">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 50, max: 1000)</param>
    /// <response code="200">Returns paginated audit logs</response>
    /// <response code="400">If parameters are invalid</response>
    /// <response code="401">If not authenticated</response>
    [HttpGet]
    public async Task<IActionResult> ListAuditLogs(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // Validate pagination
            if (page < 1)
                page = 1;

            if (pageSize < 1 || pageSize > 1000)
                pageSize = 50;

            // Get all audit logs and filter
            var allLogs = await _auditLogRepository.GetAllAsync();

            var filtered = allLogs.AsEnumerable();

            // Apply date filters
            if (fromDate.HasValue)
                filtered = filtered.Where(log => log.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                filtered = filtered.Where(log => log.CreatedAt <= toDate.Value.AddDays(1));

            // Apply action filter
            if (!string.IsNullOrEmpty(action))
                filtered = filtered.Where(log =>
                    log.ActionName?.Equals(action, StringComparison.OrdinalIgnoreCase) ?? false);

            // Sort by date descending
            filtered = filtered.OrderByDescending(log => log.CreatedAt);

            // Paginate
            var totalCount = filtered.Count();
            var logs = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PagedAuditLogResponse
            {
                Data = logs.Select(log => new AuditLogDto
                {
                    Id = log.Id,
                    UserId = log.UserId,
                    ActionName = log.ActionName,
                    EntityType = log.EntityType,
                    Description = log.Description,
                    CreatedAt = log.CreatedAt
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            _logger.LogInformation(
                "Retrieved {Count} audit logs (page {Page} of {TotalPages})",
                logs.Count, page, response.TotalPages);

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

            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            var allLogs = await _auditLogRepository.GetAllAsync();

            var logs = allLogs
                .Where(log => log.UserId == userId && log.CreatedAt >= cutoffDate)
                .OrderByDescending(log => log.CreatedAt)
                .ToList();

            var response = logs.Select(log => new AuditLogDto
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
                logs.Count, userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve audit logs" });
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
