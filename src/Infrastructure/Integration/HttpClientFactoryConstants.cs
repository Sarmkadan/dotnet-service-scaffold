#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Constants for HttpClientFactory.
/// </summary>
internal static class HttpClientFactoryConstants
{
    /// <summary>
    /// Default timeout in seconds for HTTP clients.
    /// </summary>
    public const int DefaultTimeoutSeconds = 30;

    /// <summary>
    /// Default User-Agent header name.
    /// </summary>
    public const string DefaultUserAgentHeaderName = "User-Agent";

    /// <summary>
    /// Default User-Agent header value.
    /// </summary>
    public const string DefaultUserAgentHeaderValue = "DotnetServiceScaffold/1.0";

    /// <summary>
    /// Header name for API key authentication.
    /// </summary>
    public const string ApiKeyHeaderName = "X-Api-Key";

    /// <summary>
    /// Header name for Bearer token authentication.
    /// </summary>
    public const string AuthorizationHeaderName = "Authorization";
}