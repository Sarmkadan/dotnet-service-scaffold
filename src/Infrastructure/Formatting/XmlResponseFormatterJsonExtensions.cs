#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Globalization;

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for
/// <see cref="XmlResponseFormatter"/>.
/// </summary>
/// <remarks>
/// <see cref="XmlResponseFormatter"/> is a stateless formatter that uses empty XML namespaces.
/// This class serializes a marker object to enable consistent JSON handling of formatter instances
/// across the application.
/// </remarks>
public static class XmlResponseFormatterJsonExtensions
{
    /// <summary>
    /// Gets the shared JsonSerializerOptions configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes an <see cref="XmlResponseFormatter"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The formatter instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the formatter instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this XmlResponseFormatter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(new FormatterMarker(), options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="XmlResponseFormatter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized formatter instance if successful; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static XmlResponseFormatter? FromJson(string? json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            var marker = JsonSerializer.Deserialize<FormatterMarker>(json, JsonOptions);
            return marker == null ? null : new XmlResponseFormatter();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="XmlResponseFormatter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized formatter instance if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string? json, out XmlResponseFormatter? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            var marker = JsonSerializer.Deserialize<FormatterMarker>(json, JsonOptions);
            value = marker == null ? null : new XmlResponseFormatter();
            return marker != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Marker class used to indicate an XmlResponseFormatter instance during serialization.
    /// </summary>
    private sealed class FormatterMarker
    {
    }
}