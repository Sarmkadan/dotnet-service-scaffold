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
    public string MediaType => "application/json";

    private readonly JsonSerializerOptions _options;

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
    public Task<string> FormatAsync(object? data)
    {
        if (data is null)
            return Task.FromResult("null");

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
    /// Accepts: application/json, application/json+custom, etc.
    /// </summary>
    public bool CanFormat(string mediaType)
    {
        return !string.IsNullOrEmpty(mediaType) &&
               (mediaType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ||
                mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Custom JSON converter for DateTime that uses ISO 8601 format with UTC timezone.
    /// </summary>
    private class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (DateTime.TryParse(stringValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            {
                return result.ToUniversalTime();
            }

            throw new JsonException($"Unable to convert \"{stringValue}\" to DateTime");
        }

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
