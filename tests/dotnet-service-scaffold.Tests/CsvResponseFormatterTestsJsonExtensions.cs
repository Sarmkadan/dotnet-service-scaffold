using System.Text.Json;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// System.Text.Json serialization helpers for <see cref="CsvResponseFormatterTests"/>.
/// </summary>
public static class CsvResponseFormatterTestsJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Serializes this instance to a JSON string using camelCase property naming.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">When <c>true</c>, the JSON is formatted with indentation.</param>
    /// <returns>The JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this CsvResponseFormatterTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? IndentedOptions : Options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="CsvResponseFormatterTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or <c>null</c> when <paramref name="json"/> is <c>null</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException"><paramref name="json"/> is not valid JSON for the target type.</exception>
    public static CsvResponseFormatterTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<CsvResponseFormatterTests>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="CsvResponseFormatterTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized instance, or <c>null</c> when deserialization fails.</param>
    /// <returns><c>true</c> when deserialization succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryFromJson(string json, out CsvResponseFormatterTests? value)
    {
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}