using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace DotnetServiceScaffold.Tests
{
    public static class ExternalApiClientExtensionsTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
;

        /// <summary>
        /// Serializes the specified value to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="indented">true to format the JSON with indentation; otherwise false.</param>
        /// <returns>A JSON string representation of the object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this ExternalApiClientExtensionsTests? value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            var options = indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions;
            return System.Text.Json.JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into an instance of <see cref="ExternalApiClientExtensionsTests"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Deserialized object or null if deserialization fails.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
        public static ExternalApiClientExtensionsTests? FromJson(string? json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<ExternalApiClientExtensionsTests>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into an instance of <see cref="ExternalApiClientExtensionsTests"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized object on success.</param>
        /// <returns>true if deserialization succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or whitespace.</exception>
        public static bool TryFromJson(string? json, out ExternalApiClientExtensionsTests? value)
        {
            try
            {
                ArgumentException.ThrowIfNullOrEmpty(json);
                value = System.Text.Json.JsonSerializer.Deserialize<ExternalApiClientExtensionsTests>(json, _jsonOptions);
                return value != null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
