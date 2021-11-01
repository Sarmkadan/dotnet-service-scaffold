# Service Scaffold

## ResultExtensions

The `ResultExtensions` class provides utility methods for working with `Result` and `Result<T>` types, enabling operation chaining, result aggregation, and error handling. These extensions simplify common patterns like transforming successful results, combining multiple results, extracting values safely, and validating conditions.

### Usage Examples

```csharp
// Chain synchronous operations on successful results
var result = Result.Success()
    .Then<int>(_ => 42)
    .Then(value => value * 2);

// Chain asynchronous operations on successful results
var asyncResult = Result.Success()
    .ThenAsync(async _ => 
    {
        await Task.Delay(10);
        return "processed";
    });

// Convert non-generic Result to generic Result<T>
var genericResult = Result.Success().ToGeneric<string>();

// Combine multiple results into a single aggregated result
var combined = Result.Combine(
    Result.Success(),
    Result.Failure("Error 1"),
    Result.Failure("Error 2")
);

// Add validation to a successful result
var validated = Result.Success(25)
    .Also(value => 
    {
        if (value <= 0) 
            return Result.Failure("Value must be positive");
        return Result.Success();
    });

// Extract value or use a default on failure
var valueOrDefault = Result.Failure<int>("Invalid").GetValueOrDefault(0);

// Extract value or throw on failure
try 
{
    var value = Result.Success(42).GetValueOrThrow();
}
catch (Exception ex) 
{
    // Handle exception
}

// Get error details from a failed result
var (errorMessage, errorCode) = Result.Failure("Invalid", "ERR001").GetError();

// Create result based on a condition
var conditionResult = Result.FromCondition(
    42 > 20, 
    "Value must be greater than 20", 
    "VAL001"
);
```

These extensions provide a fluent API for handling success/failure scenarios while maintaining strong type safety and avoiding boilerplate error-checking code.

## ServiceConfigurationExtensions

The `ServiceConfigurationExtensions` class provides helper methods for retrieving and updating service configuration values with type safety and validation. It includes methods for common data types like `double`, `decimal`, `DateTime`, and `Guid`, as well as utilities for checking system configuration flags and updating values conditionally.

### Usage Example

```csharp
var config = GetServiceConfiguration(); // Assume this retrieves a ServiceConfiguration instance

// Retrieve a string value with a default
var apiKey = config.GetValueOrDefault("API_KEY", "default123");

// Check if this is a system-level configuration
if (config.IsSystemConfiguration())
{
    // Safely retrieve an enum value
    var mode = config.GetEnumValue<EnvironmentMode>("ENV_MODE");
    
    // Update a numeric value only if it has changed
    config.UpdateValueIfChanged("MAX_RETRIES", 5);
}
else
{
    // Retrieve a Guid value with validation
    var serviceId = config.GetGuidValue("SERVICE_ID");
    
    // Get a decimal value for a timeout setting
    var timeout = config.GetDecimalValue("REQUEST_TIMEOUT");
}
```

This example demonstrates retrieving configuration values of different types, checking system configuration status, and conditionally updating values while ensuring type safety.
