#nullable enable

using System.Text.Json;
using DotnetServiceScaffold.Presentation.Controllers;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="ApiKeyController"/>.
/// </summary>
public static class ApiKeyControllerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Converts an <see cref="ApiKeyController"/> instance to its JSON representation.
    /// </summary>
    /// <param name="value">The <see cref="ApiKeyController"/> instance to convert.</param>
    /// <param name="indented">Whether to write indented JSON.</param>
    /// <returns>A JSON string representing the <see cref="ApiKeyController"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ApiKeyController value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = _jsonOptions.PropertyNamingPolicy }; options.WriteIndented = indented;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts a JSON string to an <see cref="ApiKeyController"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <returns>
    /// An <see cref="ApiKeyController"/> instance, or <see langword="null"/> if <paramref name="json"/> is <see langword="null"/> or empty.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">Thrown when <paramref name="json"/> is not valid JSON for an <see cref="ApiKeyController"/> instance.</exception>
    public static ApiKeyController? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ApiKeyController>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to convert a JSON string to an <see cref="ApiKeyController"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <param name="value">
    /// When this method returns, contains the <see cref="ApiKeyController"/> instance if the conversion succeeded, or <see langword="null"/> if it failed.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="json"/> was successfully converted; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out ApiKeyController? value)
    {
        if (string.IsNullOrEmpty(json))
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ApiKeyController>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}