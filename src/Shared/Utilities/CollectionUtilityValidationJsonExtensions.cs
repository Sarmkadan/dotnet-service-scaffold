#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for
/// <see cref="CollectionUtilityValidation"/> validation results.
/// </summary>
/// <remarks>
/// <see cref="CollectionUtilityValidation"/> is a static validation helper with no instance state,
/// so this class serializes the read-only list of validation error messages it produces,
/// enabling consistent JSON handling of collection validation results across the application.
/// </remarks>
public static class CollectionUtilityValidationJsonExtensions
{
    /// <summary>
    /// Gets the shared JsonSerializerOptions configured for camelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes a <see cref="CollectionUtilityValidation"/> validation result to a JSON string.
    /// </summary>
    /// <param name="value">The validation error messages to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the validation result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this IReadOnlyList<string> value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(new ValidationResultDto { Errors = value }, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="CollectionUtilityValidation"/> validation result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized validation result if successful; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static IReadOnlyList<string>? FromJson(string? json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            var result = JsonSerializer.Deserialize<ValidationResultDto>(json, JsonOptions);
            return result?.Errors;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="CollectionUtilityValidation"/> validation result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized validation result if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string? json, out IReadOnlyList<string>? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            var result = JsonSerializer.Deserialize<ValidationResultDto>(json, JsonOptions);
            value = result?.Errors;
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Data Transfer Object for collection validation result messages.
    /// </summary>
    private sealed class ValidationResultDto
    {
        public IReadOnlyList<string>? Errors { get; set; }
    }
}