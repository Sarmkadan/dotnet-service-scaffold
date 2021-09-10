# ExceptionExtensions

The `ExceptionExtensions` class provides a set of static extension methods and utility functions designed to standardize exception handling, logging, and error reporting within the `dotnet-service-scaffold` ecosystem. It facilitates the extraction of detailed diagnostic information, such as full stack traces and inner exception chains, while offering helpers for HTTP status code resolution, retry logic determination, and safe message retrieval to prevent secondary failures during error processing.

## API

### `GetFullMessage`
Retrieves the complete error message by concatenating the message of the target exception with the messages of all nested inner exceptions.
*   **Parameters**: `this Exception exception` – The source exception.
*   **Returns**: `string` – A concatenated string containing the message chain.
*   **Throws**: `ArgumentNullException` if `exception` is `null`.

### `GetFullStackTrace`
Generates a comprehensive stack trace string that includes the stack traces of the target exception and all inner exceptions.
*   **Parameters**: `this Exception exception` – The source exception.
*   **Returns**: `string` – The aggregated stack trace.
*   **Throws**: `ArgumentNullException` if `exception` is `null`.

### `Is<TException>`
Determines whether the target exception or any of its inner exceptions is of the specified type `TException`.
*   **Parameters**: `this Exception exception` – The source exception.
*   **Returns**: `bool` – `true` if a match is found in the exception chain; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if `exception` is `null`.

### `FindInnerException<TException>`
Searches the exception chain for the first occurrence of an exception of type `TException`.
*   **Parameters**: `this Exception exception` – The source exception.
*   **Returns**: `TException?` – The found exception instance, or `null` if no match exists in the chain.
*   **Throws**: `ArgumentNullException` if `exception` is `null`.

### `GetSafeMessage`
Retrieves the exception message with safeguards against null values or inaccessible message properties, returning a default fallback string if necessary.
*   **Parameters**: `this Exception exception` – The source exception.
*   **Returns**: `string` – The exception message or a safe default.
*   **Throws**: `ArgumentNullException` if `exception` is `null`.

### `GetHttpStatusCode`
Extracts the associated HTTP status code from the exception, typically by inspecting specific exception types (e.g., `HttpRequestException`) or custom properties.
*   **Parameters**: `this Exception exception` – The source exception.
*   **Returns**: `int` – The HTTP status code, or a default value (typically 500) if no code is defined.
*   **Throws**: `ArgumentNullException` if `exception` is `null`.

### `IsRetryable`
Evaluates whether the exception represents a transient failure suitable for automatic retry logic.
*   **Parameters**: `this Exception exception` – The source exception.
*   **Returns**: `bool` – `true` if the exception is considered retryable; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if `exception` is `null`.

### `ToErrorObject`
Converts the exception into a structured object suitable for serialization and API error responses.
*   **Parameters**: `this Exception exception` – The source exception.
*   **Returns**: `object` – An anonymous or typed object containing error details (message, type, stack trace).
*   **Throws**: `ArgumentNullException` if `exception` is `null`.

### `ToLogMessage`
Formats the exception into a standardized string optimized for logging systems, including severity indicators and context.
*   **Parameters**: `this Exception exception` – The source exception.
*   **Returns**: `string` – A formatted log entry.
*   **Throws**: `ArgumentNullException` if `exception` is `null`.

## Usage

### Example 1: Comprehensive Error Logging and Retry Logic
This example demonstrates how to use the extensions to determine if an operation should be retried and how to log the full diagnostic context if the failure is permanent.

```csharp
try
{
    await service.ExecuteOperationAsync();
}
catch (Exception ex)
{
    if (ex.IsRetryable())
    {
        logger.LogWarning("Transient failure detected. Retrying operation...");
        // Trigger retry mechanism
    }
    else
    {
        logger.LogError(ex.ToLogMessage());
        
        // Log full stack trace for deep debugging
        var fullTrace = ex.GetFullStackTrace();
        diagnosticService.RecordTrace(fullTrace);
        
        // Check specifically for a timeout
        if (ex.Is<TimeoutException>())
        {
            var timeoutEx = ex.FindInnerException<TimeoutException>();
            logger.LogError($"Operation timed out: {timeoutEx?.Message}");
        }
    }
}
```

### Example 2: API Error Response Construction
This example illustrates converting an caught exception into a safe user-facing message and a structured error object for an HTTP response.

```csharp
public IActionResult HandleFailure(Exception ex)
{
    // Get a safe message that won't throw if ex.Message is null
    var userMessage = ex.GetSafeMessage();
    
    // Determine the appropriate HTTP status code
    var statusCode = ex.GetHttpStatusCode();
    
    // Create a structured error payload
    var errorPayload = ex.ToErrorObject();
    
    // Get the full message chain for the response details if in development
    var fullDetails = Environment.IsDevelopment() ? ex.GetFullMessage() : "An unexpected error occurred.";

    return new ObjectResult(errorPayload)
    {
        StatusCode = statusCode,
        Value = new { Message = userMessage, Details = fullDetails }
    };
}
```

## Notes

*   **Null Safety**: All extension methods assume the input `exception` instance is not null. Passing `null` will result in an `ArgumentNullException` as these methods cannot operate on a non-existent instance.
*   **Thread Safety**: As this class consists entirely of static methods that do not maintain internal state and rely solely on the immutable data within the provided `Exception` objects, all members are thread-safe.
*   **Recursive Depth**: Methods traversing the inner exception chain (e.g., `GetFullMessage`, `FindInnerException`) rely on the `InnerException` property. In scenarios involving circular references in custom exception implementations (though rare and generally discouraged), these methods may cause a `StackOverflowException`.
*   **Serialization**: The `ToErrorObject` method returns a generic `object`. Consumers should ensure the returned type is compatible with the configured JSON serializer, particularly regarding cycle handling if the generated object includes direct exception references.
