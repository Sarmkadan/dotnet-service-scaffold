#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Interface for response formatters. Allows for multiple output formats (JSON, CSV, XML)
/// and implements the Strategy pattern for flexible response serialization.
/// </summary>
public interface IResponseFormatter
{
    /// <summary>
    /// Gets the media type this formatter handles (e.g., <c>application/json</c>).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the formatter cannot provide a media type.
    /// </exception>
    string MediaType { get; }

    /// <summary>
    /// Formats an object to a string in the appropriate format.
    /// </summary>
    /// <param name="data">The object to format. May be <c>null</c>, in which case the formatter should return an appropriate representation (e.g., <c>"null"</c> for JSON).</param>
    /// <returns>A <see cref="Task{TResult}"/> that resolves to the formatted string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the formatter fails to serialize the supplied <paramref name="data"/>.
    /// </exception>
    Task<string> FormatAsync(object? data);

    /// <summary>
    /// Determines whether this formatter can handle the given media type.
    /// </summary>
    /// <param name="mediaType">The media type to evaluate (e.g., <c>application/json</c>).</param>
    /// <returns><c>true</c> if the formatter can handle <paramref name="mediaType"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaType"/> is <c>null</c>.</exception>
    bool CanFormat(string mediaType);
}
