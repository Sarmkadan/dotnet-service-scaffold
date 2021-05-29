// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive", nameof(batchSize));

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
    public static List<List<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be positive", nameof(chunkSize));

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
    public static bool ContainsSameElements<T>(this IEnumerable<T>? first, IEnumerable<T>? second)
    {
        if (first == null && second == null)
            return true;

        if (first == null || second == null)
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
    public static IEnumerable<T> GetCommon<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        return first.Intersect(second);
    }

    /// <summary>
    /// Gets the difference between two collections (items in first but not in second).
    /// </summary>
    public static IEnumerable<T> GetDifference<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        return first.Except(second);
    }

    /// <summary>
    /// Flattens a nested collection into a single sequence.
    /// </summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(x => x);
    }

    /// <summary>
    /// Returns a shuffled copy of the collection using Fisher-Yates algorithm.
    /// </summary>
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
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Checks if a collection has any items (opposite of IsNullOrEmpty).
    /// </summary>
    public static bool HasItems<T>(this IEnumerable<T>? source)
    {
        return source != null && source.Any();
    }

    /// <summary>
    /// Executes an action on each item in the collection.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }

    /// <summary>
    /// Executes an action on each item with its index.
    /// </summary>
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
