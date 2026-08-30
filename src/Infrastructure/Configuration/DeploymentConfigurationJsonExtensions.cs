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
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="DeploymentConfiguration"/>.
/// </summary>
/// <remarks>
/// This class offers methods to convert between <see cref="DeploymentConfiguration"/> instances and JSON strings
/// using camelCase property naming policy and case-insensitive deserialization.
/// </remarks>
public static class DeploymentConfigurationJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = DeploymentConfigurationJsonExtensionsConstants.PropertyNamingPolicy,
        WriteIndented = DeploymentConfigurationJsonExtensionsConstants.WriteIndented,
        TypeInfoResolver = DeploymentConfigurationJsonExtensionsConstants.TypeInfoResolver,
        PropertyNameCaseInsensitive = DeploymentConfigurationJsonExtensionsConstants.PropertyNameCaseInsensitive,
        ReferenceHandler = DeploymentConfigurationJsonExtensionsConstants.ReferenceHandler
    };

    /// <summary>
    /// Serializes the <see cref="DeploymentConfiguration"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="DeploymentConfiguration"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="DeploymentConfiguration"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this DeploymentConfiguration value, bool indented = false)
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
    /// Deserializes a JSON string to a <see cref="DeploymentConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="DeploymentConfiguration"/> instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized.</exception>
    public static DeploymentConfiguration? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DeploymentConfiguration>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="DeploymentConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The resulting <see cref="DeploymentConfiguration"/> instance, or <see langword="null"/> if deserialization fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
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