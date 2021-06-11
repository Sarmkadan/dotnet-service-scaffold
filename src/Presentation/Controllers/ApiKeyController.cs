// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// API controller template for managing application security keys and tokens.
/// This is a placeholder template showing how to structure auth-related endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiKeyController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<ApiKeyController> _logger;

    public ApiKeyController(
        IAuditService auditService,
        ILogger<ApiKeyController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Gets information about current API authentication state.
    /// </summary>
    [HttpGet("info")]
    public async Task<IActionResult> GetAuthInfo()
    {
        var userId = GetCurrentUserId();

        try
        {
            _logger.LogInformation("User {UserId} requested auth info", userId);

            return Ok(new
            {
                authenticated = true,
                userId = userId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auth info for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve authentication info" });
        }
    }

    /// <summary>
    /// Gets the current authenticated user's ID from claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new InvalidOperationException("User ID not found in claims");
        }
        return userId;
    }
}
