#nullable enable

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Constants used in HttpUtility.
/// </summary>
internal static class HttpUtilityConstants
{
    // Authentication prefixes
    public const string BasicPrefix = "Basic ";
    public const string BearerPrefix = "Bearer ";

    // Query string separators
    public const string Ampersand = "&";
    public const string Equals = "=";
    public const string QuestionMark = "?";

    // Content-Type separators
    public const string Semicolon = ";";
    public const string Colon = ":";

    // URL components
    public const string ForwardSlash = "/";
    public const string ColonSlashSlash = "://";

    // Masking
    public const string MaskedValue = "***MASKED***";

    // HTTP status code ranges
    public const int MinSuccessStatusCode = 200;
    public const int MaxSuccessStatusCode = 300;
    public const int MinClientErrorStatusCode = 400;
    public const int MaxClientErrorStatusCode = 500;
    public const int MinServerErrorStatusCode = 500;
    public const int MaxServerErrorStatusCode = 600;

    // Retryable status codes
    public const int StatusCodeRequestTimeout = 408;
    public const int StatusCodeTooManyRequests = 429;
    public const int StatusCodeInternalServerError = 500;
    public const int StatusCodeBadGateway = 502;
    public const int StatusCodeServiceUnavailable = 503;
    public const int StatusCodeGatewayTimeout = 504;

    // Retry delay configuration
    public const int BaseDelayMultiplier = 100;
    public const int MaxAttemptExponent = 5;
    public const int JitterMax = 100;
}