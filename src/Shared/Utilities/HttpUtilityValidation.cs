#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="HttpUtility"/> class methods.
/// Validates method arguments to ensure correct usage of HttpUtility utilities.
/// </summary>
public static class HttpUtilityValidation
{
    /// <summary>
    /// Validates username and password for Basic authentication.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <param name="password">The password to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if username or password is null.</exception>
    public static IReadOnlyList<string> ValidateBasicAuth(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        var problems = new List<string>();

        if (username.Length > 256)
        {
            problems.Add("Username exceeds maximum length of 256 characters.");
        }

        if (username.Contains(':'))
        {
            problems.Add("Username contains colon character which is not allowed in Basic authentication.");
        }

        if (username.Contains('\0'))
        {
            problems.Add("Username contains null character which is not allowed.");
        }

        if (username.Any(c => c > 127))
        {
            problems.Add("Username contains non-ASCII characters which may cause interoperability issues.");
        }

        if (password.Length > 256)
        {
            problems.Add("Password exceeds maximum length of 256 characters.");
        }

        if (password.Contains('\0'))
        {
            problems.Add("Password contains null character which is not allowed.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a username for Basic authentication.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if username is null.</exception>
    public static IReadOnlyList<string> ValidateBasicAuthUsername(string username)
    {
        ArgumentNullException.ThrowIfNull(username);

        var problems = new List<string>();

        if (username.Length > 256)
        {
            problems.Add("Username exceeds maximum length of 256 characters.");
        }

        if (username.Contains(':'))
        {
            problems.Add("Username contains colon character which is not allowed in Basic authentication.");
        }

        if (username.Contains('\0'))
        {
            problems.Add("Username contains null character which is not allowed.");
        }

        if (username.Any(c => c > 127))
        {
            problems.Add("Username contains non-ASCII characters which may cause interoperability issues.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a password for Basic authentication.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if password is null.</exception>
    public static IReadOnlyList<string> ValidateBasicAuthPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var problems = new List<string>();

        if (password.Length > 256)
        {
            problems.Add("Password exceeds maximum length of 256 characters.");
        }

        if (password.Contains('\0'))
        {
            problems.Add("Password contains null character which is not allowed.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a bearer token.
    /// </summary>
    /// <param name="token">The bearer token to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateBearerToken(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var problems = new List<string>();

        if (token.Length > 4096)
        {
            problems.Add("Bearer token exceeds maximum length of 4096 characters.");
        }

        if (token.Contains('\0'))
        {
            problems.Add("Bearer token contains null character which is not allowed.");
        }

        if (token.Any(c => c > 127))
        {
            problems.Add("Bearer token contains non-ASCII characters which may cause interoperability issues.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The status code to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateStatusCode(int statusCode)
    {
        var problems = new List<string>();

        if (statusCode < 100 || statusCode > 599)
        {
            problems.Add("Status code must be between 100 and 599 inclusive.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a base URL.
    /// </summary>
    /// <param name="baseUrl">The base URL to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateBaseUrl(string baseUrl)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);

        var problems = new List<string>();

        try
        {
            var uri = new Uri(baseUrl);

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                problems.Add("Base URL must use http or https scheme.");
            }

            if (string.IsNullOrWhiteSpace(uri.Host))
            {
                problems.Add("Base URL must contain a valid host.");
            }

            if (uri.Port < 0 || uri.Port > 65535)
            {
                problems.Add("Base URL contains invalid port number.");
            }
        }
        catch (UriFormatException)
        {
            problems.Add("Base URL is not a valid URI format.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a path component for URL building.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidatePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var problems = new List<string>();

        if (path.Length > 2048)
        {
            problems.Add("Path exceeds maximum length of 2048 characters.");
        }

        if (path.Contains(".."))
        {
            problems.Add("Path contains relative traversal which is not allowed.");
        }

        if (path.Contains('\0'))
        {
            problems.Add("Path contains null character which is not allowed.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates query parameters for URL building.
    /// </summary>
    /// <param name="parameters">The query parameters to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateQueryParameters(Dictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var problems = new List<string>();

        foreach (var kvp in parameters)
        {
            ArgumentNullException.ThrowIfNull(kvp.Key);
            ArgumentNullException.ThrowIfNull(kvp.Value);

            if (kvp.Key.Length > 1024)
            {
                problems.Add("Query parameter key exceeds maximum length of 1024 characters.");
            }

            if (kvp.Value.Length > 4096)
            {
                problems.Add("Query parameter value exceeds maximum length of 4096 characters.");
            }

            if (kvp.Key.Contains('\0') || kvp.Value.Contains('\0'))
            {
                problems.Add("Query parameters contain null character which is not allowed.");
                break;
            }
        }

        if (parameters.Count > 100)
        {
            problems.Add("Query parameters exceed maximum count of 100.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a Content-Type header value.
    /// </summary>
    /// <param name="contentType">The Content-Type header value to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateContentType(string contentType)
    {
        ArgumentNullException.ThrowIfNull(contentType);

        var problems = new List<string>();

        if (contentType.Length > 256)
        {
            problems.Add("Content-Type header exceeds maximum length of 256 characters.");
        }

        if (contentType.Contains('\0'))
        {
            problems.Add("Content-Type header contains null character which is not allowed.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a header string.
    /// </summary>
    /// <param name="header">The header string to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateHeader(string header)
    {
        ArgumentNullException.ThrowIfNull(header);

        var problems = new List<string>();

        if (header.Length > 8192)
        {
            problems.Add("Header exceeds maximum length of 8192 characters.");
        }

        if (header.Contains('\0'))
        {
            problems.Add("Header contains null character which is not allowed.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a retry attempt number.
    /// </summary>
    /// <param name="attempt">The retry attempt number (1-based).</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRetryAttempt(int attempt)
    {
        var problems = new List<string>();

        if (attempt < 1)
        {
            problems.Add("Retry attempt must be 1 or greater.");
        }

        if (attempt > 20)
        {
            problems.Add("Retry attempt exceeds maximum of 20.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether username and password are valid for Basic authentication.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <param name="password">The password to check.</param>
    /// <returns>True if the credentials are valid; otherwise, false.</returns>
    public static bool IsValidBasicAuth(string username, string password)
    {
        return ValidateBasicAuth(username, password).Count == 0;
    }

    /// <summary>
    /// Determines whether a bearer token is valid.
    /// </summary>
    /// <param name="token">The bearer token to check.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    public static bool IsValidBearerToken(string token)
    {
        return ValidateBearerToken(token).Count == 0;
    }

    /// <summary>
    /// Determines whether a status code is valid.
    /// </summary>
    /// <param name="statusCode">The status code to check.</param>
    /// <returns>True if the status code is valid; otherwise, false.</returns>
    public static bool IsValidStatusCode(int statusCode)
    {
        return ValidateStatusCode(statusCode).Count == 0;
    }

    /// <summary>
    /// Determines whether a base URL is valid.
    /// </summary>
    /// <param name="baseUrl">The base URL to check.</param>
    /// <returns>True if the base URL is valid; otherwise, false.</returns>
    public static bool IsValidBaseUrl(string baseUrl)
    {
        return ValidateBaseUrl(baseUrl).Count == 0;
    }

    /// <summary>
    /// Determines whether a path is valid.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is valid; otherwise, false.</returns>
    public static bool IsValidPath(string path)
    {
        return ValidatePath(path).Count == 0;
    }

    /// <summary>
    /// Determines whether query parameters are valid.
    /// </summary>
    /// <param name="parameters">The query parameters to check.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsValidQueryParameters(Dictionary<string, string> parameters)
    {
        return ValidateQueryParameters(parameters).Count == 0;
    }

    /// <summary>
    /// Determines whether a Content-Type header is valid.
    /// </summary>
    /// <param name="contentType">The Content-Type header to check.</param>
    /// <returns>True if the Content-Type header is valid; otherwise, false.</returns>
    public static bool IsValidContentType(string contentType)
    {
        return ValidateContentType(contentType).Count == 0;
    }

    /// <summary>
    /// Determines whether a header is valid.
    /// </summary>
    /// <param name="header">The header to check.</param>
    /// <returns>True if the header is valid; otherwise, false.</returns>
    public static bool IsValidHeader(string header)
    {
        return ValidateHeader(header).Count == 0;
    }

    /// <summary>
    /// Determines whether a retry attempt is valid.
    /// </summary>
    /// <param name="attempt">The retry attempt to check.</param>
    /// <returns>True if the attempt is valid; otherwise, false.</returns>
    public static bool IsValidRetryAttempt(int attempt)
    {
        return ValidateRetryAttempt(attempt).Count == 0;
    }

    /// <summary>
    /// Ensures that username and password are valid for Basic authentication, throwing an exception if not.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <param name="password">The password to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the credentials are invalid, containing the validation problems.</exception>
    public static void EnsureValidBasicAuth(string username, string password)
    {
        var problems = ValidateBasicAuth(username, password);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Basic authentication credentials are invalid. " +
                string.Join(" ", problems),
                nameof(username));
        }
    }

    /// <summary>
    /// Ensures that a bearer token is valid, throwing an exception if not.
    /// </summary>
    /// <param name="token">The bearer token to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the token is invalid, containing the validation problems.</exception>
    public static void EnsureValidBearerToken(string token)
    {
        var problems = ValidateBearerToken(token);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Bearer token is invalid. " +
                string.Join(" ", problems),
                nameof(token));
        }
    }

    /// <summary>
    /// Ensures that a status code is valid, throwing an exception if not.
    /// </summary>
    /// <param name="statusCode">The status code to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the status code is invalid, containing the validation problems.</exception>
    public static void EnsureValidStatusCode(int statusCode)
    {
        var problems = ValidateStatusCode(statusCode);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Status code is invalid. " +
                string.Join(" ", problems),
                nameof(statusCode));
        }
    }

    /// <summary>
    /// Ensures that a base URL is valid, throwing an exception if not.
    /// </summary>
    /// <param name="baseUrl">The base URL to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the base URL is invalid, containing the validation problems.</exception>
    public static void EnsureValidBaseUrl(string baseUrl)
    {
        var problems = ValidateBaseUrl(baseUrl);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Base URL is invalid. " +
                string.Join(" ", problems),
                nameof(baseUrl));
        }
    }

    /// <summary>
    /// Ensures that a path is valid, throwing an exception if not.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the path is invalid, containing the validation problems.</exception>
    public static void EnsureValidPath(string path)
    {
        var problems = ValidatePath(path);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Path is invalid. " +
                string.Join(" ", problems),
                nameof(path));
        }
    }

    /// <summary>
    /// Ensures that query parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="parameters">The query parameters to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid, containing the validation problems.</exception>
    public static void EnsureValidQueryParameters(Dictionary<string, string> parameters)
    {
        var problems = ValidateQueryParameters(parameters);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Query parameters are invalid. " +
                string.Join(" ", problems),
                nameof(parameters));
        }
    }

    /// <summary>
    /// Ensures that a Content-Type header is valid, throwing an exception if not.
    /// </summary>
    /// <param name="contentType">The Content-Type header to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the Content-Type header is invalid, containing the validation problems.</exception>
    public static void EnsureValidContentType(string contentType)
    {
        var problems = ValidateContentType(contentType);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Content-Type header is invalid. " +
                string.Join(" ", problems),
                nameof(contentType));
        }
    }

    /// <summary>
    /// Ensures that a header is valid, throwing an exception if not.
    /// </summary>
    /// <param name="header">The header to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the header is invalid, containing the validation problems.</exception>
    public static void EnsureValidHeader(string header)
    {
        var problems = ValidateHeader(header);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Header is invalid. " +
                string.Join(" ", problems),
                nameof(header));
        }
    }

    /// <summary>
    /// Ensures that a retry attempt is valid, throwing an exception if not.
    /// </summary>
    /// <param name="attempt">The retry attempt to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the attempt is invalid, containing the validation problems.</exception>
    public static void EnsureValidRetryAttempt(int attempt)
    {
        var problems = ValidateRetryAttempt(attempt);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Retry attempt is invalid. " +
                string.Join(" ", problems),
                nameof(attempt));
        }
    }
}