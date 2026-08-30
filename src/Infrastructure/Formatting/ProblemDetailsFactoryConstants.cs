#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Constants for ProblemDetailsFactory.
/// </summary>
internal static class ProblemDetailsFactoryConstants
{
    /// <summary>
    /// Default problem type URI when none is specified.
    /// </summary>
    public const string AboutBlank = "about:blank";

    /// <summary>
    /// Extension key for trace identifier.
    /// </summary>
    public const string TraceIdKey = "traceId";

    /// <summary>
    /// Extension key for correlation identifier.
    /// </summary>
    public const string CorrelationIdKey = "correlationId";

    /// <summary>
    /// Extension key for application-specific error code.
    /// </summary>
    public const string ErrorCodeKey = "errorCode";

    /// <summary>
    /// Extension key for timestamp.
    /// </summary>
    public const string TimestampKey = "timestamp";
}