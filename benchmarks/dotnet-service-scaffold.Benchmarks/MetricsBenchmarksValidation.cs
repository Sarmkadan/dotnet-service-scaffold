#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="MetricsBenchmarks"/> to ensure benchmark setup is valid
/// before execution. Validates that the benchmark class is properly configured and ready for benchmarking.
/// </summary>
public static class MetricsBenchmarksValidation
{
    /// <summary>
    /// Validates a <see cref="MetricsBenchmarks"/> instance and returns a list of human-readable problems.
    /// Returns an empty list if the instance is valid.
    /// </summary>
    /// <param name="value">The metrics benchmarks instance to validate.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MetricsBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate that the benchmark class is ready for execution
        // The primary validation is that Setup() was called successfully
        // Since we can't access private fields, we validate the public API state

        // Check if the metrics service reference is null (indicates Setup() wasn't called)
        // We use reflection to check the private field since it's the most reliable indicator
        var metricsField = typeof(MetricsBenchmarks).GetField("_metrics",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (metricsField?.GetValue(value) is null)
        {
            errors.Add("MetricsBenchmarks instance is not properly initialized. Setup() method must be called before benchmarking.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MetricsBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The metrics benchmarks instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this MetricsBenchmarks value)
        => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="MetricsBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The metrics benchmarks instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing the validation errors.</exception>
    public static void EnsureValid(this MetricsBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"MetricsBenchmarks instance is not valid. Problems:\n{string.Join("\n", errors)}");
        }
    }
}