#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Enums;

/// <summary>
/// Outcome of validating a presented API key.
/// </summary>
public enum ApiKeyValidationStatus
{
    /// <summary>The key is well-formed, active, unexpired, from an allowed address, and its secret matches.</summary>
    Valid,

    /// <summary>The presented value does not have the expected <c>{prefix}{keyId}.{secret}</c> shape.</summary>
    Malformed,

    /// <summary>No key with the presented key-id exists.</summary>
    UnknownKey,

    /// <summary>The key has been revoked.</summary>
    Inactive,

    /// <summary>The key is past its expiry time.</summary>
    Expired,

    /// <summary>The presented secret does not match the stored hash.</summary>
    SecretMismatch,

    /// <summary>The request came from an address that is not on the key's allow list.</summary>
    IpNotAllowed,

    /// <summary>The key is valid but its owning user no longer exists.</summary>
    UserNotFound
}
