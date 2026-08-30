#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Constants for LogContextServiceExtensions.
/// </summary>
internal static class LogContextServiceExtensionsConstants
{
    /// <summary>
    /// The property name for correlation ID.
    /// </summary>
    public const string CorrelationIdPropertyName = "CorrelationId";

    /// <summary>
    /// The property name for request ID.
    /// </summary>
    public const string RequestIdPropertyName = "RequestId";

    /// <summary>
    /// The property name for user ID.
    /// </summary>
    public const string UserIdPropertyName = "UserId";

    /// <summary>
    /// The property name for operation.
    /// </summary>
    public const string OperationPropertyName = "Operation";

    /// <summary>
    /// The property name for measured action.
    /// </summary>
    public const string MeasuredActionPropertyName = "MeasuredAction";

    /// <summary>
    /// The property name for action duration in milliseconds.
    /// </summary>
    public const string ActionDurationMsPropertyName = "ActionDurationMs";

    /// <summary>
    /// The format string for generating a GUID without hyphens.
    /// </summary>
    public const string GuidFormatN = "N";
}