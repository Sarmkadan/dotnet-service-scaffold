#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Infrastructure.Data;

/// <summary>
/// Provides System.Text.Json serialization extensions for ServiceScaffoldDbContext.
/// </summary>
public static class ServiceScaffoldDbContextJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Serializes the ServiceScaffoldDbContext instance to a JSON string.
    /// </summary>
    /// <param name="value">The DbContext instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the DbContext.</returns>
    public static string ToJson(this ServiceScaffoldDbContext value, bool indented = false)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a ServiceScaffoldDbContext instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A ServiceScaffoldDbContext instance, or null if the JSON is null or empty.</returns>
    public static ServiceScaffoldDbContext? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ServiceScaffoldDbContext>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ServiceScaffoldDbContext instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The resulting ServiceScaffoldDbContext instance, or null on failure.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ServiceScaffoldDbContext? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ServiceScaffoldDbContext>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}