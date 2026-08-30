#nullable enable

namespace DotnetServiceScaffold.Presentation.Extensions;

/// <summary>
/// Constants used in HttpContextExtensions.
/// </summary>
internal static class HttpContextExtensionsConstants
{
    /// <summary>
    /// The X-Forwarded-For header name.
    /// </summary>
    public const string XForwardedForHeader = "X-Forwarded-For";

    /// <summary>
    /// The X-Real-IP header name.
    /// </summary>
    public const string XRealIpHeader = "X-Real-IP";

    /// <summary>
    /// The X-Api-Key header name.
    /// </summary>
    public const string ApiKeyHeader = "X-Api-Key";

    /// <summary>
    /// The default content type when none is specified.
    /// </summary>
    public const string DefaultContentType = "application/octet-stream";

    /// <summary>
    /// The Mozilla browser indicator string.
    /// </summary>
    public const string MozillaBrowserIndicator = "Mozilla";

    /// <summary>
    /// The Chrome browser indicator string.
    /// </summary>
    public const string ChromeBrowserIndicator = "Chrome";

    /// <summary>
    /// The Safari browser indicator string.
    /// </summary>
    public const string SafariBrowserIndicator = "Safari";
}