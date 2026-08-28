#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Interface for rate limiting middleware that implements a simple token bucket algorithm per IP address.
/// </summary>
public interface IRateLimitingMiddleware
{
    /// <summary>
    /// Invoke the middleware. Checks the rate limit for the client's IP and either allows
    /// the request or returns 429 Too Many Requests.
    /// </summary>
    Task InvokeAsync(HttpContext context);
}