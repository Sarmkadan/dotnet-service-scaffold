#nullable enable

namespace DotnetServiceScaffold.Shared.Extensions;

/// <summary>
/// Constants used in ExceptionExtensions class.
/// </summary>
internal static class ExceptionExtensionsConstants
{
    /// <summary>
    /// Separator used between exception messages in GetFullMessage.
    /// </summary>
    public const string ExceptionMessageSeparator = " -> ";

    /// <summary>
    /// Format string for exception level in GetFullStackTrace.
    /// </summary>
    public const string ExceptionLevelFormat = "Level {0}: {1}";

    /// <summary>
    /// Default context for ToLogMessage when none is provided.
    /// </summary>
    public const string DefaultLogContext = "Error";

    /// <summary>
    /// HTTP status code for Bad Request (400).
    /// </summary>
    public const int HttpStatusCodeBadRequest = 400;

    /// <summary>
    /// HTTP status code for Not Found (404).
    /// </summary>
    public const int HttpStatusCodeNotFound = 404;

    /// <summary>
    /// HTTP status code for Conflict (409).
    /// </summary>
    public const int HttpStatusCodeConflict = 409;

    /// <summary>
    /// HTTP status code for Gateway Timeout (504).
    /// </summary>
    public const int HttpStatusCodeGatewayTimeout = 504;

    /// <summary>
    /// HTTP status code for Not Implemented (501).
    /// </summary>
    public const int HttpStatusCodeNotImplemented = 501;

    /// <summary>
    /// HTTP status code for Bad Gateway (502).
    /// </summary>
    public const int HttpStatusCodeBadGateway = 502;

    /// <summary>
    /// HTTP status code for Internal Server Error (500).
    /// </summary>
    public const int HttpStatusCodeInternalServerError = 500;
}