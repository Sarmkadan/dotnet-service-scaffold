#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Constants for CorrelationIdMiddleware to avoid magic strings.
/// </summary>
internal static class CorrelationIdMiddlewareConstants
{
    /// <summary>
    /// Default correlation ID header name.
    /// </summary>
    public const string DefaultCorrelationIdHeader = "X-Correlation-Id";

    /// <summary>
    /// Format string for W3C traceparent header.
    /// </summary>
    public const string TraceParentFormatW3C = "00-{0:D32}-{1:D16}-00";

    /// <summary>
    /// Format string for legacy traceparent header.
    /// </summary>
    public const string TraceParentFormatLegacy = "00-{0:D32}-{1:D16}-01";
}