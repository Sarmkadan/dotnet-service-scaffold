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
        ValidationUtility.ValidateNotNullOrEmpty(username, nameof(username));
        ValidationUtility.ValidateNotNullOrEmpty(password, nameof(password));

        var credentials = $"{username}:{password}";
        var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        return $"Basic {encodedCredentials}";
    }

    /// <summary>
    /// Creates a Bearer token authorization header value.
    /// </summary>
    public static string CreateBearerAuthHeader(string token)
    {
        ValidationUtility.ValidateNotNullOrEmpty(token, nameof(token));
        return $"Bearer {token}";
    }

    /// <summary>
    /// Parses a Basic authorization header and returns username and password.
    /// Returns null if header is invalid.
    /// </summary>
    public static (string Username, string Password)? ParseBasicAuthHeader(string? header)
    {
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            var base64 = header["Basic ".Length..];
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
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        return header["Bearer ".Length..].Trim();
    }

    /// <summary>
    /// Builds a query string from a dictionary of parameters.
    /// Properly URL-encodes all values.
    /// </summary>
    public static string BuildQueryString(Dictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return string.Empty;

        var query = string.Join("&", parameters
            .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}")
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

        var query = queryString.StartsWith("?") ? queryString[1..] : queryString;

        foreach (var param in query.Split('&'))
        {
            var parts = param.Split('=', 2);
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
        return statusCode >= 200 && statusCode < 300;
    }

    /// <summary>
    /// Checks if a status code indicates a client error (400-499).
    /// </summary>
    public static bool IsClientErrorStatusCode(int statusCode)
    {
        return statusCode >= 400 && statusCode < 500;
    }

    /// <summary>
    /// Checks if a status code indicates a server error (500-599).
    /// </summary>
    public static bool IsServerErrorStatusCode(int statusCode)
    {
        return statusCode >= 500 && statusCode < 600;
    }

    /// <summary>
    /// Checks if a status code is retryable (429, 408, 500, 502, 503, 504).
    /// Indicates the request should be retried after waiting.
    /// </summary>
    public static bool IsRetryableStatusCode(int statusCode)
    {
        return statusCode switch
        {
            408 => true, // Request Timeout
            429 => true, // Too Many Requests
            500 => true, // Internal Server Error
            502 => true, // Bad Gateway
            503 => true, // Service Unavailable
            504 => true, // Gateway Timeout
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

        // Exponential backoff with jitter: 100ms * 2^attempt + random 0-100ms
        var baseDelay = 100 * Math.Pow(2, Math.Min(attempt, 5));
        var jitter = Random.Shared.Next(0, 100); // Fix: use thread-safe shared Random instance
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
        var url = baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');

        if (queryParams != null && queryParams.Count > 0)
        {
            url += "?" + BuildQueryString(queryParams);
        }

        return url;
    }

    /// <summary>
    /// Formats a URL for logging, masking sensitive query parameters.
    /// </summary>
    public static string MaskSensitiveUrl(string url)
    {
        var sensitiveParams = new[] { "password", "token", "api_key", "secret", "key" };

        try
        {
            var uri = new Uri(url);
            var query = ParseQueryString(uri.Query);

            foreach (var sensitiveParam in sensitiveParams)
            {
                if (query.ContainsKey(sensitiveParam))
                {
                    query[sensitiveParam] = "***MASKED***";
                }
            }

            var maskedQuery = BuildQueryString(query);
            return uri.Scheme + "://" + uri.Host + uri.AbsolutePath +
                   (string.IsNullOrEmpty(maskedQuery) ? string.Empty : "?" + maskedQuery);
        }
        catch
        {
            return url;
        }
    }
}
