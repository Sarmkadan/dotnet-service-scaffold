# CollectionUtility
The `CollectionUtility` class provides a set of static methods for working with collections in C#. It offers various utility functions for tasks such as batching, chunking, shuffling, and more, making it easier to perform common operations on collections. These methods are designed to be reusable and efficient, allowing developers to focus on the logic of their application rather than implementing these utility functions themselves.

## API
* `Batch<T>(IEnumerable<T> source, int batchSize)`: Returns an enumerable of batches of the specified size from the source collection. The `batchSize` parameter determines the size of each batch. This method does not throw any exceptions.
* `Chunk<T>(IEnumerable<T> source, int chunkSize)`: Returns a list of chunks of the specified size from the source collection. The `chunkSize` parameter determines the size of each chunk. This method does not throw any exceptions.
* `ContainsSameElements<T>(IEnumerable<T> first, IEnumerable<T> second)`: Returns a boolean indicating whether the two collections contain the same elements, regardless of order. This method does not throw any exceptions.
* `GetCommon<T>(IEnumerable<T> first, IEnumerable<T> second)`: Returns an enumerable of elements common to both collections. This method does not throw any exceptions.
* `GetDifference<T>(IEnumerable<T> first, IEnumerable<T> second)`: Returns an enumerable of elements in the first collection that are not in the second collection. This method does not throw any exceptions.
* `Flatten<T>(IEnumerable<IEnumerable<T>> source)`: Returns an enumerable of all elements in the nested collections. This method does not throw any exceptions.
* `Shuffle<T>(List<T> list)`: Returns a shuffled version of the input list. This method does not throw any exceptions.
* `DistinctPreservingOrder<T>(IEnumerable<T> source)`: Returns an enumerable of distinct elements from the source collection, preserving the original order. This method does not throw any exceptions.
* `GroupByToDictionary<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)`: Returns a dictionary where the keys are the result of the key selector function and the values are lists of elements that correspond to each key. This method does not throw any exceptions.
* `Partition<T>(IEnumerable<T> source, Func<T, bool> predicate)`: Returns a tuple containing two lists: one with elements that match the predicate and one with elements that do not match. This method does not throw any exceptions.
* `IsNullOrEmpty<T>(IEnumerable<T> source)`: Returns a boolean indicating whether the collection is null or empty. This method does not throw any exceptions.
* `HasItems<T>(IEnumerable<T> source)`: Returns a boolean indicating whether the collection has any items. This method does not throw any exceptions.
* `ForEach<T>(IEnumerable<T> source, Action<T> action)`: Performs the specified action on each element in the collection. This method does not throw any exceptions.
* `ForEach<T>(IEnumerable<T> source, Action<T, int> action)`: Performs the specified action on each element in the collection, providing the index of each element. This method does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `CollectionUtility` class:
```csharp
// Example 1: Batching and chunking
var numbers = Enumerable.Range(1, 10);
var batches = CollectionUtility.Batch(numbers, 3);
var chunks = CollectionUtility.Chunk(numbers, 3);

foreach (var batch in batches)
{
    Console.WriteLine(string.Join(", ", batch));
}

foreach (var chunk in chunks)
{
    Console.WriteLine(string.Join(", ", chunk));
}

// Example 2: Shuffling and distinct elements
var colors = new List<string> { "Red", "Green", "Blue", "Red", "Green" };
var shuffledColors = CollectionUtility.Shuffle(colors);
var distinctColors = CollectionUtility.DistinctPreservingOrder(colors);

Console.WriteLine(string.Join(", ", shuffledColors));
Console.WriteLine(string.Join(", ", distinctColors));
```

## Notes
When using the `CollectionUtility` class, keep in mind the following edge cases and thread-safety considerations:
* The `Batch` and `Chunk` methods will return an empty enumerable or list if the input collection is empty.
* The `ContainsSameElements` method uses the default equality comparer for the type `T`, which may not be suitable for all types (e.g., custom classes).
* The `GetCommon` and `GetDifference` methods use the default equality comparer for the type `T`.
* The `Flatten` method will throw an `ArgumentNullException` if the input collection is null.
* The `Shuffle` method modifies the original list and returns it.
* The `DistinctPreservingOrder` method uses the default equality comparer for the type `T`.
* The `GroupByToDictionary` method uses the default equality comparer for the type `TKey`.
* The `Partition` method uses the provided predicate function to determine which elements match and which do not.
* The `IsNullOrEmpty` and `HasItems` methods are thread-safe, as they only access the collection's `Count` property.
* The `ForEach` methods are not thread-safe, as they modify the collection or perform actions that may have side effects.
