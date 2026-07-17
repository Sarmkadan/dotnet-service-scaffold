# HttpUtilityValidation

Provides static validation methods for common HTTP-related configuration values such as authentication credentials, tokens, URLs, headers, content types, status codes, and retry policies. Each `Validate*` method returns a read-only list of validation error messages (empty when valid), while each `IsValid*` method returns a boolean indicating validity. This enables consistent, centralized validation of HTTP settings across service scaffolding and configuration binding scenarios.

## API

### ValidateBasicAuth
```csharp
public static IReadOnlyList<string> ValidateBasicAuth(string username, string password)
```
Validates a basic authentication credential pair. Returns a list of error messages; an empty list indicates both `username` and `password` are valid. Delegates to `ValidateBasicAuthUsername` and `ValidateBasicAuthPassword` internally.

### ValidateBasicAuthUsername
```csharp
public static IReadOnlyList<string> ValidateBasicAuthUsername(string username)
```
Validates a basic authentication username. Returns a list of error messages; an empty list indicates the username is valid. Typical checks include non-null, non-empty, and absence of prohibited characters.

### ValidateBasicAuthPassword
```csharp
public static IReadOnlyList<string> ValidateBasicAuthPassword(string password)
```
Validates a basic authentication password. Returns a list of error messages; an empty list indicates the password is valid. Typical checks include non-null and non-empty.

### ValidateBearerToken
```csharp
public static IReadOnlyList<string> ValidateBearerToken(string token)
```
Validates a bearer token string. Returns a list of error messages; an empty list indicates the token is valid. Typical checks include non-null, non-empty, and well-formed token structure.

### ValidateStatusCode
```csharp
public static IReadOnlyList<string> ValidateStatusCode(int statusCode)
```
Validates an HTTP status code integer. Returns a list of error messages; an empty list indicates the status code is within the expected range (typically 100–599).

### ValidateBaseUrl
```csharp
public static IReadOnlyList<string> ValidateBaseUrl(string baseUrl)
```
Validates a base URL string. Returns a list of error messages; an empty list indicates the URL is well-formed, absolute, and uses an allowed scheme.

### ValidatePath
```csharp
public static IReadOnlyList<string> ValidatePath(string path)
```
Validates a relative URL path segment. Returns a list of error messages; an empty list indicates the path is syntactically valid and does not contain illegal characters.

### ValidateQueryParameters
```csharp
public static IReadOnlyList<string> ValidateQueryParameters(string queryParameters)
```
Validates a raw query string (the portion after `?`). Returns a list of error messages; an empty list indicates the query string is properly formatted or empty/null is acceptable.

### ValidateContentType
```csharp
public static IReadOnlyList<string> ValidateContentType(string contentType)
```
Validates a media type (MIME) string such as `application/json`. Returns a list of error messages; an empty list indicates the content type is well-formed per RFC 2045.

### ValidateHeader
```csharp
public static IReadOnlyList<string> ValidateHeader(string name, string value)
```
Validates an HTTP header name-value pair. Returns a list of error messages; an empty list indicates both the header name and value conform to HTTP header specifications (e.g., no prohibited characters, non-empty name).

### ValidateRetryAttempt
```csharp
public static IReadOnlyList<string> ValidateRetryAttempt(int attempt)
```
Validates a retry attempt count. Returns a list of error messages; an empty list indicates the attempt number is a non-negative integer within a reasonable upper bound.

### IsValidBasicAuth
```csharp
public static bool IsValidBasicAuth(string username, string password)
```
Returns `true` when both `username` and `password` pass validation; otherwise `false`. Convenience wrapper around `ValidateBasicAuth`.

### IsValidBearerToken
```csharp
public static bool IsValidBearerToken(string token)
```
Returns `true` when the bearer token passes validation; otherwise `false`.

### IsValidStatusCode
```csharp
public static bool IsValidStatusCode(int statusCode)
```
Returns `true` when the status code falls within the valid HTTP range; otherwise `false`.

### IsValidBaseUrl
```csharp
public static bool IsValidBaseUrl(string baseUrl)
```
Returns `true` when the base URL is well-formed and absolute; otherwise `false`.

### IsValidPath
```csharp
public static bool IsValidPath(string path)
```
Returns `true` when the path segment is syntactically valid; otherwise `false`.

### IsValidQueryParameters
```csharp
public static bool IsValidQueryParameters(string queryParameters)
```
Returns `true` when the query string is valid or acceptably absent; otherwise `false`.

### IsValidContentType
```csharp
public static bool IsValidContentType(string contentType)
```
Returns `true` when the content type string is a well-formed media type; otherwise `false`.

### IsValidHeader
```csharp
public static bool IsValidHeader(string name, string value)
```
Returns `true` when both the header name and value are valid; otherwise `false`.

### IsValidRetryAttempt
```csharp
public static bool IsValidRetryAttempt(int attempt)
```
Returns `true` when the retry attempt count is non-negative and within bounds; otherwise `false`.

## Usage

### Example 1: Validating configuration before registering an HTTP client

```csharp
var baseUrlErrors = HttpUtilityValidation.ValidateBaseUrl(config.BaseUrl);
var tokenErrors = HttpUtilityValidation.ValidateBearerToken(config.BearerToken);
var allErrors = baseUrlErrors.Concat(tokenErrors).ToList();

if (allErrors.Any())
{
    throw new InvalidOperationException(
        $"HTTP client configuration invalid: {string.Join("; ", allErrors)}");
}

services.AddHttpClient("MyApi", client =>
{
    client.BaseAddress = new Uri(config.BaseUrl);
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", config.BearerToken);
});
```

### Example 2: Feature toggle with boolean validators

```csharp
if (HttpUtilityValidation.IsValidBasicAuth(username, password))
{
    var credential = Convert.ToBase64String(
        Encoding.ASCII.GetBytes($"{username}:{password}"));
    request.Headers.Authorization =
        new AuthenticationHeaderValue("Basic", credential);
}
else if (HttpUtilityValidation.IsValidBearerToken(token))
{
    request.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
}
else
{
    throw new ArgumentException("No valid authentication provided.");
}
```

## Notes

- All `Validate*` methods return an empty list when the input is valid; never `null`. This allows safe enumeration and LINQ composition without null checks.
- The `IsValid*` methods are functionally equivalent to checking `!Validate*().Any()` but offer a more concise syntax for boolean branching.
- These methods perform purely syntactic and range validation. They do not verify that a URL is reachable, a token is currently active, or a content type is registered with the server.
- Inputs that are `null` or empty typically produce one or more error messages; the exact messages depend on the validation rules for each member.
- All members are static and stateless, making them safe to call concurrently from any thread without synchronization.
