#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Constants for LogContextService to avoid magic strings.
/// </summary>
internal static class LogContextServiceConstants
{
    public const string CorrelationIdKey = "CorrelationId";
    public const string UserIdKey = "UserId";
    public const string ActivityIdKey = "ActivityId";
    public const string TraceParentKey = "TraceParent";

    public const string TraceParentFormatW3C = "00-{0:D32}-{1:D16}-00";
    public const string TraceParentFormatLegacy = "00-{0:D32}-{1:D16}-01";
    public const string GuidFormatN = "N";
}