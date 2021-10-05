#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Provides validation helpers for <see cref="HttpClientFactory"/> instances.
/// Validates configuration parameters and state to ensure proper HttpClient creation.
/// </summary>
public static class HttpClientFactoryValidation
{
    /// <summary>
    /// Validates the specified <see cref="HttpClientFactory"/> instance.
    /// </summary>
    /// <param name="value">The HttpClientFactory instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HttpClientFactory? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // HttpClientFactory itself is just a wrapper around IHttpClientFactory
        // No additional validation needed beyond null check

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="HttpClientFactory"/> instance is valid.
    /// </summary>
    /// <param name="value">The HttpClientFactory instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this HttpClientFactory? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="HttpClientFactory"/> instance is valid.
    /// </summary>
    /// <param name="value">The HttpClientFactory instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this HttpClientFactory? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"HttpClientFactory is invalid. Problems: {string.Join(", ", problems)}.",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates the parameters for <see cref="HttpClientFactory.CreateClient(string)"/> method.
    /// </summary>
    /// <param name="name">The client name.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateCreateClient(string? name)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            problems.Add("Client name cannot be null, empty, or whitespace.");
        }
        else if (name.Length > 100)
        {
            problems.Add("Client name cannot exceed 100 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the parameters for <see cref="HttpClientFactory.CreateClient(string)"/> are valid.
    /// </summary>
    /// <param name="name">The client name.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidCreateClient(string? name)
    {
        return ValidateCreateClient(name).Count == 0;
    }

    /// <summary>
    /// Ensures that the parameters for <see cref="HttpClientFactory.CreateClient(string)"/> are valid.
    /// </summary>
    /// <param name="name">The client name.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid, containing a list of problems.</exception>
    public static void EnsureValidCreateClient(string? name)
    {
        var problems = ValidateCreateClient(name);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid parameters for CreateClient. Problems: {string.Join(", ", problems)}.",
                nameof(name));
        }
    }

    /// <summary>
    /// Validates the parameters for <see cref="HttpClientFactory.CreateAuthenticatedClient(string, string)"/> method.
    /// </summary>
    /// <param name="apiKey">The API key to use for authentication.</param>
    /// <param name="name">The client name.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateCreateAuthenticatedClient(string? apiKey, string? name)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            problems.Add("API key cannot be null, empty, or whitespace.");
        }
        else if (apiKey.Length > 500)
        {
            problems.Add("API key cannot exceed 500 characters.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            problems.Add("Client name cannot be null, empty, or whitespace.");
        }
        else if (name.Length > 100)
        {
            problems.Add("Client name cannot exceed 100 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the parameters for <see cref="HttpClientFactory.CreateAuthenticatedClient(string, string)"/> are valid.
    /// </summary>
    /// <param name="apiKey">The API key to use for authentication.</param>
    /// <param name="name">The client name.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidCreateAuthenticatedClient(string? apiKey, string? name)
    {
        return ValidateCreateAuthenticatedClient(apiKey, name).Count == 0;
    }

    /// <summary>
    /// Ensures that the parameters for <see cref="HttpClientFactory.CreateAuthenticatedClient(string, string)"/> are valid.
    /// </summary>
    /// <param name="apiKey">The API key to use for authentication.</param>
    /// <param name="name">The client name.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid, containing a list of problems.</exception>
    public static void EnsureValidCreateAuthenticatedClient(string? apiKey, string? name)
    {
        var problems = ValidateCreateAuthenticatedClient(apiKey, name);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid parameters for CreateAuthenticatedClient. Problems: {string.Join(", ", problems)}.",
                nameof(apiKey));
        }
    }

    /// <summary>
    /// Validates the parameters for <see cref="HttpClientFactory.CreateBearerClient(string, string)"/> method.
    /// </summary>
    /// <param name="token">The bearer token to use for authentication.</param>
    /// <param name="name">The client name.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateCreateBearerClient(string? token, string? name)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(token))
        {
            problems.Add("Bearer token cannot be null, empty, or whitespace.");
        }
        else if (token.Length > 2000)
        {
            problems.Add("Bearer token cannot exceed 2000 characters.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            problems.Add("Client name cannot be null, empty, or whitespace.");
        }
        else if (name.Length > 100)
        {
            problems.Add("Client name cannot exceed 100 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the parameters for <see cref="HttpClientFactory.CreateBearerClient(string, string)"/> are valid.
    /// </summary>
    /// <param name="token">The bearer token to use for authentication.</param>
    /// <param name="name">The client name.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidCreateBearerClient(string? token, string? name)
    {
        return ValidateCreateBearerClient(token, name).Count == 0;
    }

    /// <summary>
    /// Ensures that the parameters for <see cref="HttpClientFactory.CreateBearerClient(string, string)"/> are valid.
    /// </summary>
    /// <param name="token">The bearer token to use for authentication.</param>
    /// <param name="name">The client name.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid, containing a list of problems.</exception>
    public static void EnsureValidCreateBearerClient(string? token, string? name)
    {
        var problems = ValidateCreateBearerClient(token, name);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid parameters for CreateBearerClient. Problems: {string.Join(", ", problems)}.",
                nameof(token));
        }
    }

    /// <summary>
    /// Validates the parameters for <see cref="HttpClientFactory.CreateClientWithBaseUrl(string, string)"/> method.
    /// </summary>
    /// <param name="baseUrl">The base URL for the HTTP client.</param>
    /// <param name="name">The client name.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateCreateClientWithBaseUrl(string? baseUrl, string? name)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            problems.Add("Base URL cannot be null, empty, or whitespace.");
        }
        else
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uriResult))
            {
                problems.Add("Base URL must be a valid absolute URI.");
            }
            else if (!uriResult.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
                     !uriResult.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                problems.Add("Base URL must use http:// or https:// scheme.");
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            problems.Add("Client name cannot be null, empty, or whitespace.");
        }
        else if (name.Length > 100)
        {
            problems.Add("Client name cannot exceed 100 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the parameters for <see cref="HttpClientFactory.CreateClientWithBaseUrl(string, string)"/> are valid.
    /// </summary>
    /// <param name="baseUrl">The base URL for the HTTP client.</param>
    /// <param name="name">The client name.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidCreateClientWithBaseUrl(string? baseUrl, string? name)
    {
        return ValidateCreateClientWithBaseUrl(baseUrl, name).Count == 0;
    }

    /// <summary>
    /// Ensures that the parameters for <see cref="HttpClientFactory.CreateClientWithBaseUrl(string, string)"/> are valid.
    /// </summary>
    /// <param name="baseUrl">The base URL for the HTTP client.</param>
    /// <param name="name">The client name.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid, containing a list of problems.</exception>
    public static void EnsureValidCreateClientWithBaseUrl(string? baseUrl, string? name)
    {
        var problems = ValidateCreateClientWithBaseUrl(baseUrl, name);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid parameters for CreateClientWithBaseUrl. Problems: {string.Join(", ", problems)}.",
                nameof(baseUrl));
        }
    }
}