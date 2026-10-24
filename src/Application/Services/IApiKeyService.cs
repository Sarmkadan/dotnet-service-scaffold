#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Issues, validates, lists, revokes and rotates scoped API keys.
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Issues a new API key for a user. The plaintext key is only available in the returned value.
    /// </summary>
    /// <param name="userId">The owner of the key.</param>
    /// <param name="name">A human-readable label for the key.</param>
    /// <param name="scopes">The scopes to grant. Must contain at least one known scope.</param>
    /// <param name="allowedIps">An optional comma-separated allow list of source addresses.</param>
    /// <param name="expiresAt">An optional UTC expiry time.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The stored key and its plaintext value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty, too long, or <paramref name="scopes"/> contains no valid scope.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scopes"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no user exists with <paramref name="userId"/>.</exception>
    Task<IssuedApiKey> CreateAsync(
        Guid userId,
        string name,
        IEnumerable<string> scopes,
        string? allowedIps = null,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a presented API key and the address it came from.
    /// </summary>
    /// <param name="presentedKey">The key from the client's request.</param>
    /// <param name="sourceIp">The remote address of the request, or <see langword="null"/> when unknown.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The validation outcome. The key and user are populated only when the status is <see cref="Domain.Enums.ApiKeyValidationStatus.Valid"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="presentedKey"/> is <see langword="null"/> or empty.</exception>
    Task<ApiKeyValidationResult> ValidateAsync(
        string presentedKey,
        string? sourceIp,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the active, unexpired keys owned by a user. Secrets are never returned.
    /// </summary>
    /// <param name="userId">The owner whose keys to list.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The user's active keys.</returns>
    Task<IReadOnlyList<ApiKey>> ListActiveAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a key owned by a user. Revoking an already revoked key has no effect.
    /// </summary>
    /// <param name="userId">The owner of the key.</param>
    /// <param name="apiKeyId">The key to revoke.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the key does not exist or belongs to another user.</exception>
    Task RevokeAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces a key with a new key carrying the same name, scopes, allow list and expiry, then revokes the original.
    /// </summary>
    /// <param name="userId">The owner of the key.</param>
    /// <param name="apiKeyId">The key to rotate.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The replacement key and its plaintext value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key does not exist or belongs to another user.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the key is revoked or has no scopes to carry over.</exception>
    Task<IssuedApiKey> RotateAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken = default);
}
