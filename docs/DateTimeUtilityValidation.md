# DateTimeUtilityValidation

Provides a set of static validation methods for common date, time, and duration patterns used throughout the `dotnet-service-scaffold` project. These utilities centralize parsing, range checking, and formatting logic to ensure consistent validation of user-supplied or system-generated date/time values, durations, birth dates, and reference dates. Methods return validation errors as a list of strings, return a simple boolean for quick checks, or throw an exception when the input is invalid.

## API

### `ValidateDateTime` (overload 1)

```csharp
public static IReadOnlyList<string> ValidateDateTime(string value)
```

Validates a string representation of a date and time. Returns a list of error messages if the string is null, empty, or cannot be parsed into a valid `DateTime`; returns an empty list on success.

### `ValidateDateTime` (overload 2)

```csharp
public static IReadOnlyList<string> ValidateDateTime(DateTime value)
```

Validates a `DateTime` value against application-specific constraints (e.g., not in the far future or before a minimum allowed date). Returns a list of error messages if the value falls outside the acceptable range; returns an empty list on success.

### `ValidateDuration`

```csharp
public static IReadOnlyList<string> ValidateDuration(string value)
```

Validates a string representing a duration (e.g., `"00:30:00"` or `"1.02:00:00"`). Returns a list of error messages if the string is null, empty, or cannot be parsed into a valid `TimeSpan`; returns an empty list on success.

### `ValidateBirthDate`

```csharp
public static IReadOnlyList<string> ValidateBirthDate(DateTime value)
```

Validates a birth date. Returns a list of error messages if the date is in the future, is more than a reasonable maximum age in the past, or is the default `DateTime.MinValue`; returns an empty list on success.

### `ValidateReferenceDate`

```csharp
public static IReadOnlyList<string> ValidateReferenceDate(DateTime value)
```

Validates a reference date used for business logic (e.g., effective date, expiration date). Returns a list of error messages if the date is outside an expected range (e.g., before a system-defined minimum or after a maximum); returns an empty list on success.

### `IsValidDateTime` (overload 1)

```csharp
public static bool IsValidDateTime(string value)
```

Returns `true` if the string can be parsed into a valid `DateTime`; otherwise `false`. Does not throw.

### `IsValidDateTime` (overload 2)

```csharp
public static bool IsValidDateTime(DateTime value)
```

Returns `true` if the `DateTime` value satisfies the same constraints used in `ValidateDateTime(DateTime)`; otherwise `false`. Does not throw.

### `IsValidDuration`

```csharp
public static bool IsValidDuration(string value)
```

Returns `true` if the string can be parsed into a valid `TimeSpan`; otherwise `false`. Does not throw.

### `IsValidBirthDate`

```csharp
public static bool IsValidBirthDate(DateTime value)
```

Returns `true` if the birth date passes the same validation as `ValidateBirthDate`; otherwise `false`. Does not throw.

### `EnsureValidDateTime` (overload 1)

```csharp
public static void EnsureValidDateTime(string value)
```

Parses the string and validates the resulting `DateTime`. Throws an `ArgumentException` (or a more specific derived exception) if the string is null, empty, or cannot be parsed, or if the parsed value fails range checks.

### `EnsureValidDateTime` (overload 2)

```csharp
public static void EnsureValidDateTime(DateTime value)
```

Validates the `DateTime` value. Throws an `ArgumentOutOfRangeException` (or similar) if the value is outside the acceptable range.

### `EnsureValidDuration`

```csharp
public static void EnsureValidDuration(string value)
```

Parses and validates the duration string. Throws an `ArgumentException` if the string is null, empty, or cannot be parsed into a `TimeSpan`.

### `EnsureValidBirthDate`

```csharp
public static void EnsureValidBirthDate(DateTime value)
```

Validates the birth date. Throws an `ArgumentOutOfRangeException` if the date is in the future, too far in the past, or the default `DateTime.MinValue`.

## Usage

### Example 1: Validating user input in a controller action

```csharp
public IActionResult CreateEvent(string startDate, string duration)
{
    var errors = new List<string>();

    errors.AddRange(DateTimeUtilityValidation.ValidateDateTime(startDate));
    errors.AddRange(DateTimeUtilityValidation.ValidateDuration(duration));

    if (errors.Count > 0)
    {
        return BadRequest(new { Errors = errors });
    }

    // Proceed with creation
    return Ok();
}
```

### Example 2: Ensuring a birth date is valid before persisting

```csharp
public void RegisterUser(DateTime birthDate)
{
    try
    {
        DateTimeUtilityValidation.EnsureValidBirthDate(birthDate);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        throw new ValidationException("Invalid birth date.", ex);
    }

    // Save to database
    _userRepository.Add(new User { BirthDate = birthDate });
}
```

## Notes

- All methods are static and operate only on their parameters; they do not maintain any internal state. Consequently, they are thread-safe and can be called concurrently from multiple threads without synchronization.
- The `Validate*` methods return an empty `IReadOnlyList<string>` when the input is valid. The returned list is read-only and should not be modified by callers.
- The `IsValid*` methods are lightweight alternatives that avoid allocating a list; they are suitable for quick checks in performance-sensitive paths.
- The `Ensure*` methods throw exceptions on failure. The exact exception type may vary (e.g., `ArgumentException`, `ArgumentNullException`, `ArgumentOutOfRangeException`) depending on the nature of the validation failure.
- Edge cases handled internally include:
  - `null` or empty strings (treated as invalid).
  - Strings with only whitespace (treated as invalid).
  - `DateTime.MinValue` and `DateTime.MaxValue` (typically considered invalid for birth dates and reference dates).
  - Leap year dates (February 29) are accepted when valid.
  - Duration strings must conform to the standard `TimeSpan` parseable formats; negative durations are generally rejected unless explicitly allowed by the application context.
- The exact range constraints for `ValidateDateTime(DateTime)`, `ValidateBirthDate`, and `ValidateReferenceDate` are defined by the application’s configuration (e.g., minimum and maximum allowed dates). These constraints are not exposed publicly but are consistent across all overloads.
