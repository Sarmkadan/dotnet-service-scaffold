# HttpUtility

Utility class providing helpers for common HTTP-related operations such as building and parsing authentication headers, query strings, URLs, and status code classification. Designed to simplify HTTP message handling in service clients and middleware.

## API

### `public static string CreateBasicAuthHeader(string username, string password)`

Creates a Basic authentication header value from the provided username and password.

- **Parameters**
  - `username`: The username to encode.
  - `password`: The password to encode.
- **Return value**: A string containing the `Authorization` header value in the format `Basic {base64}`.
- **Throws**: `ArgumentNullException` if `username` or `password` is `null`.

---

### `public static string CreateBearerAuthHeader(string token)`

Creates a Bearer authentication header value from the provided token.

- **Parameters**
  - `token`: The bearer token string.
- **Return value**: A string containing the `Authorization` header value in the format `Bearer {token}`.
- **Throws**: `ArgumentNullException` if `token` is `null`.

---

### `public static (string Username, string Password)? ParseBasicAuthHeader(string header)`

Parses a Basic authentication header into its username and password components.

- **Parameters**
  - `header`: The `Authorization` header value (e.g., `Basic dXNlcm5hbWU6cGFzc3dvcmQ=`).
- **Return value**: A tuple `(Username, Password)` if the header is valid and uses Basic auth; otherwise, `null`.
- **Throws**: `ArgumentNullException` if `header` is `null`.

---

### `public static string? ParseBearerToken(string header)`

Extracts the bearer token from an `Authorization` header.

- **Parameters**
  - `header`: The `Authorization` header value (e.g., `Bearer abc123`).
- **Return value**: The token string if the header uses Bearer auth; otherwise, `null`.
- **Throws**: `ArgumentNullException` if `header` is `null`.

---
### `public static string BuildQueryString(Dictionary<string, string> parameters)`

Constructs a URL-encoded query string from a dictionary of parameters.

- **Parameters**
  - `parameters`: Key-value pairs to include in the query string.
- **Return value**: A query string starting with `?` and including encoded key-value pairs (e.g., `?key1=value1&key2=value2`).
- **Throws**: `ArgumentNullException` if `parameters` is `null`.

---
### `public static Dictionary<string, string> ParseQueryString(string query)`

Parses a URL query string into a dictionary of key-value pairs.

- **Parameters**
  - `query`: The query portion of a URL (e.g., `?a=1&b=2`).
- **Return value**: A dictionary of decoded parameter names and values.
- **Throws**: `ArgumentNullException` if `query` is `null`.

---
### `public static bool IsSuccessStatusCode(int statusCode)`

Determines whether a status code indicates success (2xx).

- **Parameters**
  - `statusCode`: The HTTP status code to evaluate.
- **Return value**: `true` if `statusCode` is between 200 and 299 inclusive; otherwise, `false`.

---
### `public static bool IsClientErrorStatusCode(int statusCode)`

Determines whether a status code indicates a client error (4xx).

- **Parameters**
  - `statusCode`: The HTTP status code to evaluate.
- **Return value**: `true` if `statusCode` is between 400 and 499 inclusive; otherwise, `false`.

---
### `public static bool IsServerErrorStatusCode(int statusCode)`

Determines whether a status code indicates a server error (5xx).

- **Parameters**
  - `statusCode`: The HTTP status code to evaluate.
- **Return value**: `true` if `statusCode` is between 500 and 599 inclusive; otherwise, `false`.

---
### `public static bool IsRetryableStatusCode(int statusCode)`

Determines whether a status code indicates a retryable condition (5xx or 408, 429).

- **Parameters**
  - `statusCode`: The HTTP status code to evaluate.
- **Return value**: `true` if `statusCode` is 408, 429, or between 500 and 599 inclusive; otherwise, `false`.

---
### `public static int? GetRetryDelayMs(int statusCode)`

Extracts a retry delay in milliseconds from a status code if available (e.g., via `Retry-After` header logic).

- **Parameters**
  - `statusCode`: The HTTP status code to evaluate.
- **Return value**: The delay in milliseconds if the status code is 429 or 503 and a valid delay is present; otherwise, `null`.

---
### `public static string? GetMediaType(string contentType)`

Extracts the media type (e.g., `application/json`) from a `Content-Type` header value.

- **Parameters**
  - `contentType`: The value of a `Content-Type` header.
- **Return value**: The media type portion if present; otherwise, `null`.
- **Throws**: `ArgumentNullException` if `contentType` is `null`.

---
### `public static string? GetCharset(string contentType)`

Extracts the charset (e.g., `utf-8`) from a `Content-Type` header value.

- **Parameters**
  - `contentType`: The value of a `Content-Type` header.
- **Return value**: The charset portion if present; otherwise, `null`.
- **Throws**: `ArgumentNullException` if `contentType` is `null`.

---
### `public static string BuildUrl(string baseUrl, string? path = null, Dictionary<string, string>? query = null)`

Constructs a full URL from a base URL, optional path, and optional query parameters.

- **Parameters**
  - `baseUrl`: The base URL (e.g., `https://api.example.com`).
  - `path`: Optional path segment to append (e.g., `/v1/data`).
  - `query`: Optional dictionary of query parameters.
- **Return value**: A properly formatted URL with path and query string.
- **Throws**: `ArgumentNullException` if `baseUrl` is `null`.

---
### `public static string MaskSensitiveUrl(string url)`

Masks sensitive segments (e.g., passwords) in a URL for logging purposes.

- **Parameters**
  - `url`: The URL to mask.
- **Return value**: A sanitized URL with sensitive segments replaced by `***`.
- **Throws**: `ArgumentNullException` if `url` is `null`.
- **Remarks**: Only masks segments identified as containing credentials (e.g., `https://user:pass@example.com`). Other sensitive data is not masked.

## Usage
