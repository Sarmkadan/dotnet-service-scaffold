// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.Globalization;
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
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && !char.IsControl(c))
            {
                stringBuilder.Append(c);
            }
        }
        text = stringBuilder.ToString();
        var asciiBytes = Encoding.ASCII.GetBytes(text);
        text = Encoding.ASCII.GetString(asciiBytes);

        // Remove invalid characters
        text = Regex.Replace(text, @"[^\w\s-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.0));

        // Convert multiple spaces to single hyphen
        text = Regex.Replace(text, @"[\s-]+", "-", RegexOptions.None, TimeSpan.FromSeconds(1.0));

        // Trim hyphens from start and end
        return text.Trim('-').ToLowerInvariant();
    }

    /// <summary>
    /// Converts a string from camelCase or PascalCase to snake_case.
    /// Uses string.Create to write directly into the result buffer — zero intermediate allocations.
    /// </summary>
    public static string ToSnakeCase(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Count uppercase chars after position 0 to know the exact output length upfront.
        int extraChars = 0;
        for (int i = 1; i < text.Length; i++)
            if (char.IsUpper(text[i])) extraChars++;

        if (extraChars == 0)
            return text.ToLowerInvariant();

        return string.Create(text.Length + extraChars, text, static (span, src) =>
        {
            int pos = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (char.IsUpper(src[i]) && i > 0)
                    span[pos++] = '_';
                span[pos++] = char.ToLowerInvariant(src[i]);
            }
        });
    }

    /// <summary>
    /// Converts a string from snake_case to camelCase.
    /// Uses string.Create to avoid intermediate StringBuilder allocation.
    /// </summary>
    public static string ToCamelCase(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Output is never longer than input (underscores are removed).
        return string.Create(text.Length, text, static (span, src) =>
        {
            int pos = 0;
            bool nextUpper = false;

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == '_')
                {
                    nextUpper = pos > 0; // skip leading underscores without capitalising
                    continue;
                }

                span[pos++] = nextUpper
                    ? char.ToUpperInvariant(src[i])
                    : char.ToLowerInvariant(src[i]);

                nextUpper = false;
            }

            // Trim the span to the actual written length if underscores were removed.
            if (pos < span.Length)
                span[pos..].Clear();
        }).TrimEnd('\0');
    }

    /// <summary>
    /// Masks sensitive information in a string, keeping only the first and last characters visible.
    /// Useful for logging API keys, tokens, etc.
    /// Uses string.Create — single allocation, no intermediate strings.
    /// </summary>
    public static string MaskSensitive(string? text, int visibleChars = 1)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= visibleChars * 2)
            return new string('*', Math.Max(text?.Length ?? 0, 4));

        int maskLen = text.Length - visibleChars * 2;
        return string.Create(text.Length, (text, visibleChars, maskLen), static (span, state) =>
        {
            var (src, visible, mask) = state;
            src.AsSpan(0, visible).CopyTo(span);
            span.Slice(visible, mask).Fill('*');
            src.AsSpan(src.Length - visible).CopyTo(span.Slice(visible + mask));
        });
    }

    /// <summary>
    /// Generates a random string of specified length using alphanumeric characters.
    /// Useful for generating tokens, session IDs, etc.
    /// Uses ArrayPool&lt;char&gt; to avoid heap allocation for the working buffer.
    /// </summary>
    public static string GenerateRandomString(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var buffer = ArrayPool<char>.Shared.Rent(length);
        try
        {
            var random = Random.Shared;
            for (int i = 0; i < length; i++)
                buffer[i] = chars[random.Next(chars.Length)];
            return new string(buffer, 0, length);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
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
