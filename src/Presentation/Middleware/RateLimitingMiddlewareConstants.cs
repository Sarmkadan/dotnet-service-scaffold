namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Constants for RateLimitingMiddleware.
/// </summary>
internal static class RateLimitingMiddlewareConstants
{
    public const string HealthCheckPath = "/health";
    public const string JsonContentType = "application/json";
    public const string RetryAfterHeaderName = "Retry-After";
    public const string TooManyRequestsError = "Too Many Requests";
    public const string RateLimitExceededMessage = "Rate limit exceeded. Please try again later.";
    public const string RateLimitLimitHeaderName = "X-RateLimit-Limit";
    public const string RateLimitRemainingHeaderName = "X-RateLimit-Remaining";
    public const string UserPrefix = "user:";
    public const string IpPrefix = "ip:";
    public const string UnknownIp = "unknown";

    public const int StatusCodeTooManyRequests = 429;
    public const int One = 1;
    public const int SecondsPerMinute = 60;
    public static readonly double TokensPerSecond = 1.0 / SecondsPerMinute;
}