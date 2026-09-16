#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Runtime.CompilerServices;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Utility class for collection operations. Provides helpers for batch processing,
/// grouping, and common collection manipulations.
/// </summary>
public static class CollectionUtility
{
    /// <summary>
    /// Splits a collection into batches of a specified size.
    /// Useful for processing large collections in chunks.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection to split into batches.</param>
    /// <param name="batchSize">The maximum number of elements in each batch.</param>
    /// <returns>A sequence of batches containing the elements from <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="batchSize"/> is less than or equal to zero.</exception>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(source); // Fix: handle null source collection
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Batch size must be positive.");

        var batch = new List<T>(batchSize);

        foreach (var item in source)
        {
            batch.Add(item);

            if (batch.Count == batchSize)
            {
                yield return batch.ToList();
                batch.Clear();
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }

    /// <summary>
    /// Chunks a collection into groups. Similar to Batch but returns the actual lists.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection to split into chunks.</param>
    /// <param name="chunkSize">The maximum number of elements in each chunk.</param>
    /// <returns>A list of chunks containing the elements from <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="chunkSize"/> is less than or equal to zero.</exception>
    public static List<List<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        ArgumentNullException.ThrowIfNull(source); // Fix: handle null source collection
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), chunkSize, "Chunk size must be positive.");

        var result = new List<List<T>>();
        var chunk = new List<T>(chunkSize);

        foreach (var item in source)
        {
            chunk.Add(item);

            if (chunk.Count == chunkSize)
            {
                result.Add(new List<T>(chunk));
                chunk.Clear();
            }
        }

        if (chunk.Count > 0)
            result.Add(chunk);

        return result;
    }

    /// <summary>
    /// Checks if two collections have the same elements (order-independent).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collections.</typeparam>
    /// <param name="first">The first collection to compare.</param>
    /// <param name="second">The second collection to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both collections are <see langword="null"/> or contain the same elements
    /// with the same multiplicities; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ContainsSameElements<T>(this IEnumerable<T>? first, IEnumerable<T>? second) where T : notnull
    {
        if (first is null && second is null)
            return true;

        if (first is null || second is null)
            return false;

        var firstList = first.ToList();
        var secondList = second.ToList();

        if (firstList.Count != secondList.Count)
            return false;

        var firstGrouped = firstList.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        var secondGrouped = secondList.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        if (firstGrouped.Count != secondGrouped.Count)
            return false;

        foreach (var kvp in firstGrouped)
        {
            if (!secondGrouped.ContainsKey(kvp.Key) || secondGrouped[kvp.Key] != kvp.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the intersection of two collections.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collections.</typeparam>
    /// <param name="first">The first collection.</param>
    /// <param name="second">The collection whose elements are compared with those in <paramref name="first"/>.</param>
    /// <returns>A sequence containing the distinct elements common to both collections.</returns>
    public static IEnumerable<T> GetCommon<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        return first.Intersect(second);
    }

    /// <summary>
    /// Gets the difference between two collections (items in first but not in second).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collections.</typeparam>
    /// <param name="first">The collection whose distinct elements are returned.</param>
    /// <param name="second">The collection whose elements are excluded from the result.</param>
    /// <returns>A sequence containing the distinct elements in <paramref name="first"/> that are not in <paramref name="second"/>.</returns>
    public static IEnumerable<T> GetDifference<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        return first.Except(second);
    }

    /// <summary>
    /// Flattens a nested collection into a single sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the nested collections.</typeparam>
    /// <param name="source">The sequence of collections to flatten.</param>
    /// <returns>A sequence containing the elements of each nested collection.</returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(x => x);
    }

    /// <summary>
    /// Returns a shuffled copy of the collection using Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection to shuffle.</param>
    /// <returns>A new list containing the elements of <paramref name="source"/> in a random order.</returns>
    public static List<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        var random = new Random();

        for (int i = list.Count - 1; i > 0; i--)
        {
            var randomIndex = random.Next(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }

        return list;
    }

    /// <summary>
    /// Removes duplicates while preserving order.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection from which to remove duplicate elements.</param>
    /// <returns>A sequence containing the first occurrence of each distinct element.</returns>
    public static IEnumerable<T> DistinctPreservingOrder<T>(this IEnumerable<T> source)
    {
        var seen = new HashSet<T>();

        foreach (var item in source)
        {
            if (seen.Add(item))
                yield return item;
        }
    }

    /// <summary>
    /// Groups a collection by a key and returns as a dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key produced by <paramref name="keySelector"/>.</typeparam>
    /// <param name="source">The collection to group.</param>
    /// <param name="keySelector">A function that extracts a grouping key from each element.</param>
    /// <returns>A dictionary that maps each key to the elements with that key.</returns>
    public static Dictionary<TKey, List<T>> GroupByToDictionary<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        var result = new Dictionary<TKey, List<T>>();

        foreach (var item in source)
        {
            var key = keySelector(item);

            if (!result.ContainsKey(key))
                result[key] = new List<T>();

            result[key].Add(item);
        }

        return result;
    }

    /// <summary>
    /// Splits a collection based on a predicate.
    /// Returns a tuple of (matching, notMatching).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection to partition.</param>
    /// <param name="predicate">A function that determines which partition receives each element.</param>
    /// <returns>
    /// A tuple containing elements that satisfy <paramref name="predicate"/> and elements that do not,
    /// in their original order.
    /// </returns>
    public static (List<T> Matching, List<T> NotMatching) Partition<T>(
        this IEnumerable<T> source,
        Func<T, bool> predicate)
    {
        var matching = new List<T>();
        var notMatching = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
                matching.Add(item);
            else
                notMatching.Add(item);
        }

        return (matching, notMatching);
    }

    /// <summary>
    /// Checks if a collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection to inspect.</param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is <see langword="null"/> or contains no elements; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source is null || !source.Any();
    }

    /// <summary>
    /// Checks if a collection has any items (opposite of IsNullOrEmpty).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection to inspect.</param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is not <see langword="null"/> and contains at least one element; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasItems<T>(this IEnumerable<T>? source)
    {
        return source is not null && source.Any();
    }

    /// <summary>
    /// Executes an action on each item in the collection.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection whose elements are passed to <paramref name="action"/>.</param>
    /// <param name="action">The action to execute for each element.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }

    /// <summary>
    /// Executes an action on each item with its index.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection whose elements are passed to <paramref name="action"/>.</param>
    /// <param name="action">The action to execute for each element and its zero-based index.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        var index = 0;
        foreach (var item in source)
        {
            action(item, index);
            index++;
        }
    }
}
