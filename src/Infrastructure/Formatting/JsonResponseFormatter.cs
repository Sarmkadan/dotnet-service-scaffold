#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Formatter for JSON responses. Handles serialization of objects to JSON with
/// consistent formatting, null handling, and date serialization options.
/// </summary>
public class JsonResponseFormatter : IResponseFormatter
{
    /// <summary>
    /// Gets the media type that this formatter handles.
    /// </summary>
    public string MediaType => "application/json";

    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the JsonResponseFormatter class with default JSON serialization options.
    /// </summary>
    public JsonResponseFormatter()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new JsonDateTimeConverter()
            }
        };
    }

    /// <summary>
    /// Formats an object as JSON using standard serialization options.
    /// </summary>
    /// <param name="data">The object to format as JSON.</param>
    /// <returns>A task that represents the asynchronous formatting operation. The task result contains the JSON string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when serialization fails.</exception>
    public Task<string> FormatAsync(object? data)
    {
        ArgumentNullException.ThrowIfNull(data);

        try
        {
            var json = JsonSerializer.Serialize(data, _options);
            return Task.FromResult(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to serialize object to JSON", ex);
        }
    }

    /// <summary>
    /// Determines if this formatter can handle the given media type.
    /// </summary>
    /// <param name="mediaType">The media type to check.</param>
    /// <returns>True if the formatter can handle the media type; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when mediaType is null, empty, or consists only of white-space characters.</exception>
    public bool CanFormat(string mediaType)
    {
        ArgumentException.ThrowIfNullOrEmpty(mediaType);

        return !string.IsNullOrEmpty(mediaType) &&
               (mediaType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ||
                mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Custom JSON converter for DateTime that uses ISO 8601 format with UTC timezone.
    /// </summary>
    private class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        /// <summary>
        /// Reads and converts the JSON to DateTime.
        /// </summary>
        /// <param name="reader">The Utf8JsonReader to read from.</param>
        /// <param name="typeToConvert">The type to convert to.</param>
        /// <param name="options">The JsonSerializerOptions to use.</param>
        /// <returns>The converted DateTime value.</returns>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (DateTime.TryParse(stringValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            {
                return result.ToUniversalTime();
            }

            throw new JsonException($"Unable to convert \"{stringValue}\" to DateTime");
        }

        /// <summary>
        /// Writes the DateTime value as JSON.
        /// </summary>
        /// <param name="writer">The Utf8JsonWriter to write to.</param>
        /// <param name="value">The DateTime value to write.</param>
        /// <param name="options">The JsonSerializerOptions to use.</param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Ensure UTC and format as ISO 8601
            var utcValue = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();

            writer.WriteStringValue(utcValue.ToString("o"));
        }
    }
}
