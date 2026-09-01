#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;
using System.Threading;

namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Rate limiting middleware that implements a simple token bucket algorithm per IP address.
/// Prevents abuse by limiting the number of requests per time window. Configuration allows
/// different limits for authenticated vs anonymous requests.
/// </summary>
public class RateLimitingMiddleware : IRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, TokenBucketState> _buckets = new();

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
        try
        {
            _logger.LogInformation("Processing request {Method} {Path}", context.Request.Method, context.Request.Path);

            // Skip rate limiting for specific paths
            if (context.Request.Path.StartsWithSegments(RateLimitingMiddlewareConstants.HealthCheckPath))
            {
                _logger.LogInformation("Skipping rate limiting for health check path {Path}", context.Request.Path);
                await _next(context);
                return;
            }

            var clientId = GetClientIdentifier(context);
            var limit = context.User?.Identity?.IsAuthenticated ?? false
                ? _options.AuthenticatedRequestsPerMinute
                : _options.AnonymousRequestsPerMinute;

            _logger.LogInformation("Checking rate limit for client {ClientId} with limit {Limit} per minute", clientId, limit);

            var bucket = _buckets.GetOrAdd(clientId, _ => new TokenBucketState(limit));

            if (!bucket.TryTakeToken())
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
                context.Response.StatusCode = RateLimitingMiddlewareConstants.StatusCodeTooManyRequests;
                context.Response.ContentType = RateLimitingMiddlewareConstants.JsonContentType;
                var retryAfter = bucket.GetRetryAfterSeconds();
                context.Response.Headers[RateLimitingMiddlewareConstants.RetryAfterHeaderName] = retryAfter.ToString();

                await context.Response.WriteAsJsonAsync(new
                {
                    error = RateLimitingMiddlewareConstants.TooManyRequestsError,
                    message = RateLimitingMiddlewareConstants.RateLimitExceededMessage,
                    retryAfter = retryAfter
                });
                return;
            }

            // Add rate limit headers to response
            context.Response.Headers[RateLimitingMiddlewareConstants.RateLimitLimitHeaderName] = limit.ToString();
            context.Response.Headers[RateLimitingMiddlewareConstants.RateLimitRemainingHeaderName] = bucket.GetRemainingTokens().ToString();

            await _next(context);

            _logger.LogInformation("Request {Method} {Path} processed successfully", context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request for {Path}", context.Request.Path);
            throw;
        }
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
            return $"{RateLimitingMiddlewareConstants.UserPrefix}{userId}";
        }

        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? RateLimitingMiddlewareConstants.UnknownIp;
        return $"{RateLimitingMiddlewareConstants.IpPrefix}{remoteIp}";
    }

    /// <summary>
    /// Token bucket state for a specific client.
    /// </summary>
    private sealed class TokenBucketState
    {
        public double Tokens { get; set; }
        public DateTime LastRefillTime { get; set; }
        public int Capacity { get; set; }

        public TokenBucketState(int capacity)
        {
            Capacity = capacity;
            Tokens = capacity;
            LastRefillTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Atomically checks if a request is allowed and decrements tokens if available.
        /// </summary>
        public bool TryTakeToken()
        {
            lock (this)
            {
                RefillTokens();
                if (Tokens >= 1)
                {
                    Tokens--;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Atomically gets remaining tokens.
        /// </summary>
        public int GetRemainingTokens()
        {
            lock (this)
            {
                RefillTokens();
                return (int)Math.Floor(Tokens);
            }
        }

        /// <summary>
        /// Atomically gets the retry-after seconds.
        /// </summary>
        public int GetRetryAfterSeconds()
        {
            lock (this)
            {
                if (Tokens >= 1)
                    return 0;

                var tokensNeeded = 1 - Tokens;
                var secondsNeeded = tokensNeeded / RateLimitingMiddlewareConstants.TokensPerSecond;
                return (int)Math.Ceiling(secondsNeeded);
            }
        }

        /// <summary>
        /// Refills the token bucket based on elapsed time since last refill.
        /// </summary>
        private void RefillTokens()
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - LastRefillTime).TotalSeconds;
            var tokensToAdd = elapsed * RateLimitingMiddlewareConstants.TokensPerSecond;

            Tokens = Math.Min(Capacity, Tokens + tokensToAdd);
            LastRefillTime = now;
        }

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