#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Utilities;
using System.Security.Claims;

namespace DotnetServiceScaffold.Presentation.Extensions;

/// <summary>
/// Extension methods for HttpContext to simplify common operations.
/// Provides helpers for extracting user info, retrieving claims, and parsing headers.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the current authenticated user's ID from claims.
    /// Returns null if user is not authenticated or ID claim is not found.
    /// </summary>
    /// <returns>The user ID as Guid, or null if not found.</returns>
    public static Guid? GetUserId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        return userId;
    }

    /// <summary>
    /// Gets the current authenticated user's email from claims.
    /// Returns null if user is not authenticated or email claim is not found.
    /// </summary>
    /// <returns>The user email, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string? GetUserEmail(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets the current authenticated user's username from claims.
    /// Returns null if user is not authenticated or name claim is not found.
    /// </summary>
    /// <returns>The username, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string? GetUsername(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    /// <returns>True if authenticated, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static bool IsAuthenticated(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.User?.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Gets a claim value by type. Returns null if claim is not found.
    /// </summary>
    /// <param name="claimType">The type of the claim to retrieve.</param>
    /// <returns>The claim value, or null if the claim is not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="claimType"/> is null.</exception>
    public static string? GetClaim(this HttpContext context, string claimType)
    {
        ArgumentNullException.ThrowIfNull(claimType);
        return context.User?.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Checks if the user has a specific claim with a given value.
    /// </summary>
    /// <param name="claimType">The type of the claim to check.</param>
    /// <param name="claimValue">The value to match against.</param>
    /// <returns>True if the claim exists with the specified value, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="claimType"/> or <paramref name="claimValue"/> is null.</exception>
    public static bool HasClaim(this HttpContext context, string claimType, string claimValue)
    {
        ArgumentNullException.ThrowIfNull(claimType);
        ArgumentNullException.ThrowIfNull(claimValue);
        ArgumentNullException.ThrowIfNull(context);

        return context.User?.HasClaim(claimType, claimValue) ?? false;
    }

    /// <summary>
    /// Gets the client's IP address, accounting for reverse proxies and load balancers.
    /// Checks X-Forwarded-For header first, then uses RemoteIpAddress.
    /// </summary>
    /// <returns>The client IP address, or null if not available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string? GetClientIpAddress(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Check for X-Forwarded-For header (common with reverse proxies)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var forwardedAddress = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(forwardedAddress))
            {
                return forwardedAddress;
            }
        }

        // Check for X-Real-IP header (Nginx)
        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            var ipAddress = realIp.FirstOrDefault();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return ipAddress;
            }
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Gets the bearer token from the Authorization header.
    /// Returns null if header is missing or not properly formatted.
    /// </summary>
    /// <returns>The bearer token, or null if not available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string? GetBearerToken(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var authHeader = context.Request.Headers.Authorization.ToString();
        return HttpUtility.ParseBearerToken(authHeader);
    }

    /// <summary>
    /// Gets the API key from the X-Api-Key header.
    /// Returns null if header is missing.
    /// </summary>
    /// <returns>The API key, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string? GetApiKey(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Request.Headers.FirstOrDefault(h =>
            h.Key.Equals("X-Api-Key", StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();
    }

    /// <summary>
    /// Gets the user agent string from the request headers.
    /// </summary>
    /// <returns>The user agent string, or null if not available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string? GetUserAgent(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Request.Headers.UserAgent.ToString();
    }

    /// <summary>
    /// Gets the request's content type. Returns "application/octet-stream" as default.
    /// </summary>
    /// <returns>The content type, never null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string GetContentType(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Request.ContentType ?? "application/octet-stream";
    }

    /// <summary>
    /// Checks if the request is HTTPS/TLS.
    /// </summary>
    /// <returns>True if the connection is secure, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static bool IsSecureConnection(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Request.IsHttps;
    }

    /// <summary>
    /// Gets the full request URL including query string.
    /// </summary>
    /// <returns>The full request URL.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string GetFullUrl(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var scheme = context.Request.Scheme;
        var host = context.Request.Host;
        var path = context.Request.Path;
        var query = context.Request.QueryString;

        return $"{scheme}://{host}{path}{query}";
    }

    /// <summary>
    /// Sets a response header, replacing existing value if present.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="value"/> is null.</exception>
    public static void SetResponseHeader(this HttpContext context, string name, string value)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        context.Response.Headers[name] = value;
    }

    /// <summary>
    /// Adds a response header, preserving existing values.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="value"/> is null.</exception>
    public static void AddResponseHeader(this HttpContext context, string name, string value)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        context.Response.Headers.Append(name, value);
    }

    /// <summary>
    /// Sets the response content type.
    /// </summary>
    /// <param name="contentType">The content type to set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentType"/> is null.</exception>
    public static void SetResponseContentType(this HttpContext context, string contentType)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(contentType);

        context.Response.ContentType = contentType;
    }

    /// <summary>
    /// Checks if the request accepts JSON responses.
    /// </summary>
    /// <returns>True if JSON is accepted, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static bool AcceptsJson(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var accept = context.Request.Headers.Accept.ToString();
        return string.IsNullOrEmpty(accept) || accept.Contains("application/json") || accept.Contains("*/*");
    }

    /// <summary>
    /// Checks if the request is from a browser (based on User-Agent header).
    /// </summary>
    /// <returns>True if the request appears to be from a browser, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static bool IsFromBrowser(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var userAgent = context.GetUserAgent() ?? string.Empty;
        return userAgent.Contains("Mozilla") || userAgent.Contains("Chrome") || userAgent.Contains("Safari");
    }
}