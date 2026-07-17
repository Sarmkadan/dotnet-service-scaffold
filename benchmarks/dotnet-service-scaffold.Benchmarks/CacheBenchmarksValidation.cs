#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="CacheBenchmarks"/> instances.
/// Validates that benchmark configuration and state are valid before execution.
/// </summary>
public static class CacheBenchmarksValidation
{
    /// <summary>
    /// Validates that a <see cref="CacheBenchmarks"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this CacheBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate that the cache service is initialized
        if (value.GetCache() is null)
        {
            errors.Add("Cache service is not initialized.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="CacheBenchmarks"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The benchmark instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this CacheBenchmarks? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="CacheBenchmarks"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this CacheBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"The CacheBenchmarks instance is not valid. {string.Join(" ", errors)}",
                nameof(value));
        }
    }
}