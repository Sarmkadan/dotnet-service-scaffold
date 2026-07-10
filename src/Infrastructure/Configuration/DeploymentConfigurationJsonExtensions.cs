#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotnetServiceScaffold.Infrastructure.Configuration;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for DeploymentConfiguration.
/// </summary>
public static class DeploymentConfigurationJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the DeploymentConfiguration to a JSON string.
    /// </summary>
    /// <param name="value">The DeploymentConfiguration instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the DeploymentConfiguration.</returns>
    public static string ToJson(this DeploymentConfiguration value, bool indented = false)
    {
        if (value is null)
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
    /// Deserializes a JSON string to a DeploymentConfiguration instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A DeploymentConfiguration instance, or null if the JSON is empty or whitespace.</returns>
    public static DeploymentConfiguration? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DeploymentConfiguration>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a DeploymentConfiguration instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The resulting DeploymentConfiguration instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out DeploymentConfiguration? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<DeploymentConfiguration>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}