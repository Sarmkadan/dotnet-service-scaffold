#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Utility class for HTTP operations. Provides helpers for building requests,
/// parsing responses, and handling common HTTP patterns. Simplifies HttpClient usage.
/// </summary>
public static class HttpUtility
{
    /// <summary>
    /// Creates a properly formatted Basic authentication header value.
    /// </summary>
    public static string CreateBasicAuthHeader(string username, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var credentials = $"{username}:{password}";
        var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        return $"{HttpUtilityConstants.BasicPrefix}{encodedCredentials}";
    }

    /// <summary>
    /// Creates a Bearer token authorization header value.
    /// </summary>
    public static string CreateBearerAuthHeader(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        return $"{HttpUtilityConstants.BearerPrefix}{token}";
    }

    /// <summary>
    /// Parses a Basic authorization header and returns username and password.
    /// Returns null if header is invalid.
    /// </summary>
    public static (string Username, string Password)? ParseBasicAuthHeader(string? header)
    {
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(HttpUtilityConstants.BasicPrefix, StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            var base64 = header[HttpUtilityConstants.BasicPrefix.Length..];
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            var parts = credentials.Split(':', 2);

            if (parts.Length != 2)
                return null;

            return (parts[0], parts[1]);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts the bearer token from an authorization header.
    /// Returns null if header is invalid.
    /// </summary>
    public static string? ParseBearerToken(string? header)
    {
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(HttpUtilityConstants.BearerPrefix, StringComparison.OrdinalIgnoreCase))
            return null;

        return header[HttpUtilityConstants.BearerPrefix.Length..].Trim();
    }

    /// <summary>
    /// Builds a query string from a dictionary of parameters.
    /// Properly URL-encodes all values.
    /// </summary>
    public static string BuildQueryString(Dictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (parameters.Count == 0)
            return string.Empty;

        var query = string.Join(HttpUtilityConstants.Ampersand, parameters
            .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}{HttpUtilityConstants.Equals}{Uri.EscapeDataString(kvp.Value)}")
        );

        return query;
    }

    /// <summary>
    /// Parses a query string into a dictionary of parameters.
    /// </summary>
    public static Dictionary<string, string> ParseQueryString(string? queryString)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(queryString))
            return result;

        var query = queryString.StartsWith(HttpUtilityConstants.QuestionMark) ? queryString[HttpUtilityConstants.QuestionMark.Length..] : queryString;

        foreach (var param in query.Split(HttpUtilityConstants.Ampersand))
        {
            var parts = param.Split(HttpUtilityConstants.Equals, 2);
            if (parts.Length == 2)
            {
                var key = Uri.UnescapeDataString(parts[0]);
                var value = Uri.UnescapeDataString(parts[1]);
                result[key] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a status code indicates success (200-299).
    /// </summary>
    public static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode >= HttpUtilityConstants.MinSuccessStatusCode && statusCode < HttpUtilityConstants.MaxSuccessStatusCode;
    }

    /// <summary>
    /// Checks if a status code indicates a client error (400-499).
    /// </summary>
    public static bool IsClientErrorStatusCode(int statusCode)
    {
        return statusCode >= HttpUtilityConstants.MinClientErrorStatusCode && statusCode < HttpUtilityConstants.MaxClientErrorStatusCode;
    }

    /// <summary>
    /// Checks if a status code indicates a server error (500-599).
    /// </summary>
    public static bool IsServerErrorStatusCode(int statusCode)
    {
        return statusCode >= HttpUtilityConstants.MinServerErrorStatusCode && statusCode < HttpUtilityConstants.MaxServerErrorStatusCode;
    }

    /// <summary>
    /// Checks if a status code is retryable (429, 408, 500, 502, 503, 504).
    /// Indicates the request should be retried after waiting.
    /// </summary>
    public static bool IsRetryableStatusCode(int statusCode)
    {
        return statusCode switch
        {
            HttpUtilityConstants.StatusCodeRequestTimeout => true, // Request Timeout
            HttpUtilityConstants.StatusCodeTooManyRequests => true, // Too Many Requests
            HttpUtilityConstants.StatusCodeInternalServerError => true, // Internal Server Error
            HttpUtilityConstants.StatusCodeBadGateway => true, // Bad Gateway
            HttpUtilityConstants.StatusCodeServiceUnavailable => true, // Service Unavailable
            HttpUtilityConstants.StatusCodeGatewayTimeout => true, // Gateway Timeout
            _ => false
        };
    }

    /// <summary>
    /// Gets the recommended retry delay in milliseconds for a given HTTP status code.
    /// Returns null if status code is not retryable.
    /// </summary>
    public static int? GetRetryDelayMs(int statusCode, int attempt = 1)
    {
        if (!IsRetryableStatusCode(statusCode))
            return null;

        // Exponential backoff with jitter: BaseDelayMultiplier * 2^attempt + random 0-JitterMax
        var baseDelay = HttpUtilityConstants.BaseDelayMultiplier * Math.Pow(2, Math.Min(attempt, HttpUtilityConstants.MaxAttemptExponent));
        var jitter = Random.Shared.Next(0, HttpUtilityConstants.JitterMax); // Fix: use thread-safe shared Random instance
        return (int)(baseDelay + jitter);
    }

    /// <summary>
    /// Extracts the content type from a Content-Type header.
    /// Example: "text/html; charset=utf-8" returns "text/html"
    /// </summary>
    public static string? GetMediaType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return null;

        var parts = contentType.Split(';');
        return parts[0].Trim();
    }

    /// <summary>
    /// Extracts the charset from a Content-Type header.
    /// Example: "text/html; charset=utf-8" returns "utf-8"
    /// </summary>
    public static string? GetCharset(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return null;

        var parts = contentType.Split(';');
        if (parts.Length < 2)
            return null;

        var charset = parts[1].Split('=');
        if (charset.Length != 2)
            return null;

        return charset[1].Trim().Trim('"');
    }

    /// <summary>
    /// Builds a URL with path and query parameters.
    /// </summary>
    public static string BuildUrl(string baseUrl, string path, Dictionary<string, string>? queryParams = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseUrl);
        ArgumentException.ThrowIfNullOrEmpty(path);

        var url = baseUrl.TrimEnd(HttpUtilityConstants.ForwardSlash[0]) + HttpUtilityConstants.ForwardSlash + path.TrimStart(HttpUtilityConstants.ForwardSlash[0]);

        if (queryParams is not null && queryParams.Count > 0)
        {
            url += HttpUtilityConstants.QuestionMark + BuildQueryString(queryParams);
        }

        return url;
    }

    /// <summary>
    /// Formats a URL for logging, masking sensitive query parameters.
    /// </summary>
    public static string MaskSensitiveUrl(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        var sensitiveParams = new[] { "password", "token", "api_key", "secret", "key" };

        try
        {
            var uri = new Uri(url);
            var query = ParseQueryString(uri.Query);

            foreach (var sensitiveParam in sensitiveParams)
            {
                if (query.ContainsKey(sensitiveParam))
                {
                    query[sensitiveParam] = HttpUtilityConstants.MaskedValue;
                }
            }

            var maskedQuery = BuildQueryString(query);
            return uri.Scheme + HttpUtilityConstants.ColonSlashSlash + uri.Host + uri.AbsolutePath +
                   (string.IsNullOrEmpty(maskedQuery) ? string.Empty : HttpUtilityConstants.QuestionMark + maskedQuery);
        }
        catch
        {
            return url;
        }
    }
}
