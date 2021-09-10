# JsonUtility

A utility class for common JSON serialization, deserialization, and manipulation tasks. It provides static methods for converting between objects and JSON strings, extracting properties, merging JSON documents, validating JSON, and formatting JSON content.

## API

### `Serialize<T>(T obj)`
Serializes the given object to a compact JSON string.

- **Parameters**
  - `obj` – The object to serialize.
- **Return value**
  - A compact JSON string representation of `obj`.
- **Throws**
  - `ArgumentNullException` if `obj` is `null`.

### `SerializePretty<T>(T obj)`
Serializes the given object to a human-readable, indented JSON string.

- **Parameters**
  - `obj` – The object to serialize.
- **Return value**
  - A pretty-printed JSON string representation of `obj`.
- **Throws**
  - `ArgumentNullException` if `obj` is `null`.

### `Deserialize<T>(string json)`
Deserializes a JSON string into an instance of type `T`.

- **Parameters**
  - `json` – The JSON string to deserialize.
- **Return value**
  - An instance of type `T` populated from `json`, or `null` if deserialization fails.
- **Throws**
  - `ArgumentNullException` if `json` is `null`.

### `DeserializeDynamic(string json)`
Deserializes a JSON string into a dynamic object.

- **Parameters**
  - `json` – The JSON string to deserialize.
- **Return value**
  - A dynamic object representing the parsed JSON, or `null` if deserialization fails.
- **Throws**
  - `ArgumentNullException` if `json` is `null`.

### `GetProperty<T>(string json, string propertyName)`
Extracts the value of a named property from a JSON string.

- **Parameters**
  - `json` – The JSON string containing the property.
  - `propertyName` – The name of the property to extract.
- **Return value**
  - The value of the property cast to type `T`, or `default` if the property is missing or conversion fails.
- **Throws**
  - `ArgumentNullException` if `json` or `propertyName` is `null`.

### `MergeJson(string baseJson, string overlayJson)`
Merges two JSON strings, overlaying values from `overlayJson` onto `baseJson`.

- **Parameters**
  - `baseJson` – The base JSON document.
  - `overlayJson` – The JSON document whose values take precedence.
- **Return value**
  - A merged JSON string with properties from `overlayJson` overriding those in `baseJson`.
- **Throws**
  - `ArgumentNullException` if either input is `null`.
  - `JsonException` if either input is not valid JSON.

### `IsValidJson(string json)`
Determines whether the provided string is valid JSON.

- **Parameters**
  - `json` – The string to validate.
- **Return value**
  - `true` if `json` is valid JSON; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `json` is `null`.

### `GetJsonType(string json)`
Returns the JSON type of the top-level value in the provided JSON string.

- **Parameters**
  - `json` – The JSON string to inspect.
- **Return value**
  - A string indicating the JSON type (`"object"`, `"array"`, `"string"`, `"number"`, `"boolean"`, or `"null"`).
- **Throws**
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if `json` is not valid JSON.

### `FormatJson(string json)`
Reformats a JSON string to a consistent, compact layout without changing its semantics.

- **Parameters**
  - `json` – The JSON string to format.
- **Return value**
  - A compact, consistently formatted JSON string.
- **Throws**
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if `json` is not valid JSON.

## Usage
