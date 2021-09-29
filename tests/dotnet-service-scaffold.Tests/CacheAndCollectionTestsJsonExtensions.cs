#nullable enable

using System.Text.Json;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="CacheAndCollectionTests"/>.
/// </summary>
public static class CacheAndCollectionTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private static JsonSerializerOptions GetJsonOptions(bool indented)
    {
        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented,
        };
        return options;
    }

    /// <summary>
    /// Serializes a <see cref="CacheAndCollectionTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this CacheAndCollectionTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, GetJsonOptions(indented));
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CacheAndCollectionTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="CacheAndCollectionTests"/> instance, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static CacheAndCollectionTests? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<CacheAndCollectionTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="CacheAndCollectionTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized value if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out CacheAndCollectionTests? value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<CacheAndCollectionTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}