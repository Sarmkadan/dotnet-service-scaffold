#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Provides validation helpers for <see cref="UserRepository"/> instances.
/// </summary>
public static class UserRepositoryValidation
{
    /// <summary>
    /// Validates the specified <see cref="UserRepository"/> instance.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UserRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Repository validation - ensure dependencies are not null
        // UserRepository inherits from Repository<T> which has protected internal fields
        if (value._context is null)
        {
            problems.Add("UserRepository._context is null");
        }

        if (value._dbSet is null)
        {
            problems.Add("UserRepository._dbSet is null");
        }

        if (value._logger is null)
        {
            problems.Add("UserRepository._logger is null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UserRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this UserRepository? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="UserRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the repository is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this UserRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "UserRepository is invalid. Problems: " + string.Join("; ", problems),
                nameof(value));
        }
    }
}