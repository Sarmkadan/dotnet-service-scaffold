# PerformanceUtilityValidation

`PerformanceUtilityValidation` provides static methods to validate performance-related configuration and enforce validation rules at runtime. It is designed for use in service initialization and configuration pipelines to ensure that performance-sensitive settings are valid before the application proceeds.

## API

### `Validate(PerformanceSettings settings)`

Validates a `PerformanceSettings` instance and returns a list of validation error messages.

- **Parameters**
  - `settings` – The `PerformanceSettings` instance to validate.
- **Return Value**
  - An `IReadOnlyList<string>` of error messages. If the list is empty, the settings are valid.
- **Throws**
  - `ArgumentNullException` – If `settings` is `null`.

### `Validate(PerformanceOptions options)`

Validates a `PerformanceOptions` instance and returns a list of validation error messages.

- **Parameters**
  - `options` – The `PerformanceOptions` instance to validate.
- **Return Value**
  - An `IReadOnlyList<string>` of error messages. If the list is empty, the options are valid.
- **Throws**
  - `ArgumentNullException` – If `options` is `null`.

### `IsValid(PerformanceSettings settings)`

Determines whether a `PerformanceSettings` instance is valid.

- **Parameters**
  - `settings` – The `PerformanceSettings` instance to check.
- **Return Value**
  - `true` if the settings are valid; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` – If `settings` is `null`.

### `EnsureValid(PerformanceSettings settings)`

Validates a `PerformanceSettings` instance and throws an exception if it is invalid.

- **Parameters**
  - `settings` – The `PerformanceSettings` instance to validate.
- **Throws**
  - `ArgumentNullException` – If `settings` is `null`.
  - `InvalidOperationException` – If the settings are invalid, with a message describing the validation failure.

### `IsValid(PerformanceOptions options)`

Determines whether a `PerformanceOptions` instance is valid.

- **Parameters**
  - `options` – The `PerformanceOptions` instance to check.
- **Return Value**
  - `true` if the options are valid; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` – If `options` is `null`.

### `EnsureValid(PerformanceOptions options)`

Validates a `PerformanceOptions` instance and throws an exception if it is invalid.

- **Parameters**
  - `options` – The `PerformanceOptions` instance to validate.
- **Throws**
  - `ArgumentNullException` – If `options` is `null`.
  - `InvalidOperationException` – If the options are invalid, with a message describing the validation failure.

## Usage

```csharp
// Example 1: Validating performance settings during startup
var settings = new PerformanceSettings
{
    MaxConcurrentRequests = 100,
    CacheTtlSeconds = 300
};

if (PerformanceUtilityValidation.IsValid(settings))
{
    _logger.LogInformation("Performance settings are valid.");
}
else
{
    var errors = PerformanceUtilityValidation.Validate(settings);
    foreach (var error in errors)
    {
        _logger.LogError(error);
    }
    throw new InvalidOperationException("Invalid performance configuration.");
}

// Example 2: Enforcing validation in a configuration pipeline
try
{
    PerformanceUtilityValidation.EnsureValid(settings);
    _logger.LogInformation("Performance settings validated successfully.");
}
catch (Exception ex)
{
    _logger.LogCritical(ex, "Failed to validate performance settings.");
    throw;
}
```

## Notes

- The validation logic enforces that `MaxConcurrentRequests` is positive and `CacheTtlSeconds` is non-negative. If either value is out of bounds, a validation error is returned.
- All methods are thread-safe and may be called concurrently from multiple threads without additional synchronization.
- Passing `null` to any method results in an `ArgumentNullException`; the methods do not attempt to handle `null` gracefully.
- The validation logic does not mutate the input objects; the methods are read-only with respect to their parameters.
