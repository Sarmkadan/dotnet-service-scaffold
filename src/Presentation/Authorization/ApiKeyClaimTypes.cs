#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Presentation.Authorization;

/// <summary>
/// Claim types that the API key authentication handler adds to the principal.
/// </summary>
public static class ApiKeyClaimTypes
{
    /// <summary>Claim carrying one granted scope per claim instance.</summary>
    public const string Scope = "api_key_scope";

    /// <summary>Claim carrying the identifier of the API key that authenticated the request.</summary>
    public const string ApiKeyId = "api_key_id";
}
