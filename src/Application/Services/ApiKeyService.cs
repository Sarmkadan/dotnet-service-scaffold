#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using DotnetServiceScaffold.Shared.Configuration;
using DotnetServiceScaffold.Shared.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Issues, validates, lists, revokes and rotates scoped API keys. Keys are stored as a public key-id plus the
/// SHA-256 digest of the secret. Authentication failures are logged with a truncated key-id only.
/// </summary>
public sealed class ApiKeyService : IApiKeyService
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOptions<DotnetServiceScaffoldOptions> _options;
    private readonly ILogger<ApiKeyService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyService"/> class.
    /// </summary>
    /// <param name="apiKeyRepository">The repository that stores API keys.</param>
    /// <param name="userRepository">The repository that stores users.</param>
    /// <param name="options">The application options that supply the key prefix and secret length.</param>
    /// <param name="logger">The logger used to record key lifecycle and authentication events.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is <see langword="null"/>.</exception>
    public ApiKeyService(
        IApiKeyRepository apiKeyRepository,
        IUserRepository userRepository,
        IOptions<DotnetServiceScaffoldOptions> options,
        ILogger<ApiKeyService> logger)
    {
        ArgumentNullException.ThrowIfNull(apiKeyRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _apiKeyRepository = apiKeyRepository;
        _userRepository = userRepository;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IssuedApiKey> CreateAsync(
        Guid userId,
        string name,
        IEnumerable<string> scopes,
        string? allowedIps = null,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, ApiKeyConstants.NameMaxLength, nameof(name));

        var normalizedScopes = NormalizeScopes(scopes);

        if (await _userRepository.GetByIdAsync(userId, cancellationToken) is null)
            throw new KeyNotFoundException($"User {userId} was not found.");

        var settings = _options.Value;
        var generated = ApiKeyCredentials.Generate(settings.ApiKeyPrefix, settings.ApiKeyLength);

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            KeyHash = generated.SecretHash,
            KeyPrefix = generated.KeyId,
            AllowedScopes = string.Join(ApiKeyServiceConstants.ListSeparator, normalizedScopes),
            AllowedIps = string.IsNullOrWhiteSpace(allowedIps) ? null : allowedIps,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _apiKeyRepository.AddAsync(apiKey, cancellationToken);
        _logger.LogInformation(ApiKeyServiceConstants.LogKeyCreated, apiKey.Id, userId, ApiKeyCredentials.GetLogPrefix(generated.KeyId));

        return new IssuedApiKey(apiKey, generated.PlaintextKey);
    }

    /// <inheritdoc />
    public async Task<ApiKeyValidationResult> ValidateAsync(
        string presentedKey,
        string? sourceIp,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(presentedKey);

        var settings = _options.Value;
        if (!ApiKeyCredentials.TryParse(presentedKey, settings.ApiKeyPrefix, out var keyId, out var secret))
            return Reject(ApiKeyValidationStatus.Malformed, logKeyId: null, sourceIp);

        var apiKey = await _apiKeyRepository.GetByKeyPrefixAsync(keyId);
        if (apiKey is null)
            return Reject(ApiKeyValidationStatus.UnknownKey, keyId, sourceIp);

        var status = apiKey.Evaluate(ApiKeyCredentials.ComputeSecretHash(secret), sourceIp);
        if (status != ApiKeyValidationStatus.Valid)
            return Reject(status, keyId, sourceIp);

        var user = await _userRepository.GetByIdAsync(apiKey.UserId, cancellationToken);
        if (user is null)
            return Reject(ApiKeyValidationStatus.UserNotFound, keyId, sourceIp);

        apiKey.RecordUsage();
        await _apiKeyRepository.UpdateAsync(apiKey, cancellationToken);

        return new ApiKeyValidationResult(ApiKeyValidationStatus.Valid, apiKey, user);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ApiKey>> ListActiveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var keys = await _apiKeyRepository.GetActiveApiKeysForUserAsync(userId);
        return keys.ToList();
    }

    /// <inheritdoc />
    public async Task RevokeAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var apiKey = await GetOwnedKeyAsync(userId, apiKeyId, cancellationToken);

        if (!apiKey.IsActive)
            return;

        apiKey.Revoke();
        await _apiKeyRepository.UpdateAsync(apiKey, cancellationToken);
        _logger.LogInformation(ApiKeyServiceConstants.LogKeyRevoked, apiKey.Id, userId);
    }

    /// <inheritdoc />
    public async Task<IssuedApiKey> RotateAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var existing = await GetOwnedKeyAsync(userId, apiKeyId, cancellationToken);

        if (!existing.IsActive)
            throw new InvalidOperationException(ApiKeyServiceConstants.ErrorRotateRevoked);

        var scopes = existing.AllowedScopes?.Split(ApiKeyServiceConstants.ListSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     ?? [];
        if (scopes.Length == 0)
            throw new InvalidOperationException(ApiKeyServiceConstants.ErrorRotateNoScopes);

        var replacement = await CreateAsync(userId, existing.Name, scopes, existing.AllowedIps, existing.ExpiresAt, cancellationToken);

        existing.Revoke();
        await _apiKeyRepository.UpdateAsync(existing, cancellationToken);
        _logger.LogInformation(ApiKeyServiceConstants.LogKeyRotated, existing.Id, replacement.ApiKey.Id, userId);

        return replacement;
    }

    private async Task<ApiKey> GetOwnedKeyAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);

        // Keys owned by other users are reported as missing so that key identifiers cannot be probed.
        return apiKey is null || apiKey.UserId != userId
            ? throw new KeyNotFoundException(ApiKeyServiceConstants.ErrorKeyNotFound)
            : apiKey;
    }

    private ApiKeyValidationResult Reject(ApiKeyValidationStatus status, string? logKeyId, string? sourceIp)
    {
        _logger.LogWarning(
            ApiKeyServiceConstants.LogValidationFailed,
            status,
            logKeyId is null ? ApiKeyServiceConstants.NoKeyIdLogValue : ApiKeyCredentials.GetLogPrefix(logKeyId),
            sourceIp ?? ApiKeyServiceConstants.UnknownSourceLogValue);

        return new ApiKeyValidationResult(status);
    }

    private static IReadOnlyList<string> NormalizeScopes(IEnumerable<string> scopes)
    {
        var normalized = scopes
            .Where(static scope => !string.IsNullOrWhiteSpace(scope))
            .Select(static scope => scope.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalized.Count == 0)
            throw new ArgumentException(ApiKeyServiceConstants.ErrorNoScopes, nameof(scopes));

        var unknown = normalized.FirstOrDefault(scope => !ApiKeyScopes.All.Contains(scope));
        if (unknown is not null)
            throw new ArgumentException(string.Format(ApiKeyServiceConstants.ErrorUnknownScopeFormat, unknown), nameof(scopes));

        return normalized;
    }
}
