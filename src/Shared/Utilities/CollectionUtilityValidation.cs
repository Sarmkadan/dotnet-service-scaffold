#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Provides validation helpers for collection operations.
/// Validates parameters and constraints for <see cref="CollectionUtility"/> operations to ensure correct usage.
/// </summary>
public static class CollectionUtilityValidation
{
    /// <summary>
    /// Validates collection operation parameters before they are used with CollectionUtility methods.
    /// Returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="batchSize">The batch size to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    public static IReadOnlyList<string> Validate<T>(
        IEnumerable<T>? source,
        int batchSize = 1)
    {
        var problems = new List<string>();

        if (source is null)
        {
            problems.Add("Source collection cannot be null.");
        }
        else if (!source.Any())
        {
            problems.Add("Source collection is empty.");
        }

        if (batchSize <= 0)
        {
            problems.Add("Batch size must be a positive integer.");
        }

        if (batchSize > 1_000_000)
        {
            problems.Add("Batch size is excessively large (maximum 1,000,000).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates collection operation parameters before they are used with CollectionUtility methods.
    /// Returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="chunkSize">The chunk size to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    public static IReadOnlyList<string> Validate<T>(
        IEnumerable<T>? source,
        int chunkSize,
        bool isChunkValidation)
    {
        var problems = new List<string>();

        if (source is null)
        {
            problems.Add("Source collection cannot be null.");
        }
        else if (!source.Any())
        {
            problems.Add("Source collection is empty.");
        }

        if (chunkSize <= 0)
        {
            problems.Add("Chunk size must be a positive integer.");
        }

        if (chunkSize > 1_000_000)
        {
            problems.Add("Chunk size is excessively large (maximum 1,000,000).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates collection operation parameters before they are used with CollectionUtility methods.
    /// Returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="first">The first collection to validate.</param>
    /// <param name="second">The second collection to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    public static IReadOnlyList<string> Validate<T>(
        IEnumerable<T>? first,
        IEnumerable<T>? second)
        where T : notnull
    {
        var problems = new List<string>();

        if (first is null && second is null)
        {
            problems.Add("Both collections cannot be null.");
        }
        else if (first is null)
        {
            problems.Add("First collection cannot be null.");
        }
        else if (!first.Any())
        {
            problems.Add("First collection is empty.");
        }

        if (second is null)
        {
            problems.Add("Second collection cannot be null.");
        }
        else if (!second.Any())
        {
            problems.Add("Second collection is empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates collection operation parameters before they are used with CollectionUtility methods.
    /// Returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="predicate">The predicate function to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    public static IReadOnlyList<string> Validate<T>(
        IEnumerable<T>? source,
        Func<T, bool>? predicate)
    {
        var problems = new List<string>();

        if (source is null)
        {
            problems.Add("Source collection cannot be null.");
        }
        else if (!source.Any())
        {
            problems.Add("Source collection is empty.");
        }

        if (predicate is null)
        {
            problems.Add("Predicate function cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates collection operation parameters before they are used with CollectionUtility methods.
    /// Returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="keySelector">The key selector function to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    public static IReadOnlyList<string> Validate<T, TKey>(
        IEnumerable<T>? source,
        Func<T, TKey>? keySelector)
        where TKey : notnull
    {
        var problems = new List<string>();

        if (source is null)
        {
            problems.Add("Source collection cannot be null.");
        }
        else if (!source.Any())
        {
            problems.Add("Source collection is empty.");
        }

        if (keySelector is null)
        {
            problems.Add("Key selector function cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if collection operation parameters are valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="batchSize">The batch size to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid<T>(
        IEnumerable<T>? source,
        int batchSize = 1)
    {
        try
        {
            var problems = Validate(source, batchSize);
            return problems.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if collection operation parameters are valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="chunkSize">The chunk size to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid<T>(
        IEnumerable<T>? source,
        int chunkSize,
        bool isChunkValidation)
    {
        try
        {
            var problems = Validate(source, chunkSize, isChunkValidation);
            return problems.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if collection operation parameters are valid.
    /// </summary>
    /// <param name="first">The first collection to validate.</param>
    /// <param name="second">The second collection to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid<T>(
        IEnumerable<T>? first,
        IEnumerable<T>? second)
        where T : notnull
    {
        try
        {
            var problems = Validate(first, second);
            return problems.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if collection operation parameters are valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="predicate">The predicate function to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid<T>(
        IEnumerable<T>? source,
        Func<T, bool>? predicate)
    {
        try
        {
            var problems = Validate(source, predicate);
            return problems.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if collection operation parameters are valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="keySelector">The key selector function to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid<T, TKey>(
        IEnumerable<T>? source,
        Func<T, TKey>? keySelector)
        where TKey : notnull
    {
        try
        {
            var problems = Validate(source, keySelector);
            return problems.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="batchSize">The batch size to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    public static void EnsureValid<T>(
        IEnumerable<T>? source,
        int batchSize = 1)
    {
        var problems = Validate(source, batchSize);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="chunkSize">The chunk size to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    public static void EnsureValid<T>(
        IEnumerable<T>? source,
        int chunkSize,
        bool isChunkValidation)
    {
        var problems = Validate(source, chunkSize, isChunkValidation);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="first">The first collection to validate.</param>
    /// <param name="second">The second collection to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    public static void EnsureValid<T>(
        IEnumerable<T>? first,
        IEnumerable<T>? second)
        where T : notnull
    {
        var problems = Validate(first, second);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="predicate">The predicate function to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    public static void EnsureValid<T>(
        IEnumerable<T>? source,
        Func<T, bool>? predicate)
    {
        var problems = Validate(source, predicate);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="keySelector">The key selector function to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    public static void EnsureValid<T, TKey>(
        IEnumerable<T>? source,
        Func<T, TKey>? keySelector)
        where TKey : notnull
    {
        var problems = Validate(source, keySelector);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}