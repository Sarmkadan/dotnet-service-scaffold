#nullable enable

using System.Text.Json;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="DatabaseBenchmarks"/>.
/// </summary>
public static class DatabaseBenchmarksJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="DatabaseBenchmarks"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The database benchmarks instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the database benchmarks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this DatabaseBenchmarks value, bool indented = false) =>
        value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(value, indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="DatabaseBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized database benchmarks instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
    public static DatabaseBenchmarks FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<DatabaseBenchmarks>(json, _jsonSerializerOptions)
            ?? throw new JsonException(DatabaseBenchmarksJsonExtensionsConstants.DeserializedResultIsNull);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="DatabaseBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized database benchmarks instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out DatabaseBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<DatabaseBenchmarks>(json, _jsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}