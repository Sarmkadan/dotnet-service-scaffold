#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for validation.
/// Since ValidationUtility is a static class, these extension methods provide JSON
/// serialization capabilities for validation-related operations and data structures.
/// </summary>
public static class ValidationUtilityJsonExtensions
{
    /// <summary>
    /// Gets the shared JsonSerializerOptions configured for camelCase property naming and
    /// handling of validation-related JSON serialization.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Serializes a validation result message to a JSON string.
    /// </summary>
    /// <param name="validationResult">The validation result message to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the validation result.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="validationResult"/> is null or empty.</exception>
    public static string ToJson(this string validationResult, bool indented = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(validationResult);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions)
            {
                WriteIndented = true,
            }
            : JsonOptions;

        return JsonSerializer.Serialize(new { Message = validationResult }, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a validation result message.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A validation result message if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static string? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var result = JsonSerializer.Deserialize<ValidationResultDto>(json, JsonOptions);
            return result?.Message;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a validation result message.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the validation result message if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out string? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var result = JsonSerializer.Deserialize<ValidationResultDto>(json, JsonOptions);
            value = result?.Message;
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Data Transfer Object for validation result messages.
    /// </summary>
    private sealed class ValidationResultDto
    {
        public string? Message { get; set; }
    }
}