using System;
using System.Text.Json;

namespace Infrastructure.Formatting;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="JsonResponseFormatterExtensions"/>.
/// </summary>
public static class JsonResponseFormatterExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the specified <paramref name="value"/> to a JSON string.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output JSON will be indented.</param>
    /// <returns>A JSON string representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this JsonResponseFormatterExtensions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes the specified JSON string to an instance of <see cref="JsonResponseFormatterExtensions"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An instance of <see cref="JsonResponseFormatterExtensions"/> if deserialization succeeds; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static JsonResponseFormatterExtensions? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<JsonResponseFormatterExtensions>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize the specified JSON string to an instance of <see cref="JsonResponseFormatterExtensions"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized value if the operation succeeded, or <c>null</c> otherwise.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
    public static bool TryFromJson(string json, out JsonResponseFormatterExtensions? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<JsonResponseFormatterExtensions>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
