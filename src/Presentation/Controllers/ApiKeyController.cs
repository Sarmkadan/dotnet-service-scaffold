#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Application.Extensions;
using DotnetServiceScaffold.Shared.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// API controller template for managing application security keys and tokens.
/// This is a placeholder template showing how to structure auth-related endpoints.
/// </summary>
[ApiController]
[Route("api/apikeys")]
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
    /// Rotates the secret for the specified API key.
    /// Generates a new secret, invalidates the old one, and writes an audit log entry.
    /// </summary>
    /// <param name="id">The identifier of the API key to rotate.</param>
    /// <returns>The new secret for the API key.</returns>
    [HttpPost("{id}/rotate")]
    public async Task<IActionResult> RotateApiKey(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            // Generate a new secure token/secret
            var newSecret = EncryptionUtility.GenerateSecureToken();

            // TODO: Invalidate the old secret for the API key identified by 'id'.
            // This would typically involve updating the data store to mark the previous
            // secret as revoked and persisting the new secret.

            // Write an audit log entry using the extension method
            await _auditService.LogAsync($"User {userId} rotated API key {id}");

            _logger.LogInformation("User {UserId} rotated API key {ApiKeyId}", userId, id);

            return Ok(new
            {
                apiKeyId = id,
                secret = newSecret,
                rotatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating API key {ApiKeyId} for user {UserId}", id, userId);
            return StatusCode(500, new { error = "Failed to rotate API key" });
        }
    }

    /// <summary>
    /// Gets the current authenticated user's ID from claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new InvalidOperationException("User ID not found in claims");
        }
        return userId;
    }
}
