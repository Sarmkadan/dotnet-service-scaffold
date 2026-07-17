# EncryptionUtilityJsonExtensions

Extension methods for serializing and deserializing strongly-typed objects to and from JSON using `System.Text.Json`. These utilities integrate with the encryption utilities in the scaffold to ensure sensitive data is handled securely during serialization and deserialization.

## API

### `ToJson<T>(this T value)`

Serializes the given object to a JSON string using `System.Text.Json` with default options.

- **Parameters**
  - `value` (`T`): The object to serialize.
- **Return Value**
  - `string`: The JSON representation of the object.
- **Exceptions**
  - Throws `System.Text.Json.JsonException` if serialization fails.
  - Throws `System.ArgumentNullException` if `value` is `null`.

---

### `ToJson<T>(this T value, JsonSerializerOptions options)`

Serializes the given object to a JSON string using `System.Text.Json` with custom serialization options.

- **Parameters**
  - `value` (`T`): The object to serialize.
  - `options` (`JsonSerializerOptions`): Custom serialization options.
- **Return Value**
  - `string`: The JSON representation of the object.
- **Exceptions**
  - Throws `System.Text.Json.JsonException` if serialization fails.
  - Throws `System.ArgumentNullException` if `value` is `null` or `options` is `null`.

---

### `FromJson<T>(this string? json)`

Deserializes a JSON string to an object of type `T` using `System.Text.Json` with default options.

- **Parameters**
  - `json` (`string?`): The JSON string to deserialize.
- **Return Value**
  - `T?`: The deserialized object, or `null` if `json` is `null`.
- **Exceptions**
  - Throws `System.Text.Json.JsonException` if deserialization fails.

---

### `FromJsonToByteArray(this string? json)`

Deserializes a JSON string to a `byte[]` using `System.Text.Json` with default options.

- **Parameters**
  - `json` (`string?`): The JSON string to deserialize.
- **Return Value**
  - `byte[]?`: The deserialized byte array, or `null` if `json` is `null`.
- **Exceptions**
  - Throws `System.Text.Json.JsonException` if deserialization fails.

---
### `TryFromJson<T>(this string? json, out T? result)`

Attempts to deserialize a JSON string to an object of type `T` using `System.Text.Json` with default options.

- **Parameters**
  - `json` (`string?`): The JSON string to deserialize.
  - `result` (`out T?`): The deserialized object, or `null` if deserialization fails.
- **Return Value**
  - `bool`: `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions**
  - None.

---
### `TryFromJson(this string? json, Type type, out object? result)`

Attempts to deserialize a JSON string to an object of the specified type using `System.Text.Json` with default options.

- **Parameters**
  - `json` (`string?`): The JSON string to deserialize.
  - `type` (`Type`): The type of the object to deserialize.
  - `result` (`out object?`): The deserialized object, or `null` if deserialization fails.
- **Return Value**
  - `bool`: `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions**
  - None.

## Usage
