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
    /// Serializes a ServiceScaffoldDbContext instance to a JSON string.
    /// </summary>
    /// <remarks>
    /// This method serializes only the configuration and metadata of the DbContext,
    /// not the actual database connection or change tracking state. The serialized
    /// data includes model configuration but excludes runtime state like:
    /// <list type="bullet">
    ///   <item>Active database connections</item>
    ///   <item>Change tracker entries</item>
    ///   <item>Service provider dependencies</item>
    ///   <item>Logger instances</item>
    /// </list>
    /// </remarks>
    /// <param name="value">The DbContext instance to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the DbContext configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ServiceScaffoldDbContext value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

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
    /// <remarks>
    /// <para>WARNING: This method cannot actually deserialize a functional DbContext.</para>
    /// <para>The ServiceScaffoldDbContext requires a database connection and service provider
    /// to function properly. This method exists only for serialization round-tripping
    /// of DbContext configuration and metadata, not for creating usable DbContext instances.</para>
    /// <para>Any attempt to use the returned object will result in runtime errors due to
    /// missing required dependencies.</para>
    /// </remarks>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A ServiceScaffoldDbContext instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static ServiceScaffoldDbContext? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ServiceScaffoldDbContext>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ServiceScaffoldDbContext instance.
    /// </summary>
    /// <remarks>
    /// <para>WARNING: The deserialized object cannot function as a DbContext.</para>
    /// <para>See <see cref="FromJson(string)"/> for important limitations about what
    /// this method actually returns.</para>
    /// </remarks>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The resulting ServiceScaffoldDbContext instance, or null on failure.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out ServiceScaffoldDbContext? value)
    {
        ArgumentNullException.ThrowIfNull(json);

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