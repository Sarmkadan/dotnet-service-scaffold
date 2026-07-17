# ResultTestsJsonExtensions

Provides JSON serialization and deserialization extensions for `Result` and `Result<T>` types to support testing scenarios where results need to be persisted as JSON and later reconstructed.

## API

### `ToJson`

```csharp
public static string ToJson(this Result value, bool indented = false)
```
Serializes a `Result` instance to a JSON string.

- **value**: The result to serialize.
- **indented**: Whether to format the JSON with indentation. Defaults to `false`.
- **Returns**: A JSON string representation of the result.
- **Throws**: `ArgumentNullException` if `value` is null.

The JSON is serialized using camelCase property naming policy and ignores null values by default. When `indented` is `true`, the output is formatted with indentation for readability.

---

### `ToJson<T>`

```csharp
public static string ToJson<T>(this Result<T> value, bool indented = false)
```
Serializes a `Result<T>` instance to a JSON string.

- **T**: The type of the result value.
- **value**: The result to serialize.
- **indented**: Whether to format the JSON with indentation. Defaults to `false`.
- **Returns**: A JSON string representation of the result.
- **Throws**: `ArgumentNullException` if `value` is null.

The JSON is serialized using camelCase property naming policy and ignores null values by default. When `indented` is `true`, the output is formatted with indentation for readability.

---

### `FromJson`

```csharp
public static Result? FromJson(string json)
```
Deserializes a JSON string to a `Result` instance.

- **json**: The JSON string to deserialize.
- **Returns**: The deserialized result, or `null` if deserialization fails.
- **Throws**: `ArgumentNullException` if `json` is null.

The JSON is deserialized using camelCase property naming policy and ignores null values by default. If the JSON is malformed or does not represent a valid `Result`, returns `null`.

---

### `FromJson<T>`

```csharp
public static Result<T>? FromJson<T>(string json)
```
Deserializes a JSON string to a `Result<T>` instance.

- **T**: The type of the result value.
- **json**: The JSON string to deserialize.
- **Returns**: The deserialized result, or `null` if deserialization fails.
- **Throws**: `ArgumentNullException` if `json` is null.

The JSON is deserialized using camelCase property naming policy and ignores null values by default. If the JSON is malformed or does not represent a valid `Result<T>`, returns `null`.

---

### `TryFromJson`

```csharp
public static bool TryFromJson(string json, out Result? value)
```
Attempts to deserialize a JSON string to a `Result` instance.

- **json**: The JSON string to deserialize.
- **value**: The deserialized result, or `null` if deserialization fails.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `json` is null.

This method provides a non-throwing alternative to `FromJson`. It returns `false` and sets `value` to `null` if the JSON is malformed or does not represent a valid `Result`.

---

### `TryFromJson<T>`

```csharp
public static bool TryFromJson<T>(string json, out Result<T>? value)
```
Attempts to deserialize a JSON string to a `Result<T>` instance.

- **T**: The type of the result value.
- **json**: The JSON string to deserialize.
- **value**: The deserialized result, or `null` if deserialization fails.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `json` is null.

This method provides a non-throwing alternative to `FromJson<T>`. It returns `false` and sets `value` to `null` if the JSON is malformed or does not represent a valid `Result<T>`.


## Usage

### Serializing and deserializing a simple Result

```csharp
// Create a result
Result result = Result.Success();

// Serialize to JSON
string json = result.ToJson();
Console.WriteLine(json); // {"isSuccess":true}

// Deserialize back
Result? deserialized = ResultTestsJsonExtensions.FromJson(json);
Console.WriteLine(deserialized?.IsSuccess); // True
```

### Serializing and deserializing a typed Result

```csharp
// Create a typed result
Result<int> result = Result<int>.Success(42);

// Serialize to indented JSON
string json = result.ToJson(indented: true);
Console.WriteLine(json);
/*
{
  "isSuccess": true,
  "value": 42
}
*/

// Deserialize back
Result<int>? deserialized = ResultTestsJsonExtensions.FromJson<int>(json);
Console.WriteLine(deserialized?.Value); // 42
```

### Using TryFromJson for safe deserialization

```csharp
string malformedJson = "{ invalid json";

// TryFromJson safely handles malformed JSON
bool success = ResultTestsJsonExtensions.TryFromJson(malformedJson, out Result? result);
Console.WriteLine(success); // False
Console.WriteLine(result); // null
```

## Notes

- **Thread-safety**: The class is thread-safe. The shared `_jsonOptions` instance is immutable after construction, and all methods are read-only operations.
- **Null handling**: All methods validate their input parameters and throw `ArgumentNullException` for null arguments. The `FromJson` and `TryFromJson` methods return `null` for invalid JSON rather than throwing.
- **JSON format**: The JSON output uses camelCase property names and omits null values, ensuring consistent serialization across different .NET versions and platforms.
- **Indentation**: The `indented` parameter controls whether the JSON output is formatted for readability or compact for storage/transmission.
- **Error handling**: `FromJson` methods return `null` for deserialization failures, allowing callers to check for `null` rather than catching exceptions. `TryFromJson` methods provide an exception-free alternative for scenarios where exceptions should be avoided.