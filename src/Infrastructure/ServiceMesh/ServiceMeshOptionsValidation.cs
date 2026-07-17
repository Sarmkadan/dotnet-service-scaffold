#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using System.Linq;

namespace DotnetServiceScaffold.Infrastructure.ServiceMesh;

/// <summary>
/// Validation helpers for <see cref="ServiceMeshOptions"/> configuration.
/// Provides methods to validate service mesh configuration options and ensure
/// they meet expected constraints before use.
/// </summary>
public static class ServiceMeshOptionsValidation
{
    /// <summary>
    /// Validates the service mesh configuration options and returns a list of
    /// human-readable validation problems. Returns an empty list if the options
    /// are valid.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceMeshOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate AdminEndpoint
        if (string.IsNullOrWhiteSpace(value.AdminEndpoint))
        {
            problems.Add("ServiceMesh.AdminEndpoint must be a non-empty URL string.");
        }
        else if (!Uri.TryCreate(value.AdminEndpoint, UriKind.Absolute, out var uriResult))
        {
            problems.Add("ServiceMesh.AdminEndpoint must be a valid absolute URL (e.g., http://localhost:15000).");
        }
        else if (uriResult.Scheme is not ("http" or "https"))
        {
            problems.Add("ServiceMesh.AdminEndpoint must use http:// or https:// scheme.");
        }
        else if (uriResult.AbsolutePath != "/" && uriResult.AbsolutePath.EndsWith("/"))
        {
            problems.Add("ServiceMesh.AdminEndpoint must not end with a trailing slash.");
        }

        // Validate ReadinessTimeoutSeconds
        if (value.ReadinessTimeoutSeconds <= 0)
        {
            problems.Add("ServiceMesh.ReadinessTimeoutSeconds must be a positive integer greater than zero.");
        }
        else if (value.ReadinessTimeoutSeconds > 60)
        {
            problems.Add("ServiceMesh.ReadinessTimeoutSeconds should not exceed 60 seconds to avoid blocking application startup.");
        }

        // Validate MeshName
        if (string.IsNullOrWhiteSpace(value.MeshName))
        {
            problems.Add("ServiceMesh.MeshName must be a non-empty string identifying the mesh environment.");
        }
        else if (value.MeshName.Length > 50)
        {
            problems.Add("ServiceMesh.MeshName must be 50 characters or less.");
        }
        else if (value.MeshName.Any(c => char.IsWhiteSpace(c) && c != '-'))
        {
            problems.Add("ServiceMesh.MeshName must not contain whitespace characters other than hyphens.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the service mesh configuration options are valid.
    /// </summary>
    /// <param name="value">The options to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ServiceMeshOptions value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the service mesh configuration options are valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing
    /// all validation problems if any are found.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the options are invalid, containing a list of problems.</exception>
    public static void EnsureValid(this ServiceMeshOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ServiceMeshOptions validation failed:{Environment.NewLine}- {
                string.Join(Environment.NewLine + "- ", problems)}",
            nameof(value));
    }
}