#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Presentation.Authorization;

/// <summary>
/// Authorization policy names that gate endpoints by API key scope.
/// </summary>
public static class ApiKeyPolicies
{
    /// <summary>Requires a <c>read</c>, <c>register</c> or <c>admin</c> scope.</summary>
    public const string Read = "ApiKey.Read";

    /// <summary>Requires a <c>register</c> or <c>admin</c> scope.</summary>
    public const string Register = "ApiKey.Register";

    /// <summary>Requires the <c>admin</c> scope.</summary>
    public const string Admin = "ApiKey.Admin";
}
