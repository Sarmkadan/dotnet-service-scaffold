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
    /// Gets the media type this formatter handles (e.g., "application/json").
    /// </summary>
    string MediaType { get; }

    /// <summary>
    /// Formats an object to a string in the appropriate format.
    /// </summary>
    Task<string> FormatAsync(object? data);

    /// <summary>
    /// Determines if this formatter can handle the given media type.
    /// </summary>
    bool CanFormat(string mediaType);
}
