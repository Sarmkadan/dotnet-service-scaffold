#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Factory for creating response formatters based on media type. Implements the
/// Factory pattern to decouple formatter selection from usage. Provides a registry
/// of available formatters and selects the appropriate one for a given media type.
/// </summary>
public class ResponseFormatterFactory : IResponseFormatterFactory
{
    private readonly Dictionary<string, IResponseFormatter> _formatters;
    private readonly IResponseFormatter _defaultFormatter;

    public ResponseFormatterFactory()
    {
        _formatters = new Dictionary<string, IResponseFormatter>(StringComparer.OrdinalIgnoreCase)
        {
            { "application/json", new JsonResponseFormatter() },
            { "text/csv", new CsvResponseFormatter() },
            { "application/csv", new CsvResponseFormatter() }
        };

        _defaultFormatter = _formatters["application/json"];
    }

    /// <summary>
    /// Gets a formatter for the specified media type. Returns the default (JSON) formatter
    /// if no matching formatter is found.
    /// </summary>
    public IResponseFormatter GetFormatter(string? mediaType)
    {
        if (string.IsNullOrWhiteSpace(mediaType))
            return _defaultFormatter;

        // Try exact match first
        if (_formatters.TryGetValue(mediaType, out var formatter))
            return formatter;

        // Try to find a formatter that can handle this media type
        var matchingFormatter = _formatters.Values.FirstOrDefault(f => f.CanFormat(mediaType));
        return matchingFormatter ?? _defaultFormatter;
    }

    /// <summary>
    /// Registers a custom formatter for a media type.
    /// </summary>
    public void RegisterFormatter(string mediaType, IResponseFormatter formatter)
    {
        if (string.IsNullOrWhiteSpace(mediaType))
            throw new ArgumentException("Media type cannot be null or empty", nameof(mediaType));

        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        _formatters[mediaType.ToLowerInvariant()] = formatter;
    }

    /// <summary>
    /// Gets a list of all supported media types.
    /// </summary>
    public IEnumerable<string> GetSupportedMediaTypes()
    {
        return _formatters.Keys.ToList();
    }

    /// <summary>
    /// Checks if a media type is supported.
    /// </summary>
    public bool IsMediaTypeSupported(string? mediaType)
    {
        if (string.IsNullOrWhiteSpace(mediaType))
            return true; // Default to supported

        return _formatters.ContainsKey(mediaType) ||
               _formatters.Values.Any(f => f.CanFormat(mediaType));
    }
}

/// <summary>
/// Interface for the response formatter factory.
/// </summary>
public interface IResponseFormatterFactory
{
    IResponseFormatter GetFormatter(string? mediaType);
    void RegisterFormatter(string mediaType, IResponseFormatter formatter);
    IEnumerable<string> GetSupportedMediaTypes();
    bool IsMediaTypeSupported(string? mediaType);
}
