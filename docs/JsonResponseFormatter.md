# JsonResponseFormatter

The `JsonResponseFormatter` class provides functionality for serializing C# objects into JSON-formatted strings, facilitating standardized data output within the `dotnet-service-scaffold` infrastructure. It implements a structured approach for handling API response generation by overriding core base formatter members to ensure consistent data transmission and serialization behavior.

## API

### JsonResponseFormatter()
Initializes a new instance of the `JsonResponseFormatter` class.

### Task<string> FormatAsync(object data)
Asynchronously converts the provided object into a JSON-formatted string representation.

### bool CanFormat
Indicates whether the formatter supports the data structure provided for serialization.

### override DateTime Read(...)
Reads data from the underlying source. This member is an override of the base formatter functionality and returns a `DateTime` representing the read operation's timestamp or associated data.

### override void Write(...)
Writes data to the underlying destination. This member is an override of the base formatter functionality and handles the serialization output process.

## Usage

### Basic Formatting
```csharp
var formatter = new JsonResponseFormatter();
var data = new { Name = "Example", Status = "Active" };
string json = await formatter.FormatAsync(data);
```

### Conditional Formatting
```csharp
var formatter = new JsonResponseFormatter();
var complexObject = GetComplexData();

if (formatter.CanFormat)
{
    var json = await formatter.FormatAsync(complexObject);
    // Proceed with sending the JSON response
}
else
{
    // Handle cases where formatting is not supported
}
```

## Notes

- **Thread Safety**: This class is generally not thread-safe. Instances should not be shared across threads during active serialization operations.
- **Exception Handling**: The `FormatAsync` method may throw exceptions if serialization fails, such as when encountering circular references or unsupported data types.
- **Inheritance**: The `Read` and `Write` methods are overrides of base class members. Their behavior relies on the underlying formatter contract, and they should be used in accordance with that contract to maintain data integrity.
