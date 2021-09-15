# ValidationUtility

Utility class containing static methods for common validation checks. It provides a centralized way to validate arguments, strings, collections, and formatted data, throwing appropriate exceptions or returning boolean results as needed.

## API

### ValidateNotNullOrEmpty
```csharp
public static void ValidateNotNullOrEmpty(string value, string paramName = null)
```
- **Purpose**: Ensures that a string is neither `null` nor empty (`""`).
- **Parameters**:
  - `value`: The string to validate.
  - `paramName`: Optional name of the parameter being validated; used in exception messages.
- **Return value**: None.
- **Exceptions**:
  - `ArgumentNullException` if `value` is `null`.
  - `ArgumentException` if `value` is `String.Empty`.

### ValidateRange<T>
```csharp
public static void ValidateRange<T>(T value, T min, T max, string paramName = null) where T : IComparable<T>
```
- **Purpose**: Verifies that a comparable value falls within an inclusive range.
- **Parameters**:
  - `value`: The value to test.
  - `min`: The inclusive lower bound.
  - `max`: The inclusive upper bound.
  - `paramName`: Optional parameter name for exception messages.
- **Return value**: None.
- **Exceptions**:
  - `ArgumentOutOfRangeException` if `value` is less than `min` or greater than `max`.

### ValidateLength
```csharp
public static void ValidateLength(string value, int minLength, int maxLength, string paramName = null)
```
- **Purpose**: Checks that a string's length is between `minLength` and `maxLength` inclusive.
- **Parameters**:
  - `value`: The string to validate (may be `null`; null is treated as length 0).
  - `minLength`: Minimum allowed length.
  - `maxLength`: Maximum allowed length.
  - `paramName`: Optional parameter name for exception messages.
- **Return value**: None.
- **Exceptions**:
  - `ArgumentException` if the length of `value` is outside the specified range.

### IsPasswordStrong
```csharp
public static bool IsPasswordStrong(string password, int minLength = 8)
```
- **Purpose**: Evaluates password strength based on length and character variety.
- **Parameters**:
  - `password`: The password string to evaluate.
  - `minLength`: Minimum required length (default 8).
- **Return value**: `true` if the password meets the criteria (length ≥ `minLength`, contains at least one uppercase letter, one lowercase letter, one digit, and one non‑alphanumeric character); otherwise `false`.
- **Exceptions**: None.

### IsValidUrl
```csharp
public static bool IsValidUrl(string uri)
```
- **Purpose**: Determines whether a string is a well-formed absolute URI.
- **Parameters**:
  - `uri`: The URI string to validate.
- **Return value**: `true` if `uri` can be parsed by `Uri.TryCreate` with `UriKind.Absolute`; otherwise `false`.
- **Exceptions**: None.

### IsValidPhoneNumber
```csharp
public static bool IsValidPhoneNumber(string phoneNumber)
```
- **Purpose**: Checks if a string matches a common phone number pattern (e.g., US format with optional country code).
- **Parameters**:
  - `phoneNumber`: The phone number string to validate.
- **Return value**: `true` if the string conforms to the pattern; otherwise `false`.
- **Exceptions**: None.

### IsValidEmail
```csharp
public static bool IsValidEmail(string email)
```
- **Purpose**: Validates an email address using a regular expression.
- **Parameters**:
  - `email`: The email address string to validate.
- **Return value**: `true` if the string matches the email pattern; otherwise `false`.
- **Exceptions**: None.

### IsValidGuid
```csharp
public static bool IsValidGuid(string guid)
```
- **Purpose**: Determines whether a string represents a valid GUID.
- **Parameters**:
  - `guid`: The GUID string to validate.
- **Return value**: `true` if `Guid.TryParse` succeeds; otherwise `false`.
- **Exceptions**: None.

### IsValidIpAddress
```csharp
public static bool IsValidIpAddress(string ip)
```
- **Purpose**: Checks if a string is a valid IPv4 or IPv6 address.
- **Parameters**:
  - `ip`: The IP address string to validate.
- **Return value**: `true` if `IPAddress.TryParse` succeeds; otherwise `false`.
- **Exceptions**: None.

### IsValidJson
```csharp
public static bool IsValidJson(string json)
```
- **Purpose**: Verifies that a string contains valid JSON.
- **Parameters**:
  - `json`: The JSON string to validate.
- **Return value**: `true` if the string can be parsed by a JSON parser (e.g., `JsonDocument.Parse`) without throwing; otherwise `false`.
- **Exceptions**: None.

### ValidateCollectionNotEmpty<T>
```csharp
public static void ValidateCollectionNotEmpty<T>(IEnumerable<T> collection, string paramName = null)
```
- **Purpose**: Ensures that a collection is not `null` and contains at least one element.
- **Parameters**:
  - `collection`: The collection to validate.
  - `paramName`: Optional parameter name for exception messages.
- **Return value**: None.
- **Exceptions**:
  - `ArgumentNullException` if `collection` is `null`.
  - `ArgumentException` if the collection contains no elements.

### MatchesPattern
```csharp
public static bool MatchesPattern(string input, string pattern)
```
- **Purpose**: Tests whether an input matchesPattern(string input, string pattern)
```
- **Purpose**: Determines if an input string matches a specified regular expression.
- **Parameters**:
  - `input`: The string to test.
  - `pattern`: The regular expression pattern.
- **Return value**: `true` if `Regex.IsMatch(input, pattern)` returns true; otherwise `false`.
- **Exceptions**: None (throws `ArgumentException` if `pattern` is invalid, propagated from `Regex`).

## Usage

```csharp
using MyProject.Utilities;

// Validate a user‑supplied name before processing
public void RegisterUser(string userName, string email)
{
    ValidationUtility.ValidateNotNullOrEmpty(userName, nameof(userName));
    if (!ValidationUtility.IsValidEmail(email))
    {
        throw new ArgumentException("Invalid e‑mail address.", nameof(email));
    }
    // Proceed with registration...
}
```

```csharp
using MyProject.Utilities;

// Ensure a configuration value lies within an allowed range
public void ConfigureTimeout(int timeoutSeconds)
{
    ValidationUtility.ValidateRange(timeoutSeconds, 5, 300, nameof(timeoutSeconds));
    // Apply the timeout setting...
}
```

## Notes

- All methods are stateless and thread‑safe; they rely only on their inputs and do not modify shared state.
- String‑based validators treat a `null` input as invalid unless explicitly documented otherwise (e.g., `ValidateLength` treats `null` as length 0).
- Culture‑sensitive checks (e.g., email, phone number) use the invariant culture or language‑agnostic regular expressions; results may vary for locale‑specific formats.
- Regular expression‑based methods (`IsValidEmail`, `IsValidPhoneNumber`, `MatchesPattern`) may throw an `ArgumentException` if the supplied pattern is malformed; callers should ensure patterns are valid.
- For `ValidateCollectionNotEmpty<T>`, enumerating the collection may cause side effects if the enumerable is not repeatable (e.g., a one‑time stream); the method attempts to evaluate the collection only once.
