// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.RegularExpressions;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Utility class for string operations. Provides helpers for common string manipulations
/// like truncation, slug generation, case conversion, and validation. Thread-safe.
/// </summary>
public static class StringUtility
{
    /// <summary>
    /// Truncates a string to a specified length and adds an ellipsis if truncated.
    /// </summary>
    public static string Truncate(string? text, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        if (text.Length <= maxLength)
            return text;

        var truncateLength = maxLength - suffix.Length;
        return text[..Math.Max(0, truncateLength)] + suffix;
    }

    /// <summary>
    /// Converts a string to a URL-friendly slug. Removes special characters,
    /// converts to lowercase, and replaces spaces with hyphens.
    /// </summary>
    public static string ToSlug(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert to lowercase and remove accents
        var bytes = Encoding.UTF8.GetBytes(text);
        text = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetString(bytes));

        // Remove invalid characters
        text = Regex.Replace(text, @"[^\w\s-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.0));

        // Convert multiple spaces to single hyphen
        text = Regex.Replace(text, @"[\s-]+", "-", RegexOptions.None, TimeSpan.FromSeconds(1.0));

        // Trim hyphens from start and end
        return text.Trim('-').ToLowerInvariant();
    }

    /// <summary>
    /// Converts a string from camelCase to PascalCase or snake_case.
    /// </summary>
    public static string ToSnakeCase(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var result = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && i > 0)
            {
                result.Append('_');
            }
            result.Append(char.ToLowerInvariant(text[i]));
        }
        return result.ToString();
    }

    /// <summary>
    /// Converts a string from snake_case to camelCase.
    /// </summary>
    public static string ToCamelCase(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var parts = text.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return string.Empty;

        var result = new StringBuilder(parts[0].ToLowerInvariant());
        for (int i = 1; i < parts.Length; i++)
        {
            result.Append(char.ToUpperInvariant(parts[i][0]));
            result.Append(parts[i][1..].ToLowerInvariant());
        }
        return result.ToString();
    }

    /// <summary>
    /// Masks sensitive information in a string, keeping only the first and last characters visible.
    /// Useful for logging API keys, tokens, etc.
    /// </summary>
    public static string MaskSensitive(string? text, int visibleChars = 1)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= visibleChars * 2)
            return new string('*', Math.Max(text?.Length ?? 0, 4));

        var masked = text![..visibleChars] + new string('*', text.Length - visibleChars * 2) + text.Substring(text.Length - visibleChars);
        return masked;
    }

    /// <summary>
    /// Generates a random string of specified length using alphanumeric characters.
    /// Useful for generating tokens, session IDs, etc.
    /// </summary>
    public static string GenerateRandomString(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Checks if a string is a valid email address using a simple regex pattern.
    /// For strict validation, use a library like FluentValidation.
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Strips all HTML tags from a string, leaving only text content.
    /// Useful for sanitizing user-provided content.
    /// </summary>
    public static string StripHtmlTags(string? html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        var pattern = @"<[^>]+>";
        return Regex.Replace(html, pattern, string.Empty);
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// </summary>
    public static string Repeat(string? text, int count)
    {
        if (string.IsNullOrEmpty(text) || count <= 0)
            return string.Empty;

        var result = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            result.Append(text);
        }
        return result.ToString();
    }
}
