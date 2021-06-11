// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Rate limiting middleware that implements a simple token bucket algorithm per IP address.
/// Prevents abuse by limiting the number of requests per time window. Configuration allows
/// different limits for authenticated vs anonymous requests.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        RateLimitOptions options,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Invoke the middleware. Checks the rate limit for the client's IP and either allows
    /// the request or returns 429 Too Many Requests.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for specific paths
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var limit = context.User?.Identity?.IsAuthenticated ?? false
            ? _options.AuthenticatedRequestsPerMinute
            : _options.AnonymousRequestsPerMinute;

        var bucket = _buckets.AddOrUpdate(clientId,
            new TokenBucket(limit),
            (_, existing) => existing);

        if (!bucket.AllowRequest(limit))
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Too Many Requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = bucket.GetRetryAfterSeconds()
            });
            return;
        }

        // Add rate limit headers to response
        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = bucket.GetRemainingTokens(limit).ToString();

        await _next(context);
    }

    /// <summary>
    /// Extracts the client identifier from the request. Uses user ID if authenticated,
    /// otherwise uses IP address. This allows per-user rate limiting for API clients.
    /// </summary>
    private string GetClientIdentifier(HttpContext context)
    {
        if (context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            is string userId && !string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{remoteIp}";
    }
}

/// <summary>
/// Token bucket implementation for rate limiting. Implements the token bucket algorithm
/// where tokens are refilled at a fixed rate. Requests consume one token.
/// </summary>
public class TokenBucket
{
    private double _tokens;
    private DateTime _lastRefillTime;
    private const double TokensPerSecond = 1.0 / 60.0; // One request per second max

    public TokenBucket(int capacity)
    {
        _tokens = capacity;
        _lastRefillTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if a request is allowed. Refills tokens based on elapsed time,
    /// then consumes one token if available.
    /// </summary>
    public bool AllowRequest(int capacity)
    {
        RefillTokens(capacity);
        if (_tokens >= 1)
        {
            _tokens--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Refills the token bucket based on elapsed time since last refill.
    /// </summary>
    private void RefillTokens(int capacity)
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastRefillTime).TotalSeconds;
        var tokensToAdd = elapsed * TokensPerSecond;

        _tokens = Math.Min(capacity, _tokens + tokensToAdd);
        _lastRefillTime = now;
    }

    /// <summary>
    /// Returns the number of remaining tokens for the current window.
    /// </summary>
    public int GetRemainingTokens(int capacity)
    {
        RefillTokens(capacity);
        return (int)Math.Floor(_tokens);
    }

    /// <summary>
    /// Returns the number of seconds to wait before the next request is allowed.
    /// </summary>
    public int GetRetryAfterSeconds()
    {
        if (_tokens >= 1)
            return 0;

        var tokensNeeded = 1 - _tokens;
        var secondsNeeded = tokensNeeded / TokensPerSecond;
        return (int)Math.Ceiling(secondsNeeded);
    }
}

/// <summary>
/// Configuration options for rate limiting middleware.
/// </summary>
public class RateLimitOptions
{
    public int AnonymousRequestsPerMinute { get; set; } = 60;
    public int AuthenticatedRequestsPerMinute { get; set; } = 300;
}
