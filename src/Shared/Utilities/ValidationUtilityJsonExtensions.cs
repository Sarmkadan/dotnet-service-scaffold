#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for validation result messages.
/// </summary>
/// <remarks>
/// This class provides extension methods for serializing and deserializing validation result messages
/// to and from JSON format, enabling consistent handling of validation results across the application.
/// </remarks>
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
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes a validation result message to a JSON string.
    /// </summary>
    /// <param name="validationResult">The validation result message to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the validation result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="validationResult"/> is <see langword="null"/>.</exception>
    public static string ToJson(this string validationResult, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(new { Message = validationResult }, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a validation result message.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A validation result message if successful; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static string? FromJson(string? json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

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
    /// <param name="value">Receives the validation result message if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string? json, out string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

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
        [JsonPropertyName(ValidationUtilityJsonExtensionsConstants.MessagePropertyName)]
        public string? Message { get; set; }
    }
}
