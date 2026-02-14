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
    public static Guid? GetUserId(this HttpContext context)
    {
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
    public static string? GetUserEmail(this HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets the current authenticated user's username from claims.
    /// Returns null if user is not authenticated or name claim is not found.
    /// </summary>
    public static string? GetUsername(this HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    public static bool IsAuthenticated(this HttpContext context)
    {
        return context.User?.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Gets a claim value by type. Returns null if claim is not found.
    /// </summary>
    public static string? GetClaim(this HttpContext context, string claimType)
    {
        return context.User?.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Checks if the user has a specific claim with a given value.
    /// </summary>
    public static bool HasClaim(this HttpContext context, string claimType, string claimValue)
    {
        return context.User?.HasClaim(claimType, claimValue) ?? false;
    }

    /// <summary>
    /// Gets the client's IP address, accounting for reverse proxies and load balancers.
    /// Checks X-Forwarded-For header first, then uses RemoteIpAddress.
    /// </summary>
    public static string? GetClientIpAddress(this HttpContext context)
    {
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
    public static string? GetBearerToken(this HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        return HttpUtility.ParseBearerToken(authHeader);
    }

    /// <summary>
    /// Gets the API key from the X-Api-Key header.
    /// Returns null if header is missing.
    /// </summary>
    public static string? GetApiKey(this HttpContext context)
    {
        return context.Request.Headers.FirstOrDefault(h =>
            h.Key.Equals("X-Api-Key", StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();
    }

    /// <summary>
    /// Gets the user agent string from the request headers.
    /// </summary>
    public static string? GetUserAgent(this HttpContext context)
    {
        return context.Request.Headers.UserAgent.ToString();
    }

    /// <summary>
    /// Gets the request's content type. Returns "application/octet-stream" as default.
    /// </summary>
    public static string GetContentType(this HttpContext context)
    {
        return context.Request.ContentType ?? "application/octet-stream";
    }

    /// <summary>
    /// Checks if the request is HTTPS/TLS.
    /// </summary>
    public static bool IsSecureConnection(this HttpContext context)
    {
        return context.Request.IsHttps;
    }

    /// <summary>
    /// Gets the full request URL including query string.
    /// </summary>
    public static string GetFullUrl(this HttpContext context)
    {
        var scheme = context.Request.Scheme;
        var host = context.Request.Host;
        var path = context.Request.Path;
        var query = context.Request.QueryString;

        return $"{scheme}://{host}{path}{query}";
    }

    /// <summary>
    /// Sets a response header, replacing existing value if present.
    /// </summary>
    public static void SetResponseHeader(this HttpContext context, string name, string value)
    {
        context.Response.Headers[name] = value;
    }

    /// <summary>
    /// Adds a response header, preserving existing values.
    /// </summary>
    public static void AddResponseHeader(this HttpContext context, string name, string value)
    {
        context.Response.Headers.Append(name, value);
    }

    /// <summary>
    /// Sets the response content type.
    /// </summary>
    public static void SetResponseContentType(this HttpContext context, string contentType)
    {
        context.Response.ContentType = contentType;
    }

    /// <summary>
    /// Checks if the request accepts JSON responses.
    /// </summary>
    public static bool AcceptsJson(this HttpContext context)
    {
        var accept = context.Request.Headers.Accept.ToString();
        return string.IsNullOrEmpty(accept) || accept.Contains("application/json") || accept.Contains("*/*");
    }

    /// <summary>
    /// Checks if the request is from a browser (based on User-Agent header).
    /// </summary>
    public static bool IsFromBrowser(this HttpContext context)
    {
        var userAgent = context.GetUserAgent() ?? string.Empty;
        return userAgent.Contains("Mozilla") || userAgent.Contains("Chrome") || userAgent.Contains("Safari");
    }
}
