#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Extension methods for <see cref="JsonResponseFormatter"/> that provide
/// additional convenience helpers while preserving the original formatter's
/// behaviour and configuration.
/// </summary>
public static class JsonResponseFormatterExtensions
{
    /// <summary>
    /// Formats the supplied <paramref name="data"/> synchronously using the
    /// formatter's asynchronous <c>FormatAsync</c> method.
    /// </summary>
    /// <param name="formatter">The formatter instance.</param>
    /// <param name="data">The object to serialize; may be <c>null</c>.</param>
    /// <returns>The JSON string representation of <paramref name="data"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Propagated from <see cref="JsonResponseFormatter.FormatAsync"/> if serialization fails.</exception>
    public static string Format(this JsonResponseFormatter formatter, object? data)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        // Synchronously wait for the async implementation – this is safe because the
        // underlying method is CPU‑bound and does not perform I/O.
        return formatter.FormatAsync(data).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Attempts to format <paramref name="data"/> without throwing. Returns
    /// <c>true</c> on success and supplies the resulting JSON via <paramref name="result"/>.
    /// </summary>
    /// <param name="formatter">The formatter instance.</param>
    /// <param name="data">The object to serialize; may be <c>null</c>.</param>
    /// <param name="result">When the method returns <c>true</c>, contains the JSON string; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if formatting succeeded; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is <c>null</c>.</exception>
    public static bool TryFormat(this JsonResponseFormatter formatter, object? data, out string? result)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        try
        {
            result = formatter.Format(data);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Formats <paramref name="data"/> using a custom <see cref="JsonSerializerOptions"/>
    /// instance while still reusing the formatter's core logic.
    /// </summary>
    /// <param name="formatter">The formatter instance.</param>
    /// <param name="data">The object to serialize; may be <c>null</c>.</param>
    /// <param name="options">The serializer options to apply.</param>
    /// <returns>The JSON string produced with the supplied <paramref name="options"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formatter"/> or <paramref name="options"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">Propagated if serialization fails.</exception>
    public static string FormatWithOptions(this JsonResponseFormatter formatter, object? data, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(options);

        if (data is null)
            return "null";

        try
        {
            return JsonSerializer.Serialize(data, options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to serialize object to JSON with custom options", ex);
        }
    }

    /// <summary>
    /// Ensures that the formatter can handle the specified <paramref name="mediaType"/>.
    /// Throws <see cref="InvalidOperationException"/> if the media type is unsupported.
    /// </summary>
    /// <param name="formatter">The formatter instance.</param>
    /// <param name="mediaType">The media type to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="mediaType"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the media type cannot be formatted.</exception>
    public static void EnsureCanFormat(this JsonResponseFormatter formatter, string mediaType)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentException.ThrowIfNullOrEmpty(mediaType);

        if (!formatter.CanFormat(mediaType))
            throw new InvalidOperationException(
                $"Media type '{mediaType}' is not supported by {nameof(JsonResponseFormatter)}.");
    }

    /// <summary>
    /// Returns a read‑only collection of the media‑type patterns that the formatter
    /// recognises. The collection is static and reflects the logic inside
    /// <see cref="JsonResponseFormatter.CanFormat(string)"/>.
    /// </summary>
    /// <param name="formatter">The formatter instance.</param>
    /// <returns>An <see cref="IReadOnlyList{String}"/> containing supported patterns.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> GetSupportedMediaTypePatterns(this JsonResponseFormatter formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        // The patterns are derived from the implementation of CanFormat.
        return new[]
        {
            "application/json",
            "application/json+*",
            "*+json"
        };
    }
}
