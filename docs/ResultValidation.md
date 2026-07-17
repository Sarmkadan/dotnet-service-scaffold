# ResultValidation

Provides a set of static helper members for validating objects and aggregating validation results into readable error messages. The type is intended to be used throughout the application to centralize validation logic and to enforce invariants before proceeding with business logic.

## API

### Validate
```csharp
public static IReadOnlyList<string> Validate(object instance)
```
**Purpose** – Returns a list of validation error messages for the supplied object.  
**Parameters** – `instance`: The object to validate.  
**Return value** – An `IReadOnlyList<string>` containing zero or more error messages; an empty list indicates the object is valid.  
**Exceptions** – Throws `ArgumentNullException` if `instance` is `null`.

### Validate<T>
```csharp
public static IReadOnlyList<string> Validate<T>(T instance)
```
**Purpose** – Generic counterpart of `Validate` that provides compile‑time type safety for the instance being validated.  
**Parameters** – `instance`: The object of type `T` to validate.  
**Return value** – An `IReadOnlyList<string>` of error messages; empty when the instance passes validation.  
**Exceptions** – Throws `ArgumentNullException` if `instance` is `null`.

### IsValid
```csharp
public static bool IsValid(object instance)
```
**Purpose** – Determines whether the supplied object passes validation without returning the detailed messages.  
**Parameters** – `instance`: The object to validate.  
**Return value** – `true` if the object has no validation errors; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `instance` is `null`.

### IsValid<T>
```csharp
public static bool IsValid<T>(T instance)
```
**Purpose** – Generic version of `IsValid`.  
**Parameters** – `instance`: The object of type `T` to validate.  
**Return value** – `true` when the instance is valid; `false` otherwise.  
**Exceptions** – Throws `ArgumentNullException` if `instance` is `null`.

### EnsureValid
```csharp
public static void EnsureValid(object instance)
```
**Purpose** – Asserts that the supplied object is valid; throws an exception if validation fails.  
**Parameters** – `instance`: The object to validate.  
**Return value** – None.  
**Exceptions** –  
- `ArgumentNullException` if `instance` is `null`.  
- `ValidationException` (or a derived exception) containing the concatenated validation error messages when the instance is invalid.

### EnsureValid<T>
```csharp
public static void EnsureValid<T>(T instance)
```
**Purpose** – Generic version of `EnsureValid`.  
**Parameters** – `instance`: The object of type `T` to validate.  
**Return value** – None.  
**Exceptions** –  
- `ArgumentNullException` if `instance` is `null`.  
- `ValidationException` (or a derived exception) with the validation error messages when the instance is invalid.

## Usage

### Example 1: Simple validation check
```csharp
var dto = new CreateOrderDto { CustomerId = 123, Items = new List<OrderItemDto>() };

IReadOnlyList<string> errors = ResultValidation.Validate(dto);
if (errors.Count > 0)
{
    // Log or return the validation problems
    foreach (var error in errors)
    {
        _logger.Warning("Validation error: {Error}", error);
    }
    return BadRequest(errors);
}

// Proceed with processing because the DTO is valid
```

### Example 2: Using EnsureValid to throw on failure
```csharp
public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDto dto)
{
    // Throws ValidationException if dto is not valid
    ResultValidation.EnsureValid(dto);

    // At this point we know dto is valid
    var order = await _orderService.CreateAsync(dto);
    return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
}
```

## Notes
- All members are **static** and contain no mutable state; therefore they are thread‑safe and can be called concurrently from multiple threads without additional synchronization.
- An empty list returned by `Validate`/`Validate<T>` indicates success; callers should treat any non‑empty list as a failure condition.
- Passing `null` to any member results in an `ArgumentNullException`; the methods do not treat `null` as a valid state.
- The concrete exception type thrown by `EnsureValid`/`EnsureValid<T>` is implementation‑specific but will always contain the validation messages, allowing callers to catch a single exception type for handling invalid input.
- Because the validation logic is encapsulated within these helpers, changes to validation rules only require updates in one place, reducing the risk of inconsistency across the code duplication.
