# ResponseFormatterFactory

The `ResponseFormatterFactory` provides a centralized mechanism for managing and retrieving `IResponseFormatter` implementations based on media types. This component enables the decoupling of service logic from specific serialization or formatting requirements, allowing for the dynamic resolution of formatters at runtime within the application infrastructure.

## API

### ResponseFormatterFactory()
Initializes a new instance of the `ResponseFormatterFactory` class.

### IResponseFormatter GetFormatter(string mediaType)
Retrieves the `IResponseFormatter` associated with the specified media type.
*   **Parameters:**
    *   `mediaType`: The MIME type string (e.g., "application/json").
*   **Returns:** The `IResponseFormatter` implementation corresponding to the requested media type.
*   **Throws:**
    *   `ArgumentNullException`: Thrown if `mediaType` is null or whitespace.
    *   `KeyNotFoundException`: Thrown if no formatter is registered for the provided media type.

### void RegisterFormatter(string mediaType, IResponseFormatter formatter)
Registers a new `IResponseFormatter` instance for a specific media type.
*   **Parameters:**
    *   `mediaType`: The MIME type string to associate with the formatter.
    *   `formatter`: The `IResponseFormatter` implementation to register.
*   **Throws:**
    *   `ArgumentNullException`: Thrown if `mediaType` or `formatter` is null.

### IEnumerable<string> GetSupportedMediaTypes()
Returns a collection of all currently registered media types supported by the factory.
*   **Returns:** An `IEnumerable<string>` containing the registered media types.

### bool IsMediaTypeSupported(string mediaType)
Determines whether a formatter is registered for the specified media type.
*   **Parameters:**
    *   `mediaType`: The MIME type string to check.
*   **Returns:** `true` if a formatter is registered for the media type; otherwise, `false`.

## Usage

### Registering and Retrieving a Formatter
```csharp
var factory = new ResponseFormatterFactory();
factory.RegisterFormatter("application/json", new JsonResponseFormatter());

// Retrieve the formatter later in the request pipeline
var formatter = factory.GetFormatter("application/json");
var result = formatter.Format(data);
```

### Checking Media Type Support Before Retrieval
```csharp
string requestedType = "application/xml";

if (factory.IsMediaTypeSupported(requestedType))
{
    var formatter = factory.GetFormatter(requestedType);
    // Proceed with formatting
}
else
{
    // Handle unsupported media type
    throw new NotSupportedException($"Media type {requestedType} is not supported.");
}
```

## Notes

*   **Thread Safety:** The implementation is intended to be thread-safe for concurrent read operations (`GetFormatter`, `IsMediaTypeSupported`, `GetSupportedMediaTypes`). However, concurrent modifications—specifically calling `RegisterFormatter` while other threads are reading—may require external synchronization or a thread-safe dictionary implementation within the factory.
*   **Case Sensitivity:** Media type string matching is generally treated as case-insensitive, adhering to standard HTTP MIME type conventions. Ensure that strings passed to `RegisterFormatter` and `GetFormatter` are normalized if necessary.
*   **Registration Timing:** It is recommended to register all required formatters during the application startup/configuration phase to avoid potential race conditions during request processing.
