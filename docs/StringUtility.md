# StringUtility
The `StringUtility` class provides a set of static methods for performing common string operations, such as truncation, case conversion, and validation. These methods can be used to simplify string manipulation tasks and improve code readability.

## API
* `public static string Truncate(string value, int length)`: Truncates a string to the specified length. Returns the truncated string. Throws `ArgumentNullException` if `value` is null.
* `public static string ToSlug(string value)`: Converts a string to a slug (a string that can be used in a URL). Returns the slug. Throws `ArgumentNullException` if `value` is null.
* `public static string ToSnakeCase(string value)`: Converts a string to snake case (a string with words separated by underscores). Returns the converted string. Throws `ArgumentNullException` if `value` is null.
* `public static string ToCamelCase(string value)`: Converts a string to camel case (a string with the first word in lower case and the first letter of each subsequent word capitalized). Returns the converted string. Throws `ArgumentNullException` if `value` is null.
* `public static string MaskSensitive(string value, int length)`: Masks a sensitive string by replacing characters with asterisks, except for the specified number of characters at the end. Returns the masked string. Throws `ArgumentNullException` if `value` is null.
* `public static string GenerateRandomString(int length)`: Generates a random string of the specified length. Returns the generated string.
* `public static bool IsValidEmail(string email)`: Checks if a string is a valid email address. Returns true if the string is a valid email address, false otherwise. Throws `ArgumentNullException` if `email` is null.
* `public static string StripHtmlTags(string value)`: Removes HTML tags from a string. Returns the string without HTML tags. Throws `ArgumentNullException` if `value` is null.
* `public static string Repeat(string value, int count)`: Repeats a string the specified number of times. Returns the repeated string. Throws `ArgumentNullException` if `value` is null.

## Usage
```csharp
// Example 1: Truncating a string and converting to slug
string originalString = "This is a very long string that needs to be truncated";
string truncatedString = StringUtility.Truncate(originalString, 20);
string slug = StringUtility.ToSlug(truncatedString);
Console.WriteLine(slug);

// Example 2: Validating an email address and generating a random string
string email = "example@example.com";
if (StringUtility.IsValidEmail(email))
{
    string randomString = StringUtility.GenerateRandomString(10);
    Console.WriteLine($"Email is valid. Random string: {randomString}");
}
else
{
    Console.WriteLine("Email is not valid");
}
```

## Notes
The `StringUtility` class is designed to be thread-safe, as all methods are static and do not rely on any shared state. However, it is worth noting that some methods, such as `GenerateRandomString`, may not be suitable for use in cryptographic applications due to their reliance on the `Random` class. Additionally, the `IsValidEmail` method uses a simple regular expression to validate email addresses, which may not cover all possible valid email address formats. Edge cases, such as null or empty input strings, are handled by throwing `ArgumentNullException` or returning default values, as specified in the API documentation.
