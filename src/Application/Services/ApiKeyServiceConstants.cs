#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Constants used by <see cref="ApiKeyService"/>.
/// </summary>
internal static class ApiKeyServiceConstants
{
    // Storage format
    public const string ListSeparator = ",";

    // Log messages (never include a secret or a full key-id)
    public const string LogKeyCreated = "API key {ApiKeyId} created for user {UserId} (key-id {KeyIdPrefix})";
    public const string LogKeyRevoked = "API key {ApiKeyId} revoked for user {UserId}";
    public const string LogKeyRotated = "API key {ReplacedApiKeyId} rotated to {ReplacementApiKeyId} for user {UserId}";
    public const string LogValidationFailed = "API key authentication failed: {Reason} (key-id {KeyIdPrefix}, source {SourceIp})";
    public const string NoKeyIdLogValue = "none";
    public const string UnknownSourceLogValue = "unknown";

    // Error messages
    public const string ErrorNoScopes = "At least one scope must be specified.";
    public const string ErrorUnknownScopeFormat = "Unknown API key scope '{0}'.";
    public const string ErrorKeyNotFound = "API key was not found.";
    public const string ErrorRotateRevoked = "A revoked API key cannot be rotated.";
    public const string ErrorRotateNoScopes = "The API key has no scopes and cannot be rotated.";
}
