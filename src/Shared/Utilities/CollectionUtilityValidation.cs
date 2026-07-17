#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Provides validation helpers for collection operations.
/// Validates parameters and constraints for <see cref="CollectionUtility"/> operations to ensure correct usage.
/// </summary>
/// <remarks>
/// This class contains validation methods that check collection parameters before they are passed to
/// corresponding methods in the <see cref="CollectionUtility"/> class. All validation methods throw
/// <see cref="ArgumentNullException"/> for null inputs and return validation error messages for invalid inputs.
/// </remarks>
public static class CollectionUtilityValidation
{
    /// <summary>
    /// Validates collection operation parameters before they are used with <see cref="CollectionUtility"/> methods.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="batchSize">The batch size to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate<T>(
        IEnumerable<T>? source,
        int batchSize = 1)
    {
        var problems = new List<string>();

        ArgumentNullException.ThrowIfNull(source);

        if (!source.Any())
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
    /// Validates collection operation parameters before they are used with <see cref="CollectionUtility"/> methods.
    /// Returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="chunkSize">The chunk size to validate.</param>
    /// <param name="isChunkValidation">Whether this is chunk validation.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate<T>(
        IEnumerable<T>? source,
        int chunkSize,
        bool isChunkValidation)
    {
        var problems = new List<string>();

        ArgumentNullException.ThrowIfNull(source);

        if (!source.Any())
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
    /// Validates collection operation parameters before they are used with <see cref="CollectionUtility"/> methods.
    /// </summary>
    /// <param name="first">The first collection to validate.</param>
    /// <param name="second">The second collection to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="first"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="second"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate<T>(
        IEnumerable<T>? first,
        IEnumerable<T>? second)
        where T : notnull
    {
        var problems = new List<string>();

        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        if (!first.Any())
        {
            problems.Add("First collection is empty.");
        }

        if (!second.Any())
        {
            problems.Add("Second collection is empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates collection operation parameters before they are used with <see cref="CollectionUtility"/> methods.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="predicate">The predicate function to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate<T>(
        IEnumerable<T>? source,
        Func<T, bool>? predicate)
    {
        var problems = new List<string>();

        ArgumentNullException.ThrowIfNull(source);

        if (!source.Any())
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
    /// Validates collection operation parameters before they are used with <see cref="CollectionUtility"/> methods.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="keySelector">The key selector function to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate<T, TKey>(
        IEnumerable<T>? source,
        Func<T, TKey>? keySelector)
        where TKey : notnull
    {
        var problems = new List<string>();

        ArgumentNullException.ThrowIfNull(source);

        if (!source.Any())
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
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static bool IsValid<T>(
        IEnumerable<T>? source,
        int batchSize = 1)
    {
        ArgumentNullException.ThrowIfNull(source);
        return !Validate(source, batchSize).Any();
    }

    /// <summary>
    /// Checks if collection operation parameters are valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="chunkSize">The chunk size to validate.</param>
    /// <param name="isChunkValidation">Whether this is chunk validation.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static bool IsValid<T>(
        IEnumerable<T>? source,
        int chunkSize,
        bool isChunkValidation)
    {
        ArgumentNullException.ThrowIfNull(source);
        return !Validate(source, chunkSize, isChunkValidation).Any();
    }

    /// <summary>
    /// Checks if collection operation parameters are valid.
    /// </summary>
    /// <param name="first">The first collection to validate.</param>
    /// <param name="second">The second collection to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="first"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="second"/> is <see langword="null"/>.</exception>
    public static bool IsValid<T>(
        IEnumerable<T>? first,
        IEnumerable<T>? second) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        return !Validate(first, second).Any();
    }

    /// <summary>
    /// Checks if collection operation parameters are valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="predicate">The predicate function to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static bool IsValid<T>(
        IEnumerable<T>? source,
        Func<T, bool>? predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        return !Validate(source, predicate).Any();
    }

    /// <summary>
    /// Checks if collection operation parameters are valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="keySelector">The key selector function to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static bool IsValid<T, TKey>(
        IEnumerable<T>? source,
        Func<T, TKey>? keySelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return !Validate(source, keySelector).Any();
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="batchSize">The batch size to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static void EnsureValid<T>(
        IEnumerable<T>? source,
        int batchSize = 1)
    {
        var problems = Validate(source, batchSize);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="chunkSize">The chunk size to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static void EnsureValid<T>(
        IEnumerable<T>? source,
        int chunkSize,
        bool isChunkValidation)
    {
        var problems = Validate(source, chunkSize, isChunkValidation);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="first">The first collection to validate.</param>
    /// <param name="second">The second collection to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="first"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="second"/> is <see langword="null"/>.</exception>
    public static void EnsureValid<T>(
        IEnumerable<T>? first,
        IEnumerable<T>? second)
        where T : notnull
    {
        var problems = Validate(first, second);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="predicate">The predicate function to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static void EnsureValid<T>(
        IEnumerable<T>? source,
        Func<T, bool>? predicate)
    {
        var problems = Validate(source, predicate);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures that collection operation parameters are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if not valid.
    /// </summary>
    /// <param name="source">The source collection to validate.</param>
    /// <param name="keySelector">The key selector function to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with detailed error messages.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static void EnsureValid<T, TKey>(
        IEnumerable<T>? source,
        Func<T, TKey>? keySelector)
        where TKey : notnull
    {
        var problems = Validate(source, keySelector);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection operation validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }
}