#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Interface for XML response formatter.
/// </summary>
public interface IXmlResponseFormatter
{
    /// <summary>
    /// Formats an object as XML using XmlSerializer.
    /// </summary>
    Task<string> FormatAsync(object? data);

    /// <summary>
    /// Determines if this formatter can handle the given media type.
    /// Accepts: application/xml, text/xml, application/*+xml
    /// </summary>
    bool CanFormat(string mediaType);
}