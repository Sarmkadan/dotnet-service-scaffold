# CollectionUtilityTests

`CollectionUtilityTests` is a unit test class that verifies the behavior of the `CollectionUtility` helper methods. It contains test methods covering null-or-empty checks, safe index-based retrieval with fallback defaults, and pagination logic for partitioning collections into fixed-size pages.

## API

### IsNullOrEmpty_ShouldReturnTrueForNullCollection
- **Purpose**: Confirms that `IsNullOrEmpty` returns `true` when the input collection is `null`.
- **Parameters**: None (test method).
- **Return value**: `void`.
- **Exceptions**: None expected; the test asserts the method under test handles `null` without throwing.

### IsNullOrEmpty_ShouldReturnTrueForEmptyCollection
- **Purpose**: Confirms that `IsNullOrEmpty` returns `true` when the input collection is non-null but contains zero elements.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: None expected.

### IsNullOrEmpty_ShouldReturnFalseForNonEmptyCollection
- **Purpose**: Confirms that `IsNullOrEmpty` returns `false` when the input collection contains at least one element.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: None expected.

### GetOrDefault_ShouldReturnElementIfIndexIsValid
- **Purpose**: Verifies that `GetOrDefault` returns the element at the specified index when the index is within the valid range of the collection.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: None expected.

### GetOrDefault_ShouldReturnDefaultValueIfIndexIsNegative
- **Purpose**: Verifies that `GetOrDefault` returns the default value for the element type when a negative index is supplied.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: None expected; the method under test must not throw on negative indices.

### GetOrDefault_ShouldReturnDefaultValueIfIndexIsOutOfRange
- **Purpose**: Verifies that `GetOrDefault` returns the default value for the element type when the index equals or exceeds the collection’s count.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: None expected.

### Paginate_ShouldReturnCorrectPage
- **Purpose**: Validates that `Paginate` returns the expected subset of elements for a given page number and page size when the requested page exists.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: None expected.

### Paginate_ShouldReturnEmptyListForOutOfRangePageNumber
- **Purpose**: Validates that `Paginate` returns an empty list when the requested page number exceeds the total number of available pages.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: None expected.

## Usage

```csharp
// Testing IsNullOrEmpty with various collection states
[Test]
public void ValidateNullOrEmptyBehavior()
{
    var tests = new CollectionUtilityTests();

    tests.IsNullOrEmpty_ShouldReturnTrueForNullCollection();
    tests.IsNullOrEmpty_ShouldReturnTrueForEmptyCollection();
    tests.IsNullOrEmpty_ShouldReturnFalseForNonEmptyCollection();
}
```

```csharp
// Testing GetOrDefault and Paginate edge cases
[Test]
public void ValidateSafeAccessAndPagination()
{
    var tests = new CollectionUtilityTests();

    // Safe index access
    tests.GetOrDefault_ShouldReturnElementIfIndexIsValid();
    tests.GetOrDefault_ShouldReturnDefaultValueIfIndexIsNegative();
    tests.GetOrDefault_ShouldReturnDefaultValueIfIndexIsOutOfRange();

    // Pagination boundaries
    tests.Paginate_ShouldReturnCorrectPage();
    tests.Paginate_ShouldReturnEmptyListForOutOfRangePageNumber();
}
```

## Notes

- **Edge cases**: The `GetOrDefault` tests explicitly cover negative indices and indices beyond the collection’s upper bound. `Paginate` tests ensure that requesting a page beyond the last available page yields an empty result rather than throwing.
- **Thread safety**: These are unit test methods with no shared mutable state; they are safe to execute concurrently when run by a test framework that isolates test instances. The methods under test (`IsNullOrEmpty`, `GetOrDefault`, `Paginate`) should be evaluated separately for thread safety based on their own implementations and the collections passed to them.
- **Default values**: `GetOrDefault` relies on the `default` literal for the element type. For reference types this is `null`; for value types it is the zero-initialized value. Tests should account for this distinction when verifying results.
