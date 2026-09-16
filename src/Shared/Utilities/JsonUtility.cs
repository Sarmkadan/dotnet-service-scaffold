#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Utility class for JSON operations. Provides helpers for parsing, serializing,
/// and manipulating JSON data with consistent options across the application.
/// </summary>
public static class JsonUtility
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Web-optimized JSON serializer options (camelCase, case-insensitive, allows trailing commas, numbers from strings).
    /// </summary>
    private static readonly JsonSerializerOptions WebDefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        AllowTrailingCommas = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Serializes an object to JSON string using default options.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A JSON string representation of <paramref name="obj"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
    public static string Serialize<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return JsonSerializer.Serialize(obj, DefaultOptions);
    }

    /// <summary>
    /// Serializes an object to JSON string using web-optimized options.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A web-optimized JSON string representation of <paramref name="obj"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
    public static string SerializeWeb<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return JsonSerializer.Serialize(obj, WebDefaultOptions);
    }

    /// <summary>
    /// Serializes an object to JSON string with pretty formatting (indented).
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>An indented JSON string representation of <paramref name="obj"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
    public static string SerializePretty<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return JsonSerializer.Serialize(obj, PrettyOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object.
    /// </summary>
    /// <typeparam name="T">The type to which the JSON is deserialized.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized object, or <see langword="null"/> when the JSON represents a null value.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="json"/> is not valid JSON for <typeparamref name="T"/>.</exception>
    public static T? Deserialize<T>(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"{JsonUtilityConstants.DeserializeErrorMessage}{ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes a JSON string to a dynamic object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The dynamically typed JSON value, or <see langword="null"/> when the JSON represents a null value.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="json"/> is not valid JSON.</exception>
    public static dynamic? DeserializeDynamic(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<dynamic>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"{JsonUtilityConstants.DeserializeErrorMessage}{ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extracts a property value from a JSON string without fully deserializing.
    /// Useful for reading specific values from large JSON documents.
    /// </summary>
    /// <typeparam name="T">The type to which the property value is deserialized.</typeparam>
    /// <param name="json">The JSON string to inspect.</param>
    /// <param name="propertyPath">The dot-delimited path of the property to retrieve.</param>
    /// <returns>
    /// The deserialized property value, or the default value of <typeparamref name="T"/> when the path
    /// does not exist or the JSON cannot be parsed or deserialized.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> or <paramref name="propertyPath"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> or <paramref name="propertyPath"/> is <see langword="null"/>.</exception>
    public static T? GetProperty<T>(string json, string propertyPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        ArgumentException.ThrowIfNullOrEmpty(propertyPath);

        try
        {
            using (var doc = JsonDocument.Parse(json))
            {
                var element = doc.RootElement;

                // Support dot notation for nested properties
                foreach (var part in propertyPath.Split('.'))
                {
                    if (element.TryGetProperty(part, out var nextElement))
                    {
                        element = nextElement;
                    }
                    else
                    {
                        return default;
                    }
                }

                return element.Deserialize<T>(DefaultOptions);
            }
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Merges two JSON objects. Properties from the second object override those in the first.
    /// </summary>
    /// <param name="json1">The base JSON string.</param>
    /// <param name="json2">The JSON string whose values override values in <paramref name="json1"/>.</param>
    /// <returns>The merged JSON string.</returns>
    /// <exception cref="ArgumentException"><paramref name="json1"/> or <paramref name="json2"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="json1"/> or <paramref name="json2"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Either argument is not valid JSON.</exception>
    public static string MergeJson(string json1, string json2)
    {
        ArgumentException.ThrowIfNullOrEmpty(json1);
        ArgumentException.ThrowIfNullOrEmpty(json2);

        if (string.IsNullOrEmpty(json1))
            return json2 ?? string.Empty;

        if (string.IsNullOrEmpty(json2))
            return json1;

        try
        {
            var node1 = JsonNode.Parse(json1);
            var node2 = JsonNode.Parse(json2);
            var merged = MergeNodes(node1, node2);
            return merged is null ? "null" : merged.ToJsonString(DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"{JsonUtilityConstants.MergeErrorMessage}{ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates if a string is valid JSON.
    /// </summary>
    /// <param name="json">The string to validate.</param>
    /// <returns><see langword="true"/> if <paramref name="json"/> contains valid JSON; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the JSON type of a value (object, array, string, number, bool, null).
    /// </summary>
    /// <param name="json">The JSON string whose value type is identified.</param>
    /// <returns>The inferred JSON type, or <c>unknown</c> when <paramref name="json"/> is null or empty.</returns>
    public static string GetJsonType(string json)
    {
        if (string.IsNullOrEmpty(json))
            return "unknown";

        var trimmed = json.Trim();

        return trimmed switch
        {
            "null" => "null",
            "true" or "false" => "boolean",
            _ when trimmed.StartsWith("{") => "object",
            _ when trimmed.StartsWith("[") => "array",
            _ when trimmed.StartsWith("\"") => "string",
            _ => "number"
        };
    }

    /// <summary>
    /// Formats a JSON string with consistent indentation.
    /// </summary>
    /// <param name="json">The JSON string to format.</param>
    /// <returns>
    /// The indented JSON string, or the original value when <paramref name="json"/> is null, empty, or cannot be parsed.
    /// </returns>
    public static string FormatJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;

        try
        {
            using (var doc = JsonDocument.Parse(json))
            {
                return JsonSerializer.Serialize(doc, PrettyOptions);
            }
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Recursively merges two JSON nodes. Object properties are merged key by key, with
    /// values from <paramref name="second"/> overriding those in <paramref name="first"/>
    /// (recursively for nested objects). Non-object values (including arrays) from
    /// <paramref name="second"/> take precedence entirely.
    /// </summary>
    private static JsonNode? MergeNodes(JsonNode? first, JsonNode? second)
    {
        if (first is JsonObject firstObject && second is JsonObject secondObject)
        {
            var result = new JsonObject();

            foreach (var property in firstObject)
            {
                result[property.Key] = property.Value?.DeepClone();
            }

            foreach (var property in secondObject)
            {
                result[property.Key] = result.TryGetPropertyValue(property.Key, out var existing)
                    ? MergeNodes(existing, property.Value)
                    : property.Value?.DeepClone();
            }

            return result;
        }

        return second?.DeepClone();
    }
}
