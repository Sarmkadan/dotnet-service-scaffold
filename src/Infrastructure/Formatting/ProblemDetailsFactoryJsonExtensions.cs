using System;
using System.Text.Json;
using System.Globalization;

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Provides JSON extension methods for <see cref="ProblemDetails"/>.
/// </summary>
public static class ProblemDetailsFactoryJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="ProblemDetails"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ProblemDetails"/> to serialize.</param>
    /// <param name="indented">Whether to use indented formatting.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ProblemDetails value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized <see cref="ProblemDetails"/> object, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static ProblemDetails? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<ProblemDetails>(json, _options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="value">The deserialized <see cref="ProblemDetails"/> object if successful; otherwise, null.</param>
    /// <returns>True if deserialization was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out ProblemDetails? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ProblemDetails>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
