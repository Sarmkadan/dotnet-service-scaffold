#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Xml;
using System.Xml.Serialization;

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Formatter for XML responses. Handles serialization of objects to XML using
/// System.Xml.Serialization.XmlSerializer with consistent formatting and null handling.
/// </summary>
public class XmlResponseFormatter : IResponseFormatter
{
    public string MediaType => "application/xml";

    private readonly XmlSerializerNamespaces _emptyNamespaces;

    public XmlResponseFormatter()
    {
        // Use empty namespaces to avoid xmlns attributes in output
        _emptyNamespaces = new XmlSerializerNamespaces();
        _emptyNamespaces.Add("", "");
    }

    /// <summary>
    /// Formats an object as XML using XmlSerializer.
    /// </summary>
    public Task<string> FormatAsync(object? data)
    {
        if (data is null)
            return Task.FromResult("");

        try
        {
            using var writer = new StringWriter();
            var serializer = new XmlSerializer(data.GetType());
            serializer.Serialize(writer, data, _emptyNamespaces);
            return Task.FromResult(writer.ToString());
        }
        catch (InvalidOperationException ex) when (ex.InnerException is XmlException)
        {
            throw new InvalidOperationException("Failed to serialize object to XML", ex);
        }
    }

    /// <summary>
    /// Determines if this formatter can handle the given media type.
    /// Accepts: application/xml, text/xml, application/*+xml
    /// </summary>
    public bool CanFormat(string mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
            return false;

        var normalizedMediaType = mediaType.ToLowerInvariant();

        return normalizedMediaType.StartsWith("application/xml", StringComparison.OrdinalIgnoreCase) ||
               normalizedMediaType.StartsWith("text/xml", StringComparison.OrdinalIgnoreCase) ||
               normalizedMediaType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase);
    }
}