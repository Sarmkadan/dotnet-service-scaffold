namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Constants for ResponseFormatterFactoryExtensions.
/// </summary>
internal static class ResponseFormatterFactoryExtensionsConstants
{
    /// <summary>
    /// Message template for when no formatter is registered for a media type.
    /// </summary>
    public const string NoFormatterRegisteredMessage = "No formatter registered for media type '{0}'. ";

    /// <summary>
    /// Prefix for the list of available media types.
    /// </>
    public const string AvailableMediaTypesMessage = "Available media types: ";
}