# ServiceConfigurationExtensions

Extension methods for retrieving and updating strongly-typed values from a service configuration store. These methods provide type-safe access to configuration values while handling common conversion and validation scenarios.

## API

### `GetDoubleValue`
Retrieves a configuration value as a `double`.

- **Parameters**
  - `IConfiguration configuration`: The configuration store.
  - `string key`: The key identifying the configuration value.
  - `double defaultValue`: The value to return if the key is missing or invalid.
- **Return Value**
  Returns the parsed `double` value if the key exists and is valid; otherwise, returns `defaultValue`.
- **Exceptions**
  Throws `FormatException` if the stored value cannot be parsed as a `double`.

### `GetDecimalValue`
Retrieves a configuration value as a `decimal`.

- **Parameters**
  - `IConfiguration configuration`: The configuration store.
  - `string key`: The key identifying the configuration value.
  - `decimal defaultValue`: The value to return if the key is missing or invalid.
- **Return Value**
  Returns the parsed `decimal` value if the key exists and is valid; otherwise, returns `defaultValue`.
- **Exceptions**
  Throws `FormatException` if the stored value cannot be parsed as a `decimal`.

### `GetDateTimeValue`
Retrieves a configuration value as a `DateTime`.

- **Parameters**
  - `IConfiguration configuration`: The configuration store.
  - `string key`: The key identifying the configuration value.
  - `DateTime defaultValue`: The value to return if the key is missing or invalid.
- **Return Value**
  Returns the parsed `DateTime` value if the key exists and is valid; otherwise, returns `defaultValue`.
- **Exceptions**
  Throws `FormatException` if the stored value cannot be parsed as a `DateTime`.

### `GetGuidValue`
Retrieves a configuration value as a `Guid`.

- **Parameters**
  - `IConfiguration configuration`: The configuration store.
  - `string key`: The key identifying the configuration value.
  - `Guid defaultValue`: The value to return if the key is missing or invalid.
- **Return Value**
  Returns the parsed `Guid` value if the key exists and is valid; otherwise, returns `defaultValue`.
- **Exceptions**
  Throws `FormatException` if the stored value cannot be parsed as a `Guid`.

### `UpdateValueIfChanged`
Updates a configuration value only if the new value differs from the existing one.

- **Parameters**
  - `IConfiguration configuration`: The configuration store.
  - `string key`: The key identifying the configuration value.
  - `string newValue`: The new value to set.
- **Return Value**
  Returns `true` if the value was updated; otherwise, returns `false`.
- **Exceptions**
  Throws `ArgumentNullException` if `configuration` or `key` is `null`.

### `GetValueOrDefault`
Retrieves a configuration value as a string, or returns a default if missing.

- **Parameters**
  - `IConfiguration configuration`: The configuration store.
  - `string key`: The key identifying the configuration value.
  - `string defaultValue`: The value to return if the key is missing.
- **Return Value**
  Returns the configuration value if the key exists; otherwise, returns `defaultValue`.

### `IsSystemConfiguration`
Determines whether a configuration key represents a system-level setting.

- **Parameters**
  - `string key`: The configuration key to check.
- **Return Value**
  Returns `true` if the key starts with the system configuration prefix (e.g., `"System:"`); otherwise, returns `false`.

### `GetEnumValue<T>`
Retrieves a configuration value as an enum of type `T`.

- **Parameters**
  - `IConfiguration configuration`: The configuration store.
  - `string key`: The key identifying the configuration value.
  - `T defaultValue`: The value to return if the key is missing or invalid.
- **Type Parameters**
  - `T`: The enum type to parse the value into.
- **Return Value**
  Returns the parsed enum value if the key exists and is valid; otherwise, returns `defaultValue`.
- **Exceptions**
  Throws `ArgumentException` if `T` is not an enum type.
  Throws `FormatException` if the stored value cannot be parsed as the specified enum.

## Usage

### Example 1: Retrieving strongly-typed configuration values
