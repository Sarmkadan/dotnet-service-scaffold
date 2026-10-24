#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Generates, parses and hashes API keys. A presented key has the form <c>{prefix}{keyId}.{secret}</c>:
/// the key-id is a public lookup handle and the secret is the credential. Only the SHA-256 digest of the
/// secret is persisted, so a leaked database does not reveal usable keys.
/// </summary>
public static class ApiKeyCredentials
{
    /// <summary>
    /// Generates a new API key. The plaintext key is returned once and must not be stored.
    /// </summary>
    /// <param name="prefix">The configured key prefix, for example <c>sk_live_</c>.</param>
    /// <param name="secretLength">The number of random bytes in the secret.</param>
    /// <returns>The plaintext key, its key-id and the digest to persist.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="prefix"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="secretLength"/> is below the minimum secret size.</exception>
    public static GeneratedApiKey Generate(string prefix, int secretLength)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);
        ArgumentOutOfRangeException.ThrowIfLessThan(secretLength, ApiKeyCredentialsConstants.MinSecretLength);

        var keyId = CreateKeyId();
        var secret = EncryptionUtility.GenerateSecureToken(secretLength);

        return new GeneratedApiKey(
            PlaintextKey: $"{prefix}{keyId}{ApiKeyCredentialsConstants.Separator}{secret}",
            KeyId: keyId,
            SecretHash: ComputeSecretHash(secret));
    }

    /// <summary>
    /// Splits a presented key into its key-id and secret, verifying the prefix and key-id shape.
    /// </summary>
    /// <param name="presentedKey">The key presented by the client.</param>
    /// <param name="prefix">The configured key prefix the key must start with.</param>
    /// <param name="keyId">When this method returns <see langword="true"/>, the key-id.</param>
    /// <param name="secret">When this method returns <see langword="true"/>, the secret.</param>
    /// <returns><see langword="true"/> when the key is well-formed; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="presentedKey"/> or <paramref name="prefix"/> is <see langword="null"/> or empty.</exception>
    public static bool TryParse(string presentedKey, string prefix, out string keyId, out string secret)
    {
        ArgumentException.ThrowIfNullOrEmpty(presentedKey);
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        keyId = string.Empty;
        secret = string.Empty;

        if (!presentedKey.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        var body = presentedKey.AsSpan(prefix.Length);
        var separatorIndex = body.IndexOf(ApiKeyCredentialsConstants.Separator);
        if (separatorIndex != ApiKeyCredentialsConstants.KeyIdLength)
            return false;

        var candidateKeyId = body[..separatorIndex].ToString();
        var candidateSecret = body[(separatorIndex + 1)..].ToString();
        if (candidateSecret.Length == 0 || !IsAlphanumeric(candidateKeyId))
            return false;

        keyId = candidateKeyId;
        secret = candidateSecret;
        return true;
    }

    /// <summary>
    /// Computes the lowercase hexadecimal SHA-256 digest of an API key secret.
    /// </summary>
    /// <param name="secret">The secret portion of the key.</param>
    /// <returns>The hexadecimal digest.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="secret"/> is <see langword="null"/> or empty.</exception>
    public static string ComputeSecretHash(string secret)
    {
        ArgumentException.ThrowIfNullOrEmpty(secret);

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));
    }

    /// <summary>
    /// Returns the portion of a key-id that is safe to write to logs.
    /// </summary>
    /// <param name="keyId">The key-id to truncate.</param>
    /// <returns>The first <see cref="ApiKeyCredentialsConstants.LogPrefixLength"/> characters of the key-id.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="keyId"/> is <see langword="null"/> or empty.</exception>
    public static string GetLogPrefix(string keyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId);

        return keyId.Length <= ApiKeyCredentialsConstants.LogPrefixLength
            ? keyId
            : keyId[..ApiKeyCredentialsConstants.LogPrefixLength];
    }

    private static string CreateKeyId()
    {
        var alphabet = ApiKeyCredentialsConstants.KeyIdAlphabet;
        return string.Create(ApiKeyCredentialsConstants.KeyIdLength, alphabet, static (span, chars) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
        });
    }

    private static bool IsAlphanumeric(string value) => value.All(static c => char.IsAsciiLetterOrDigit(c));
}
