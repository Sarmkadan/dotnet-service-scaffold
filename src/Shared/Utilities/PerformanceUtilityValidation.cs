#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Provides validation helpers for performance measurement data.
/// Validates memory statistics and garbage collection statistics for correctness.
/// </summary>
public static class PerformanceUtilityValidation
{
    /// <summary>
    /// Validates a <see cref="MemoryStats"/> instance.
    /// </summary>
    /// <param name="stats">The memory stats to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MemoryStats stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        var errors = new List<string>();

        if (stats.WorkingSetMb < 0)
            errors.Add($"WorkingSetMb{PerformanceUtilityValidationConstants.MustBeNonNegativeGot}{stats.WorkingSetMb}{PerformanceUtilityValidationConstants.MbSuffix}");

        if (stats.PrivateMemoryMb < 0)
            errors.Add($"PrivateMemoryMb{PerformanceUtilityValidationConstants.MustBeNonNegativeGot}{stats.PrivateMemoryMb}{PerformanceUtilityValidationConstants.MbSuffix}");

        if (stats.PeakWorkingSetMb < 0)
            errors.Add($"PeakWorkingSetMb{PerformanceUtilityValidationConstants.MustBeNonNegativeGot}{stats.PeakWorkingSetMb}{PerformanceUtilityValidationConstants.MbSuffix}");

        if (stats.PeakWorkingSetMb < stats.WorkingSetMb)
            errors.Add($"PeakWorkingSetMb ({stats.PeakWorkingSetMb}{PerformanceUtilityValidationConstants.MbSuffix}){PerformanceUtilityValidationConstants.CannotBeLessThan}WorkingSetMb ({stats.WorkingSetMb}{PerformanceUtilityValidationConstants.MbSuffix})");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="GarbageCollectionStats"/> instance.
    /// </summary>
    /// <param name="stats">The GC stats to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this GarbageCollectionStats stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        var errors = new List<string>();

        if (stats.Gen0Collections < 0)
            errors.Add($"Gen0Collections{PerformanceUtilityValidationConstants.MustBeNonNegativeGot}{stats.Gen0Collections}");

        if (stats.Gen1Collections < 0)
            errors.Add($"Gen1Collections{PerformanceUtilityValidationConstants.MustBeNonNegativeGot}{stats.Gen1Collections}");

        if (stats.Gen2Collections < 0)
            errors.Add($"Gen2Collections{PerformanceUtilityValidationConstants.MustBeNonNegativeGot}{stats.Gen2Collections}");

        if (stats.TotalMemoryBytes < 0)
            errors.Add($"TotalMemoryBytes{PerformanceUtilityValidationConstants.MustBeNonNegativeGot}{stats.TotalMemoryBytes}{PerformanceUtilityValidationConstants.BytesSuffix}");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MemoryStats"/> instance is valid.
    /// </summary>
    /// <param name="stats">The memory stats to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
    public static bool IsValid(this MemoryStats stats)
    {
        ArgumentNullException.ThrowIfNull(stats);
        return Validate(stats).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="MemoryStats"/> instance is valid.
    /// </summary>
    /// <param name="stats">The memory stats to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="stats"/> is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this MemoryStats stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        var errors = Validate(stats);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"MemoryStats instance is not valid.{PerformanceUtilityValidationConstants.ValidationHeaderPrefix}{string.Join(PerformanceUtilityValidationConstants.ValidationErrorsSeparator, errors)}");
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="GarbageCollectionStats"/> instance is valid.
    /// </summary>
    /// <param name="stats">The GC stats to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
    public static bool IsValid(this GarbageCollectionStats stats)
    {
        ArgumentNullException.ThrowIfNull(stats);
        return Validate(stats).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="GarbageCollectionStats"/> instance is valid.
    /// </summary>
    /// <param name="stats">The GC stats to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="stats"/> is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this GarbageCollectionStats stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        var errors = Validate(stats);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"GarbageCollectionStats instance is not valid.{PerformanceUtilityValidationConstants.ValidationHeaderPrefix}{string.Join(PerformanceUtilityValidationConstants.ValidationErrorsSeparator, errors)}");
        }
    }
}