#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Application.Extensions;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Presentation.Authorization;
using DotnetServiceScaffold.Shared.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Manages the calling user's scoped API keys: issue, list, revoke and rotate.
/// Read endpoints require the <c>read</c> scope; key management requires <c>admin</c>.
/// </summary>
[ApiController]
[Route(ApiKeyControllerConstants.RouteBase)]
[Authorize]
public class ApiKeyController : ControllerBase, IApiKeyController
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ApiKeyController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyController"/> class.
    /// </summary>
    /// <param name="apiKeyService">The service that issues, validates, revokes and rotates API keys.</param>
    /// <param name="auditService">The service used to record API key audit events.</param>
    /// <param name="logger">The logger used to record controller activity.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is <see langword="null"/>.</exception>
    public ApiKeyController(
        IApiKeyService apiKeyService,
        IAuditService auditService,
        ILogger<ApiKeyController> logger)
    {
        ArgumentNullException.ThrowIfNull(apiKeyService);
        ArgumentNullException.ThrowIfNull(auditService);
        ArgumentNullException.ThrowIfNull(logger);

        _apiKeyService = apiKeyService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Gets information about current API authentication state.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>An action result containing the current authentication information.</returns>
    [HttpGet(ApiKeyControllerConstants.GetAuthInfo)]
    [Authorize(Policy = ApiKeyPolicies.Read)]
    public async Task<IActionResult> GetAuthInfo(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var userId = GetCurrentUserId();

        try
        {
            _logger.LogInformation(ApiKeyControllerConstants.LogGetAuthInfoRequested, userId);

            return Ok(new
            {
                authenticated = true,
                userId = userId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ApiKeyControllerConstants.LogGetAuthInfoError, userId);
            return StatusCode(500, new { error = ApiKeyControllerConstants.ErrorFailedToRetrieveAuthInfo });
        }
    }

    /// <summary>
    /// Lists the calling user's active, unexpired API keys. Secrets are never returned.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The user's active keys.</returns>
    [HttpGet(ApiKeyControllerConstants.ListApiKeys)]
    [Authorize(Policy = ApiKeyPolicies.Read)]
    public async Task<IActionResult> ListApiKeys(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var keys = await _apiKeyService.ListActiveAsync(userId, cancellationToken);

        return Ok(keys.Select(key => new
        {
            id = key.Id,
            name = key.Name,
            scopes = ParseScopes(key.AllowedScopes),
            keyId = ApiKeyCredentials.GetLogPrefix(key.KeyPrefix),
            createdAt = key.CreatedAt,
            expiresAt = key.ExpiresAt,
            lastUsedAt = key.LastUsedAt
        }));
    }

    /// <summary>
    /// Issues a new API key. The secret is returned in this response only.
    /// </summary>
    /// <param name="request">The key name, scopes, allow list and expiry.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The created key, including its one-time secret.</returns>
    [HttpPost(ApiKeyControllerConstants.CreateApiKey)]
    [Authorize(Policy = ApiKeyPolicies.Admin)]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = GetCurrentUserId();

        var issued = await _apiKeyService.CreateAsync(userId, request.Name, request.Scopes, request.AllowedIps, request.ExpiresAt, cancellationToken);
        await _auditService.LogAsync(string.Format(ApiKeyControllerConstants.AuditKeyCreated, userId, issued.ApiKey.Id), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new
        {
            id = issued.ApiKey.Id,
            name = issued.ApiKey.Name,
            scopes = ParseScopes(issued.ApiKey.AllowedScopes),
            expiresAt = issued.ApiKey.ExpiresAt,
            secret = issued.PlaintextKey
        });
    }

    /// <summary>
    /// Revokes one of the calling user's API keys. Revoking an already revoked key succeeds.
    /// </summary>
    /// <param name="id">The identifier of the key to revoke.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>No content on success; not found when the key does not exist or belongs to another user.</returns>
    [HttpDelete(ApiKeyControllerConstants.RevokeApiKey)]
    [Authorize(Policy = ApiKeyPolicies.Admin)]
    public async Task<IActionResult> RevokeApiKey(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        await _apiKeyService.RevokeAsync(userId, id, cancellationToken);
        await _auditService.LogAsync(string.Format(ApiKeyControllerConstants.AuditKeyRevoked, userId, id), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Rotates the secret for the specified API key. A new key with the same name, scopes, allow list and expiry
    /// is issued, and the original key is revoked.
    /// </summary>
    /// <param name="id">The identifier of the API key to rotate.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The replacement key identifier and its one-time secret.</returns>
    [HttpPost(ApiKeyControllerConstants.RotateApiKey)]
    [Authorize(Policy = ApiKeyPolicies.Admin)]
    public async Task<IActionResult> RotateApiKey(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var rotated = await _apiKeyService.RotateAsync(userId, id, cancellationToken);
        await _auditService.LogAsync(string.Format(ApiKeyControllerConstants.AuditKeyRotated, userId, id, rotated.ApiKey.Id), cancellationToken);
        _logger.LogInformation(ApiKeyControllerConstants.LogRotateApiKey, userId, id);

        return Ok(new
        {
            apiKeyId = rotated.ApiKey.Id,
            replacedApiKeyId = id,
            secret = rotated.PlaintextKey,
            rotatedAt = DateTime.UtcNow
        });
    }

    private static IReadOnlyList<string> ParseScopes(string? allowedScopes) =>
        (allowedScopes ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
