// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Utility class for input validation. Provides helpers for common validation scenarios
/// like URL validation, phone numbers, passwords, etc. Throws ArgumentException on validation failure.
/// </summary>
public static class ValidationUtility
{
    /// <summary>
    /// Validates that a string is not null or empty. Throws ArgumentException if invalid.
    /// </summary>
    public static void ValidateNotNullOrEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
    }

    /// <summary>
    /// Validates that a value is within a specified range. Throws ArgumentException if invalid.
    /// </summary>
    public static void ValidateRange<T>(T value, T min, T max, string paramName) where T : IComparable<T>
    {
        if (value == null)
            throw new ArgumentNullException(paramName); // Fix: handle null input

        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must be between {min} and {max}.");
    }

    /// <summary>
    /// Validates that a string length is within specified bounds.
    /// </summary>
    public static void ValidateLength(string? value, int minLength, int maxLength, string paramName)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);

        if (value.Length < minLength || value.Length > maxLength)
            throw new ArgumentException(
                $"{paramName} length must be between {minLength} and {maxLength}, but was {value.Length}", paramName);
    }

    /// <summary>
    /// Validates a password meets minimum security requirements.
    /// Requires: at least 8 characters, one uppercase, one lowercase, one digit, one special char.
    /// </summary>
    public static bool IsPasswordStrong(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        if (password.Length < 8)
            return false;

        var hasUpper = Regex.IsMatch(password, "[A-Z]");
        var hasLower = Regex.IsMatch(password, "[a-z]");
        var hasDigit = Regex.IsMatch(password, @"\d");
        var hasSpecial = Regex.IsMatch(password, @"[!@#$%^&*()_\-+=\[\]{};:'""<>,.?/\\|`~]");

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    /// <summary>
    /// Validates a URL is properly formatted.
    /// </summary>
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a phone number using a simple pattern. Accepts +1-234-567-8900 format.
    /// </summary>
    public static bool IsValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var pattern = @"^\+?[1-9]\d{1,14}$";
        return Regex.IsMatch(phone.Replace(" ", "").Replace("-", ""), pattern);
    }

    /// <summary>
    /// Validates an email address using a basic pattern.
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            if (!Regex.IsMatch(email, pattern))
                return false;

            new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a UUID/GUID string.
    /// </summary>
    public static bool IsValidGuid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Validates an IP address (IPv4 or IPv6).
    /// </summary>
    public static bool IsValidIpAddress(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;

        return System.Net.IPAddress.TryParse(ip, out _);
    }

    /// <summary>
    /// Validates a JSON string can be parsed.
    /// </summary>
    public static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a collection is not null or empty.
    /// </summary>
    public static void ValidateCollectionNotEmpty<T>(IEnumerable<T>? collection, string paramName)
    {
        if (collection == null || !collection.Any())
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
    }

    /// <summary>
    /// Validates that a value matches a regex pattern.
    /// </summary>
    public static bool MatchesPattern(string? value, string pattern)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        try
        {
            return Regex.IsMatch(value, pattern, RegexOptions.None, TimeSpan.FromSeconds(1.0));
        }
        catch
        {
            return false;
        }
    }
}
