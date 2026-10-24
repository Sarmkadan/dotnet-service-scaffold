#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// A newly generated API key.
/// </summary>
/// <param name="PlaintextKey">The full key to hand to the client. It is shown once and never stored.</param>
/// <param name="KeyId">The public key-id, stored for lookup.</param>
/// <param name="SecretHash">The SHA-256 digest of the secret, stored for verification.</param>
public readonly record struct GeneratedApiKey(string PlaintextKey, string KeyId, string SecretHash);
