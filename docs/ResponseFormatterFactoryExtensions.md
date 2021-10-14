# ResponseFormatterFactoryExtensions

Extension methods for working with `IResponseFormatterFactory` instances, providing convenient ways to retrieve, register, and inspect formatters. These methods simplify common scenarios such as selecting an appropriate formatter for a given media type, ensuring required formatters are available, and managing formatter registration.

## API

### `GetFormatterOrDefault`
Retrieves the first formatter that supports any of the specified media types, or returns the default formatter if no match is found.

- **Parameters**
  - `factory`: The `IResponseFormatterFactory` instance.
  - `mediaTypes`: A collection of media types to match against registered formatters.
- **Returns**
  - The first matching formatter, or the default formatter if no match is found.
- **Throws**
  - `ArgumentNullException`: If `factory` or `mediaTypes` is `null`.

### `TryGetFormatter`
Attempts to retrieve a formatter that supports any of the specified media types.

- **Parameters**
  - `factory`: The `IResponseFormatterFactory` instance.
  - `mediaTypes`: A collection of media types to match against registered formatters.
  - `formatter`: Output parameter that receives the matched formatter if successful.
- **Returns**
  - `true` if a matching formatter is found; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `factory` or `mediaTypes` is `null`.

### `GetFormatterRequired`
Retrieves the first formatter that supports any of the specified media types, throwing an exception if no match is found.

- **Parameters**
  - `factory`: The `IResponseFormatterFactory` instance.
  - `mediaTypes`: A collection of media types to match against registered formatters.
- **Returns**
  - The first matching formatter.
- **Throws**
  - `ArgumentNullException`: If `factory` or `mediaTypes` is `null`.
  - `InvalidOperationException`: If no formatter supports any of the specified media types.

### `RegisterFormatter`
Registers a formatter with the factory.

- **Parameters**
  - `factory`: The `IResponseFormatterFactory` instance.
  - `formatter`: The formatter to register.
- **Throws**
  - `ArgumentNullException`: If `factory` or `formatter` is `null`.

### `AreAnyMediaTypesSupported`
Determines whether any of the specified media types are supported by any registered formatter.

- **Parameters**
  - `factory`: The `IResponseFormatterFactory` instance.
  - `mediaTypes`: A collection of media types to check.
- **Returns**
  - `true` if at least one media type is supported; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `factory` or `mediaTypes` is `null`.

### `GetDefaultFormatter`
Retrieves the default formatter from the factory.

- **Parameters**
  - `factory`: The `IResponseFormatterFactory` instance.
- **Returns**
  - The default formatter.
- **Throws**
  - `ArgumentNullException`: If `factory` is `null`.
  - `InvalidOperationException`: If no default formatter is registered.

## Usage
