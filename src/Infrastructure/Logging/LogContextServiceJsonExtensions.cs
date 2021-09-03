#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for
/// <see cref="LogContextService"/> to enable JSON conversion of log context properties.
/// </summary>
public static class LogContextServiceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="LogContextService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The log context service to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the log context service.</returns>
    public static string ToJson(this LogContextService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value.GetProperties(), options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="LogContextService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A new <see cref="LogContextService"/> instance populated with the deserialized properties, or null if the JSON is null or empty.</returns>
    public static LogContextService? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, _jsonOptions);
        if (dictionary is null)
        {
            return null;
        }

        var service = new LogContextService();
        foreach (var (key, value) in dictionary)
        {
            service.AddProperty(key, value);
        }

        return service;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="LogContextService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized service if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out LogContextService? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}