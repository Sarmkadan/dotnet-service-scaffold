#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Permission scopes that can be granted to an API key. Scopes are hierarchical:
/// <c>admin</c> implies <c>register</c>, which implies <c>read</c>.
/// </summary>
public static class ApiKeyScopes
{
    /// <summary>Read-only access.</summary>
    public const string Read = "read";

    /// <summary>Read access plus the ability to register and modify resources.</summary>
    public const string Register = "register";

    /// <summary>Full access, including API key management.</summary>
    public const string Admin = "admin";

    /// <summary>All scopes that may be granted to a key.</summary>
    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal) { Read, Register, Admin };

    /// <summary>
    /// Returns the set of granted scopes that satisfy a required scope.
    /// </summary>
    /// <param name="requiredScope">The scope an operation requires.</param>
    /// <returns>The granted scopes that satisfy <paramref name="requiredScope"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="requiredScope"/> is not a known scope.</exception>
    public static IReadOnlySet<string> GetGrantingScopes(string requiredScope)
    {
        ArgumentException.ThrowIfNullOrEmpty(requiredScope);

        return requiredScope switch
        {
            Read => new HashSet<string>(StringComparer.Ordinal) { Read, Register, Admin },
            Register => new HashSet<string>(StringComparer.Ordinal) { Register, Admin },
            Admin => new HashSet<string>(StringComparer.Ordinal) { Admin },
            _ => throw new ArgumentException($"Unknown API key scope '{requiredScope}'.", nameof(requiredScope))
        };
    }
}
