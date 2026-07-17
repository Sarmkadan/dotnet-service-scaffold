# CollectionUtilityValidation

Provides validation methods for collection operations to ensure parameters meet requirements before being processed by `CollectionUtility` methods. This static class offers three categories of functionality: detailed validation that returns error messages, boolean checks for validity, and exception-throwing validation that immediately fails on invalid inputs.

## API

### Validate Methods

#### `Validate<T>(IEnumerable<T>? source, int batchSize = 1)`

Validates a collection and batch size parameter before collection operations.

- **Parameters:**
  - `source` - The collection to validate
  - `batchSize` - The batch size (must be positive, maximum 1,000,000)
- **Returns:** `IReadOnlyList<string>` - Empty list if valid, otherwise list of validation error messages
- **Throws:** Never throws exceptions; returns error messages for all validation failures

#### `Validate<T>(IEnumerable<T>? source, int chunkSize, bool isChunkValidation)`

Validates a collection and chunk size parameter with explicit chunk validation flag.

- **Parameters:**
  - `source` - The collection to validate
  - `chunkSize` - The chunk size (must be positive, maximum 1,000,000)
  - `isChunkValidation` - Flag indicating this is chunk validation (parameter exists for overload resolution)
- **Returns:** `IReadOnlyList<string>` - Empty list if valid, otherwise list of validation error messages
- **Throws:** Never throws exceptions; returns error messages for all validation failures

#### `Validate<T>(IEnumerable<T>? first, IEnumerable<T>? second)`

Validates two collections for operations requiring pairwise validation.

- **Parameters:**
  - `first` - The first collection to validate
  - `second` - The second collection to validate
- **Type Parameters:**
  - `T` - The element type (must be non-nullable)
- **Returns:** `IReadOnlyList<string>` - Empty list if valid, otherwise list of validation error messages
- **Throws:** Never throws exceptions; returns error messages for all validation failures

#### `Validate<T>(IEnumerable<T>? source, Func<T, bool>? predicate)`

Validates a collection and predicate function for filtering operations.

- **Parameters:**
  - `source` - The collection to validate
  - `predicate` - The predicate function to validate
- **Returns:** `IReadOnlyList<string>` - Empty list if valid, otherwise list of validation error messages
- **Throws:** Never throws exceptions; returns error messages for all validation failures

#### `Validate<T, TKey>(IEnumerable<T>? source, Func<T, TKey>? keySelector)`

Validates a collection and key selector function for grouping operations.

- **Parameters:**
  - `source` - The collection to validate
  - `keySelector` - The key selector function to validate
- **Type Parameters:**
  - `T` - The element type
  - `TKey` - The key type (must be non-nullable)
- **Returns:** `IReadOnlyList<string>` - Empty list if valid, otherwise list of validation error messages
- **Throws:** Never throws exceptions; returns error messages for all validation failures

### IsValid Methods

#### `IsValid<T>(IEnumerable<T>? source, int batchSize = 1)`

Checks if collection and batch size parameters are valid without throwing exceptions.

- **Parameters:**
  - `source` - The collection to validate
  - `batchSize` - The batch size to validate
- **Returns:** `bool` - `true` if valid, `false` if any validation fails
- **Throws:** Never throws exceptions; returns `false` for all validation failures

#### `IsValid<T>(IEnumerable<T>? source, int chunkSize, bool isChunkValidation)`

Checks if collection and chunk size parameters are valid with explicit chunk validation flag.

- **Parameters:**
  - `source` - The collection to validate
  - `chunkSize` - The chunk size to validate
  - `isChunkValidation` - Flag indicating this is chunk validation
- **Returns:** `bool` - `true` if valid, `false` if any validation fails
- **Throws:** Never throws exceptions; returns `false` for all validation failures

#### `IsValid<T>(IEnumerable<T>? first, IEnumerable<T>? second)`

Checks if two collections are valid for pairwise operations.

- **Parameters:**
  - `first` - The first collection to validate
  - `second` - The second collection to validate
- **Type Parameters:**
  - `T` - The element type (must be non-nullable)
- **Returns:** `bool` - `true` if valid, `false` if any validation fails
- **Throws:** Never throws exceptions; returns `false` for all validation failures

#### `IsValid<T>(IEnumerable<T>? source, Func<T, bool>? predicate)`

Checks if collection and predicate function are valid for filtering operations.

- **Parameters:**
  - `source` - The collection to validate
  - `predicate` - The predicate function to validate
- **Returns:** `bool` - `true` if valid, `false` if any validation fails
- **Throws:** Never throws exceptions; returns `false` for all validation failures

#### `IsValid<T, TKey>(IEnumerable<T>? source, Func<T, TKey>? keySelector)`

Checks if collection and key selector function are valid for grouping operations.

- **Parameters:**
  - `source` - The collection to validate
  - `keySelector` - The key selector function to validate
- **Type Parameters:**
  - `T` - The element type
  - `TKey` - The key type (must be non-nullable)
- **Returns:** `bool` - `true` if valid, `false` if any validation fails
- **Throws:** Never throws exceptions; returns `false` for all validation failures

### EnsureValid Methods

#### `EnsureValid<T>(IEnumerable<T>? source, int batchSize = 1)`

Ensures collection and batch size parameters are valid, throwing an exception if not.

- **Parameters:**
  - `source` - The collection to validate
  - `batchSize` - The batch size to validate
- **Returns:** `void`
- **Throws:** `ArgumentException` with detailed validation messages if any validation fails

#### `EnsureValid<T>(IEnumerable<T>? source, int chunkSize, bool isChunkValidation)`

Ensures collection and chunk size parameters are valid with explicit chunk validation flag, throwing an exception if not.

- **Parameters:**
  - `source` - The collection to validate
  - `chunkSize` - The chunk size to validate
  - `isChunkValidation` - Flag indicating this is chunk validation
- **Returns:** `void`
- **Throws:** `ArgumentException` with detailed validation messages if any validation fails

#### `EnsureValid<T>(IEnumerable<T>? first, IEnumerable<T>? second)`

Ensures two collections are valid for pairwise operations, throwing an exception if not.

- **Parameters:**
  - `first` - The first collection to validate
  - `second` - The second collection to validate
- **Type Parameters:**
  - `T` - The element type (must be non-nullable)
- **Returns:** `void`
- **Throws:** `ArgumentException` with detailed validation messages if any validation fails

#### `EnsureValid<T>(IEnumerable<T>? source, Func<T, bool>? predicate)`

Ensures collection and predicate function are valid for filtering operations, throwing an exception if not.

- **Parameters:**
  - `source` - The collection to validate
  - `predicate` - The predicate function to validate
- **Returns:** `void`
- **Throws:** `ArgumentException` with detailed validation messages if any validation fails

#### `EnsureValid<T, TKey>(IEnumerable<T>? source, Func<T, TKey>? keySelector)`

Ensures collection and key selector function are valid for grouping operations, throwing an exception if not.

- **Parameters:**
  - `source` - The collection to validate
  - `keySelector` - The key selector function to validate
- **Type Parameters:**
  - `T` - The element type
  - `TKey` - The key type (must be non-nullable)
- **Returns:** `void`
- **Throws:** `ArgumentException` with detailed validation messages if any validation fails

## Usage

### Example 1: Batch Processing Validation

```csharp
using DotnetServiceScaffold.Shared.Utilities;

var products = GetProductList();
var batchSize = 50;

// Validate before processing
var validationErrors = CollectionUtilityValidation.Validate(products, batchSize);

if (validationErrors.Count == 0)
{
    // Safe to process in batches
    ProcessInBatches(products, batchSize);
}
else
{
    Console.WriteLine("Validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

### Example 2: Grouping Operation with Key Selector

```csharp
using DotnetServiceScaffold.Shared.Utilities;

var orders = GetOrderList();
Func<Order, string> customerIdSelector = o => o.CustomerId;

// Validate key selector before grouping
if (CollectionUtilityValidation.IsValid(orders, customerIdSelector))
{
    // Safe to group by customer
    var ordersByCustomer = orders.ToLookup(customerIdSelector);
    ProcessCustomerOrders(ordersByCustomer);
}
else
{
    Console.WriteLine("Cannot group orders - validation failed");
}
```

## Notes

### Validation Rules

- **Null collections:** All validation methods reject null collections with appropriate error messages
- **Empty collections:** Empty collections are rejected unless explicitly allowed by the consuming method
- **Batch/Chunk sizes:** Must be positive integers between 1 and 1,000,000 inclusive
- **Predicate functions:** Must not be null
- **Key selector functions:** Must not be null and must return non-nullable keys

### Error Messages

Validation methods return human-readable error messages that can be directly displayed to users or logged:
- "Source collection cannot be null."
- "Source collection is empty."
- "Batch size must be a positive integer."
- "Batch size is excessively large (maximum 1,000,000)."
- "First collection cannot be null."
- "Second collection cannot be null."
- "Both collections cannot be null."
- "First collection is empty."
- "Second collection is empty."
- "Predicate function cannot be null."
- "Key selector function cannot be null."

### Thread Safety

All methods in `CollectionUtilityValidation` are thread-safe. The class is stateless and all methods are implemented as static methods that operate only on their parameters. Multiple threads can safely call any combination of these methods concurrently without synchronization.

### Performance Considerations

- Validation methods use `Any()` to check for empty collections, which is efficient for most collection types
- Error messages are constructed only when validation fails
- The `IsValid` methods catch all exceptions and return `false` rather than propagating exceptions, making them safe for use in performance-critical paths
- All methods avoid materializing collections unnecessarily

### Integration with CollectionUtility

These validation methods are designed to be used with `CollectionUtility` methods. They validate the same parameters that `CollectionUtility` methods accept, ensuring consistent validation behavior across the codebase.