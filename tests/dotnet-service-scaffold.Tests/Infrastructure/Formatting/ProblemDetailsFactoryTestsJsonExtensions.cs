using System;
using System.Text.Json;

namespace dotnet_service_scaffold.Tests.Infrastructure.Formatting
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for <see cref="ProblemDetailsFactoryTests"/>.
    /// </summary>
    public static class ProblemDetailsFactoryTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the supplied <paramref name="value"/> to a JSON string using camelCase property naming.
        /// </summary>
        /// <param name="value">The <see cref="ProblemDetailsFactoryTests"/> instance to serialize.</param>
        /// <param name="indented">Whether the resulting JSON should be indented for readability.</param>
        /// <returns>The JSON representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
        public static string ToJson(this ProblemDetailsFactoryTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
                : JsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes the supplied JSON string into a <see cref="ProblemDetailsFactoryTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="ProblemDetailsFactoryTests"/> instance, or <c>null</c> if the JSON represents a null value.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="json"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="json"/> is empty or whitespace.</exception>
        /// <exception cref="JsonException">If <paramref name="json"/> is not valid JSON or cannot be deserialized.</exception>
        public static ProblemDetailsFactoryTests? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<ProblemDetailsFactoryTests>(json, JsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize the supplied JSON string into a <see cref="ProblemDetailsFactoryTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">When this method returns, contains the deserialized instance, or <c>null</c> if deserialization failed.</param>
        /// <returns><c>true</c> if <paramref name="json"/> was successfully deserialized; otherwise, <c>false</c>.</returns>
        public static bool TryFromJson(string json, out ProblemDetailsFactoryTests? value)
        {
            try
            {
                value = FromJson(json);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}