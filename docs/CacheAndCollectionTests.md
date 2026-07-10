# CacheAndCollectionTests

The `CacheAndCollectionTests` class contains unit tests that validate the behavior of password strength assessment, numeric range validation, collection batching and partitioning, and an in-memory cache service. Each test method exercises a specific scenario and asserts the expected outcome, including correct return values, exception types, and parameter names.

## API

### `public void IsPasswordStrong_VariousPasswords_ReturnsExpectedStrengthAssessment`

- **Purpose**: Verifies that the password strength assessment method returns the correct strength level for a variety of input passwords, including weak, moderate, and strong examples.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exception; test failures are reported via assertion exceptions if the assessed strength does not match the expected value.

### `public void ValidateRange_ValueAboveUpperBound_ThrowsArgumentExceptionWithParamName`

- **Purpose**: Confirms that a range validation method throws an `ArgumentException` when the supplied value exceeds the defined upper bound, and that the exception’s `ParamName` property matches the expected parameter name.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exception; the test passes only if the expected `ArgumentException` is thrown.

### `public void Batch_TenElementsWithBatchSizeThree_ProducesFourBatchesWithCorrectSizes`

- **Purpose**: Tests that a batch operation on a collection of ten elements with a batch size of three yields four batches, where the first three batches contain three elements each and the last batch contains one element.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exception; assertion failures occur if batch sizes or count are incorrect.

### `public void Partition_IntegerCollection_SeparatesEvenAndOddNumbersCorrectly`

- **Purpose**: Validates that a partition operation correctly splits a collection of integers into two groups: even numbers and odd numbers, preserving the relative order within each group.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exception; test failures indicate incorrect partitioning logic.

### `public async Task InMemoryCacheService_SetThenGetAsync_ReturnsStoredValue`

- **Purpose**: Ensures that a value stored in the in-memory cache via `SetAsync` can be retrieved correctly with `GetAsync`, and that the retrieved value matches the original.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: No direct exception; the test fails if the cached value is missing or does not match.

### `public async Task InMemoryCacheService_RemoveAsync_DeletesEntryFromCache`

- **Purpose**: Verifies that after calling `RemoveAsync` on a previously stored cache entry, subsequent `GetAsync` calls return `null` (or the default value) for that key, confirming the entry has been deleted.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: No direct exception; the test fails if the entry is still present after removal.

## Usage

The following examples demonstrate how the production code tested by `CacheAndCollectionTests` is intended to be used.

```csharp
// Example 1: Batching and partitioning collections
var numbers = Enumerable.Range(1, 10).ToList();

// Batch into groups of three
var batches = numbers.Batch(3);
foreach (var batch in batches)
{
    Console.WriteLine($"Batch size: {batch.Count}");
}

// Partition into evens and odds
var (evens, odds) = numbers.Partition(n => n % 2 == 0);
Console.WriteLine($"Evens: {string.Join(", ", evens)}");
Console.WriteLine($"Odds: {string.Join(", ", odds)}");
```

```csharp
// Example 2: In-memory cache service
var cache = new InMemoryCacheService();

// Store a value
await cache.SetAsync("user:123", new UserProfile { Name = "Alice" });

// Retrieve the value
var profile = await cache.GetAsync<UserProfile>("user:123");
Console.WriteLine(profile.Name); // Output: Alice

// Remove the entry
await cache.RemoveAsync("user:123");
var afterRemoval = await cache.GetAsync<UserProfile>("user:123");
Console.WriteLine(afterRemoval is null); // Output: True
```

## Notes

- **Edge cases**: The batch and partition methods should handle empty collections gracefully (returning zero batches or empty partitions). The password strength assessment must account for null or empty strings, as well as passwords that meet only some criteria. The range validation should correctly reject values exactly at the boundary (e.g., values equal to the upper bound may be considered valid or invalid depending on the implementation).
- **Thread safety**: The `InMemoryCacheService` is intended for single-threaded or sequentially accessed scenarios. Concurrent reads and writes are not synchronized; if used in a multi-threaded context, external locking or a thread-safe cache implementation should be employed. The test methods themselves are not thread-safe and should be run sequentially.
- **Exception behavior**: The `ValidateRange` test relies on the production method throwing an `ArgumentException` with a specific parameter name. If the production code throws a different exception type or omits the parameter name, the test will fail.
