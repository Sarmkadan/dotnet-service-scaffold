#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

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
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="username"/> or <paramref name="password"/> is null.</exception>
    public static IReadOnlyList<string> ValidateBasicAuth(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        var problems = new List<string>();

        if (username.Length > HttpUtilityValidationConstants.MaxBasicAuthLength)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.UsernameExceedsMaxLength, HttpUtilityValidationConstants.MaxBasicAuthLength));
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

        return problems;
    }

    /// <summary>
    /// Validates a username for Basic authentication.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="username"/> is null.</exception>
    public static IReadOnlyList<string> ValidateBasicAuthUsername(string username)
    {
        ArgumentNullException.ThrowIfNull(username);

        var problems = new List<string>();

        if (username.Length > HttpUtilityValidationConstants.MaxBasicAuthLength)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.UsernameExceedsMaxLength, HttpUtilityValidationConstants.MaxBasicAuthLength));
        }

        if (username.Contains(':'))
        {
            problems.Add(HttpUtilityValidationConstants.UsernameContainsColon);
        }

        if (username.Contains('\0'))
        {
            problems.Add(HttpUtilityValidationConstants.UsernameContainsNull);
        }

        if (username.Any(c => c > 127))
        {
            problems.Add(HttpUtilityValidationConstants.UsernameContainsNonAscii);
        }

        return problems;
    }

    /// <summary>
    /// Validates a password for Basic authentication.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
    public static IReadOnlyList<string> ValidateBasicAuthPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var problems = new List<string>();

        if (password.Length > HttpUtilityValidationConstants.MaxBasicAuthLength)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.PasswordExceedsMaxLength, HttpUtilityValidationConstants.MaxBasicAuthLength));
        }

        if (password.Contains('\0'))
        {
            problems.Add(HttpUtilityValidationConstants.PasswordContainsNull);
        }

        return problems;
    }

    /// <summary>
    /// Validates a bearer token.
    /// </summary>
    /// <param name="token">The bearer token to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="token"/> is null.</exception>
    public static IReadOnlyList<string> ValidateBearerToken(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var problems = new List<string>();

        if (token.Length > HttpUtilityValidationConstants.MaxBearerTokenLength)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.BearerTokenExceedsMaxLength, HttpUtilityValidationConstants.MaxBearerTokenLength));
        }

        if (token.Contains('\0'))
        {
            problems.Add(HttpUtilityValidationConstants.BearerTokenContainsNull);
        }

        if (token.Any(c => c > 127))
        {
            problems.Add(HttpUtilityValidationConstants.BearerTokenContainsNonAscii);
        }

        return problems;
    }

    /// <summary>
    /// Validates HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The status code to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateStatusCode(int statusCode)
    {
        var problems = new List<string>();

        if (statusCode < HttpUtilityValidationConstants.MinStatusCode || statusCode > HttpUtilityValidationConstants.MaxStatusCode)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.StatusCodeOutOfRange, HttpUtilityValidationConstants.MinStatusCode, HttpUtilityValidationConstants.MaxStatusCode));
        }

        return problems;
    }

    /// <summary>
    /// Validates a base URL.
    /// </summary>
    /// <param name="baseUrl">The base URL to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateBaseUrl(string baseUrl)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);

        var problems = new List<string>();

        try
        {
            var uri = new Uri(baseUrl);

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                problems.Add(HttpUtilityValidationConstants.BaseUrlInvalidScheme);
            }

            if (string.IsNullOrWhiteSpace(uri.Host))
            {
                problems.Add(HttpUtilityValidationConstants.BaseUrlMissingHost);
            }

            if (uri.Port < HttpUtilityValidationConstants.MinPortNumber || uri.Port > HttpUtilityValidationConstants.MaxPortNumber)
            {
                problems.Add(HttpUtilityValidationConstants.BaseUrlInvalidPort);
            }
        }
        catch (UriFormatException)
        {
            problems.Add(HttpUtilityValidationConstants.BaseUrlInvalidFormat);
        }

        return problems;
    }

    /// <summary>
    /// Validates a path component for URL building.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidatePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var problems = new List<string>();

        if (path.Length > HttpUtilityValidationConstants.MaxPathLength)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.PathExceedsMaxLength, HttpUtilityValidationConstants.MaxPathLength));
        }

        if (path.Contains(".."))
        {
            problems.Add(HttpUtilityValidationConstants.PathContainsRelativeTraversal);
        }

        if (path.Contains('\0'))
        {
            problems.Add(HttpUtilityValidationConstants.PathContainsNull);
        }

        return problems;
    }

    /// <summary>
    /// Validates query parameters for URL building.
    /// </summary>
    /// <param name="parameters">The query parameters to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateQueryParameters(Dictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var problems = new List<string>();

        foreach (var kvp in parameters)
        {
            ArgumentNullException.ThrowIfNull(kvp.Key);
            ArgumentNullException.ThrowIfNull(kvp.Value);

            if (kvp.Key.Length > HttpUtilityValidationConstants.MaxQueryParameterKeyLength)
            {
                problems.Add(string.Format(HttpUtilityValidationConstants.QueryParameterKeyExceedsMaxLength, HttpUtilityValidationConstants.MaxQueryParameterKeyLength));
            }

            if (kvp.Value.Length > HttpUtilityValidationConstants.MaxQueryParameterValueLength)
            {
                problems.Add(string.Format(HttpUtilityValidationConstants.QueryParameterValueExceedsMaxLength, HttpUtilityValidationConstants.MaxQueryParameterValueLength));
            }

            if (kvp.Key.Contains('\0') || kvp.Value.Contains('\0'))
            {
                problems.Add(HttpUtilityValidationConstants.QueryParametersContainNull);
                break;
            }
        }

        if (parameters.Count > HttpUtilityValidationConstants.MaxQueryParameterCount)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.QueryParametersExceedMaxCount, HttpUtilityValidationConstants.MaxQueryParameterCount));
        }

        return problems;
    }

    /// <summary>
    /// Validates a Content-Type header value.
    /// </summary>
    /// <param name="contentType">The Content-Type header value to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateContentType(string contentType)
    {
        ArgumentNullException.ThrowIfNull(contentType);

        var problems = new List<string>();

        if (contentType.Length > HttpUtilityValidationConstants.MaxContentTypeLength)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.ContentTypeExceedsMaxLength, HttpUtilityValidationConstants.MaxContentTypeLength));
        }

        if (contentType.Contains('\0'))
        {
            problems.Add(HttpUtilityValidationConstants.ContentTypeContainsNull);
        }

        return problems;
    }

    /// <summary>
    /// Validates a header string.
    /// </summary>
    /// <param name="header">The header string to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateHeader(string header)
    {
        ArgumentNullException.ThrowIfNull(header);

        var problems = new List<string>();

        if (header.Length > HttpUtilityValidationConstants.MaxHeaderLength)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.HeaderExceedsMaxLength, HttpUtilityValidationConstants.MaxHeaderLength));
        }

        if (header.Contains('\0'))
        {
            problems.Add(HttpUtilityValidationConstants.HeaderContainsNull);
        }

        return problems;
    }

    /// <summary>
    /// Validates a retry attempt number.
    /// </summary>
    /// <param name="attempt">The retry attempt number (1-based).</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRetryAttempt(int attempt)
    {
        var problems = new List<string>();

        if (attempt < HttpUtilityValidationConstants.MinRetryAttempt)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.RetryAttemptTooLow, HttpUtilityValidationConstants.MinRetryAttempt));
        }

        if (attempt > HttpUtilityValidationConstants.MaxRetryAttempt)
        {
            problems.Add(string.Format(HttpUtilityValidationConstants.RetryAttemptTooHigh, HttpUtilityValidationConstants.MaxRetryAttempt));
        }

        return problems;
    }

    /// <summary>
    /// Determines whether username and password are valid for Basic authentication.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <param name="password">The password to check.</param>
    /// <returns>True if the credentials are valid; otherwise, false.</returns>
    public static bool IsValidBasicAuth(string username, string password) => ValidateBasicAuth(username, password).Count == 0;

    /// <summary>
    /// Determines whether a bearer token is valid.
    /// </summary>
    /// <param name="token">The bearer token to check.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    public static bool IsValidBearerToken(string token) => ValidateBearerToken(token).Count == 0;

    /// <summary>
    /// Determines whether a status code is valid.
    /// </summary>
    /// <param name="statusCode">The status code to check.</param>
    /// <returns>True if the status code is valid; otherwise, false.</returns>
    public static bool IsValidStatusCode(int statusCode) => ValidateStatusCode(statusCode).Count == 0;

    /// <summary>
    /// Determines whether a base URL is valid.
    /// </summary>
    /// <param name="baseUrl">The base URL to check.</param>
    /// <returns>True if the base URL is valid; otherwise, false.</returns>
    public static bool IsValidBaseUrl(string baseUrl) => ValidateBaseUrl(baseUrl).Count == 0;

    /// <summary>
    /// Determines whether a path is valid.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is valid; otherwise, false.</returns>
    public static bool IsValidPath(string path) => ValidatePath(path).Count == 0;

    /// <summary>
    /// Determines whether query parameters are valid.
    /// </summary>
    /// <param name="parameters">The query parameters to check.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsValidQueryParameters(Dictionary<string, string> parameters) => ValidateQueryParameters(parameters).Count == 0;

    /// <summary>
    /// Determines whether a Content-Type header is valid.
    /// </summary>
    /// <param name="contentType">The Content-Type header to check.</param>
    /// <returns>True if the Content-Type header is valid; otherwise, false.</returns>
    public static bool IsValidContentType(string contentType) => ValidateContentType(contentType).Count == 0;

    /// <summary>
    /// Determines whether a header is valid.
    /// </summary>
    /// <param name="header">The header to check.</param>
    /// <returns>True if the header is valid; otherwise, false.</returns>
    public static bool IsValidHeader(string header) => ValidateHeader(header).Count == 0;

    /// <summary>
    /// Determines whether a retry attempt is valid.
    /// </summary>
    /// <param name="attempt">The retry attempt to check.</param>
    /// <returns>True if the attempt is valid; otherwise, false.</returns>
    public static bool IsValidRetryAttempt(int attempt) => ValidateRetryAttempt(attempt).Count == 0;

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