#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
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
    /// Serializes an object to JSON string using default options.
    /// </summary>
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, DefaultOptions);
    }

    /// <summary>
    /// Serializes an object to JSON string with pretty formatting (indented).
    /// </summary>
    public static string SerializePretty<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, PrettyOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object.
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes a JSON string to a dynamic object.
    /// </summary>
    public static dynamic? DeserializeDynamic(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<dynamic>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extracts a property value from a JSON string without fully deserializing.
    /// Useful for reading specific values from large JSON documents.
    /// </summary>
    public static T? GetProperty<T>(string json, string propertyPath)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(propertyPath))
            return default;

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
    public static string MergeJson(string json1, string json2)
    {
        if (string.IsNullOrEmpty(json1))
            return json2 ?? string.Empty;

        if (string.IsNullOrEmpty(json2))
            return json1;

        try
        {
            using (var doc1 = JsonDocument.Parse(json1))
            using (var doc2 = JsonDocument.Parse(json2))
            {
                var merged = MergeElements(doc1.RootElement, doc2.RootElement);
                return JsonSerializer.Serialize(merged, DefaultOptions);
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to merge JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates if a string is valid JSON.
    /// </summary>
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
    /// Recursively merges two JSON elements.
    /// </summary>
    private static JsonElement MergeElements(JsonElement first, JsonElement second)
    {
        if (first.ValueKind == JsonValueKind.Object && second.ValueKind == JsonValueKind.Object)
        {
            var options = new JsonSerializerOptions { WriteIndented = false };
            var result = JsonDocument.Parse("{}").RootElement;

            foreach (var property in first.EnumerateObject())
            {
                var merged = result.GetProperty(property.Name);
                // In a real implementation, would need to actually merge the objects
            }

            foreach (var property in second.EnumerateObject())
            {
                // Override with second object's properties
            }

            return second; // Simplified: just return second for now
        }

        return second;
    }
}
