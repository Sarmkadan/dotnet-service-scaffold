# HttpContextExtensions

Extension methods for `Microsoft.AspNetCore.Http.HttpContext` that provide convenient access to common HTTP request and response operations, including user identity, headers, content negotiation, and connection information.

## API

### `public static Guid? GetUserId(HttpContext context)`

Retrieves the current user's unique identifier from the `sub` claim in the authenticated user's claims principal. Returns `null` if the user is not authenticated or the claim is missing.

### `public static string? GetUserEmail(HttpContext context)`

Retrieves the current user's email address from the `email` claim in the authenticated user's claims principal. Returns `null` if the user is not authenticated or the claim is missing.

### `public static string? GetUsername(HttpContext context)`

Retrieves the current user's username from the `preferred_username` claim in the authenticated user's claims principal. Returns `null` if the user is not authenticated or the claim is missing.

### `public static bool IsAuthenticated(HttpContext context)`

Determines whether the current user is authenticated. Returns `true` if the user has been authenticated; otherwise, `false`.

### `public static string? GetClaim(HttpContext context, string claimType)`

Retrieves the value of the specified claim type from the authenticated user's claims principal. Returns `null` if the user is not authenticated, the claim type does not exist, or the claim value is empty.

- **Parameters**
  - `claimType`: The type of the claim to retrieve.
- **Throws**
  - `ArgumentNullException`: If `claimType` is `null`.

### `public static bool HasClaim(HttpContext context, string claimType)`

Determines whether the authenticated user has the specified claim type. Returns `false` if the user is not authenticated.

- **Parameters**
  - `claimType`: The type of the claim to check.
- **Throws**
  - `ArgumentNullException`: If `claimType` is `null`.

### `public static string? GetClientIpAddress(HttpContext context)`

Retrieves the client's IP address from the request headers. Checks `X-Forwarded-For`, `X-Real-IP`, and falls back to `HttpContext.Connection.RemoteIpAddress`. Returns `null` if no valid IP address can be determined.

### `public static string? GetBearerToken(HttpContext context)`

Retrieves the bearer token from the `Authorization` header. Returns `null` if the header is missing, malformed, or does not use the Bearer scheme.

### `public static string? GetApiKey(HttpContext context)`

Retrieves the API key from the `X-API-Key` header. Returns `null` if the header is missing or empty.

### `public static string? GetUserAgent(HttpContext context)`

Retrieves the user agent string from the `User-Agent` header. Returns `null` if the header is missing.

### `public static string GetContentType(HttpContext context)`

Retrieves the content type of the request from the `Content-Type` header. Returns an empty string if the header is missing.

### `public static bool IsSecureConnection(HttpContext context)`

Determines whether the current connection is secure (HTTPS). Returns `true` if the connection is secure; otherwise, `false`.

### `public static string GetFullUrl(HttpContext context)`

Constructs the full URL of the current request, including scheme, host, path, and query string. Returns a string representation of the full URL.

### `public static void SetResponseHeader(HttpContext context, string name, string value)`

Sets a response header with the specified name and value. Overwrites any existing header with the same name.

- **Parameters**
  - `name`: The name of the header.
  - `value`: The value of the header.
- **Throws**
  - `ArgumentNullException`: If `name` or `value` is `null`.

### `public static void AddResponseHeader(HttpContext context, string name, string value)`

Adds a response header with the specified name and value. Does not overwrite existing headers with the same name.

- **Parameters**
  - `name`: The name of the header.
  - `value`: The value of the header.
- **Throws**
  - `ArgumentNullException`: If `name` or `value` is `null`.

### `public static void SetResponseContentType(HttpContext context, string contentType)`

Sets the `Content-Type` response header to the specified value.

- **Parameters**
  - `contentType`: The content type to set.
- **Throws**
  - `ArgumentNullException`: If `contentType` is `null`.

### `public static bool AcceptsJson(HttpContext context)`

Determines whether the client accepts JSON responses by inspecting the `Accept` header. Returns `true` if `application/json` is explicitly accepted; otherwise, `false`.

### `public static bool IsFromBrowser(HttpContext context)`

Determines whether the request originated from a browser by inspecting the `User-Agent` header for common browser identifiers. Returns `true` if a browser user agent is detected; otherwise, `false`.

## Usage

### Example 1: Accessing user identity and IP address
