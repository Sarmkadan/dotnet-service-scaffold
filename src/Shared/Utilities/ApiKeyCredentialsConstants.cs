#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Constants used by <see cref="ApiKeyCredentials"/>.
/// </summary>
public static class ApiKeyCredentialsConstants
{
    /// <summary>Number of characters in a generated key-id.</summary>
    public const int KeyIdLength = 12;

    /// <summary>Characters a generated key-id is drawn from.</summary>
    public const string KeyIdAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>Separator between the key-id and the secret. Base64url secrets never contain it.</summary>
    public const char Separator = '.';

    /// <summary>Minimum number of random bytes a secret may have.</summary>
    public const int MinSecretLength = 16;

    /// <summary>Number of key-id characters written to logs.</summary>
    public const int LogPrefixLength = 8;
}
