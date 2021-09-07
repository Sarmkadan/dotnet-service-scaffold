#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace DotnetServiceScaffold.Shared.Extensions;

/// <summary>
/// System.Text.Json extension methods for Exception types to enable serialization
/// and deserialization of exception-related data.
/// </summary>
public static class ExceptionExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an exception to a JSON string.
    /// </summary>
    /// <param name="exception">The exception to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the exception</returns>
    public static string ToJson(this Exception exception, bool indented = false)
    {
        if (exception is null)
        {
            return "{}";
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(exception, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an Exception instance.
    /// Note: This creates a generic Exception with serialized properties, not the original exception type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>An Exception instance with serialized properties, or null if deserialization fails</returns>
    public static Exception? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Exception>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an Exception instance.
    /// Note: This creates a generic Exception with serialized properties, not the original exception type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="exception">Output parameter containing the deserialized exception, or null if failed</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    public static bool TryFromJson(string json, out Exception? exception)
    {
        exception = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            exception = JsonSerializer.Deserialize<Exception>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}