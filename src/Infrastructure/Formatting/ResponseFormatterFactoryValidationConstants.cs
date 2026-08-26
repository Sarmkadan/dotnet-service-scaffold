#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Contains constant values used by <see cref="ResponseFormatterFactoryValidation"/>.
/// </summary>
internal static class ResponseFormatterFactoryValidationConstants
{
    /// <summary>
    /// Message indicating no formatters are registered.
    /// </summary>
    public const string NoRegisteredFormatters = "ResponseFormatterFactory has no registered formatters.";

    /// <summary>
    /// Message indicating the default formatter is null.
    /// </summary>
    public const string DefaultFormatterIsNull = "ResponseFormatterFactory default formatter is null.";

    /// <summary>
    /// Message format for default formatter retrieval failure.
    /// </summary>
    public const string DefaultFormatterRetrievalFailed = "ResponseFormatterFactory default formatter retrieval failed: {0}.";

    /// <summary>
    /// Message indicating a formatter has null or whitespace media type.
    /// </summary>
    public const string FormatterMediaTypeIsNullOrWhiteSpace = "ResponseFormatterFactory contains formatter with null or whitespace media type.";

    /// <summary>
    /// Message format for formatter being null for a specific media type.
    /// </summary>
    public const string FormatterForMediaTypeIsNull = "ResponseFormatterFactory formatter for media type '{0}' is null.";

    /// <summary>
    /// Message format for media type reported as not supported despite being registered.
    /// </summary>
    public const string MediaTypeNotSupportedDespiteRegistered = "ResponseFormatterFactory reports media type '{0}' as not supported despite being registered.";

    /// <summary>
    /// Message format for duplicate media type registrations.
    /// </summary>
    public const string DuplicateMediaTypeRegistrations = "ResponseFormatterFactory contains duplicate media type registrations: {0}.";
}