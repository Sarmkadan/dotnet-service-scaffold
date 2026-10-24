#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Request body for issuing a new API key.
/// </summary>
/// <param name="Name">A human-readable label for the key.</param>
/// <param name="Scopes">The scopes to grant: <c>read</c>, <c>register</c> or <c>admin</c>.</param>
/// <param name="AllowedIps">An optional comma-separated allow list of source addresses.</param>
/// <param name="ExpiresAt">An optional UTC expiry time.</param>
public sealed record CreateApiKeyRequest(string Name, IReadOnlyList<string> Scopes, string? AllowedIps = null, DateTime? ExpiresAt = null);
