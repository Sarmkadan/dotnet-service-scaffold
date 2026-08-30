#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="UpstreamCluster"/> instances.
/// </summary>
/// <remarks>
/// This static class offers extension methods to validate <see cref="UpstreamCluster"/> objects,
/// ensuring they meet business rules before being used in the service mesh.
/// </remarks>
public static class UpstreamClusterValidation
{
    /// <summary>
    /// Validates an <see cref="UpstreamCluster"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The cluster to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UpstreamCluster? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Name
    if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add(UpstreamClusterValidationConstants.NameCannotBeNullOrWhitespace);
        }

        // Validate Endpoint
    if (string.IsNullOrWhiteSpace(value.Endpoint))
        {
            errors.Add(UpstreamClusterValidationConstants.EndpointCannotBeNullOrWhitespace);
        }

        // Validate HealthyHosts and TotalHosts
        if (value.HealthyHosts < 0)
        {
            errors.Add(UpstreamClusterValidationConstants.HealthyHostsCannotBeNegative);
        }

        if (value.TotalHosts < 0)
        {
            errors.Add(UpstreamClusterValidationConstants.TotalHostsCannotBeNegative);
        }

        else if (value.TotalHosts < value.HealthyHosts)
        {
            errors.Add(UpstreamClusterValidationConstants.TotalHostsCannotBeLessThanHealthyHosts);
        }

        // Validate CircuitBreakerOpen consistency
        if (value.TotalHosts > 0 && value.HealthyHosts == 0 && !value.CircuitBreakerOpen)
        {
            errors.Add(UpstreamClusterValidationConstants.CircuitBreakerOpenShouldBeTrueWhenHostsPresentButNoneHealthy);
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="UpstreamCluster"/> instance is valid.
    /// </summary>
    /// <param name="value">The cluster to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UpstreamCluster? value) =>
        value is not null && Validate(value).Count == 0;

    /// <summary>
    /// Ensures that an <see cref="UpstreamCluster"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The cluster to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, with a detailed message listing all validation errors.</exception>
    public static void EnsureValid(this UpstreamCluster? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"{UpstreamClusterValidationConstants.UpstreamClusterValidationFailedHeader}{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}