#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Extension methods for <see cref="ResponseFormatterFactory"/> that provide additional
/// convenience methods for working with response formatters.
/// </summary>
public static class ResponseFormatterFactoryExtensions
{
    /// <summary>
    /// Gets a formatter for the specified media type, with a fallback to the specified
    /// formatter if no matching formatter is found.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="mediaType">The media type to get a formatter for.</param>
    /// <param name="fallbackFormatter">The formatter to use as fallback if no matching formatter is found.</param>
    /// <returns>The formatter instance.</returns>
    public static IResponseFormatter GetFormatterOrDefault(
        this ResponseFormatterFactory factory,
        string? mediaType,
        IResponseFormatter fallbackFormatter)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        if (fallbackFormatter is null)
            throw new ArgumentNullException(nameof(fallbackFormatter));

        var formatter = factory.GetFormatter(mediaType);
        return formatter ?? fallbackFormatter;
    }

    /// <summary>
    /// Attempts to get a formatter for the specified media type.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="mediaType">The media type to get a formatter for.</param>
    /// <param name="formatter">When this method returns, contains the formatter instance if found; otherwise, null.</param>
    /// <returns>True if a formatter was found; otherwise, false.</returns>
    public static bool TryGetFormatter(
        this ResponseFormatterFactory factory,
        string? mediaType,
        out IResponseFormatter? formatter)
    {
        if (factory is null)
        {
            formatter = null;
            throw new ArgumentNullException(nameof(factory));
        }

        formatter = factory.GetFormatter(mediaType);
        return formatter != null;
    }

    /// <summary>
    /// Gets a formatter for the specified media type, throwing a descriptive exception
    /// if no matching formatter is found.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="mediaType">The media type to get a formatter for.</param>
    /// <returns>The formatter instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no formatter is found for the media type.</exception>
    public static IResponseFormatter GetFormatterRequired(
        this ResponseFormatterFactory factory,
        string mediaType)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        if (string.IsNullOrWhiteSpace(mediaType))
            throw new ArgumentException("Media type cannot be null or empty", nameof(mediaType));

        var formatter = factory.GetFormatter(mediaType);
        if (formatter is null)
        {
            throw new InvalidOperationException(
                $"No formatter registered for media type '{mediaType}'. " +
                $"Available media types: {string.Join(", ", factory.GetSupportedMediaTypes())}");
        }

        return formatter;
    }

    /// <summary>
    /// Registers a custom formatter for multiple media types at once.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="formatter">The formatter to register.</param>
    /// <param name="mediaTypes">The media types to register the formatter for.</param>
    public static void RegisterFormatter(
        this ResponseFormatterFactory factory,
        IResponseFormatter formatter,
        params string[] mediaTypes)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (mediaTypes is null || mediaTypes.Length == 0)
            throw new ArgumentException("At least one media type must be provided", nameof(mediaTypes));

        foreach (var mediaType in mediaTypes)
        {
            factory.RegisterFormatter(mediaType, formatter);
        }
    }

    /// <summary>
    /// Checks if any of the specified media types are supported.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="mediaTypes">The media types to check.</param>
    /// <returns>True if any of the media types are supported; otherwise, false.</returns>
    public static bool AreAnyMediaTypesSupported(
        this ResponseFormatterFactory factory,
        params string[] mediaTypes)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        if (mediaTypes is null || mediaTypes.Length == 0)
            return true;

        foreach (var mediaType in mediaTypes)
        {
            if (factory.IsMediaTypeSupported(mediaType))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the default formatter (JSON formatter) from the factory.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <returns>The default formatter instance.</returns>
    public static IResponseFormatter GetDefaultFormatter(
        this ResponseFormatterFactory factory)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        return factory.GetFormatter(null);
    }
}