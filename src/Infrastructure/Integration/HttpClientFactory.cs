#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Utilities;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Factory for creating configured HttpClient instances. Centralizes HTTP client
/// configuration including timeouts, retry policies, and default headers.
/// Uses the default HttpClient from DI when possible to benefit from handler pooling.
/// </summary>
public class HttpClientFactory : ICustomHttpClientFactory
{
    private readonly System.Net.Http.IHttpClientFactory _factory;
    private readonly ILogger<HttpClientFactory> _logger;

    public HttpClientFactory(IHttpClientFactory factory, ILogger<HttpClientFactory> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    /// <summary>
    /// Creates an HttpClient with default configuration for external API calls.
    /// Includes standard timeout and User-Agent headers.
    /// </summary>
    public HttpClient CreateClient(string name = "default")
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _logger.LogInformation("Entering CreateClient with {Name}", name);

        var client = _factory.CreateClient(name);

        // Set standard timeouts
        client.Timeout = TimeSpan.FromSeconds(HttpClientFactoryConstants.DefaultTimeoutSeconds);
        _logger.LogInformation("Setting timeout to {Timeout} seconds for client {Name}", HttpClientFactoryConstants.DefaultTimeoutSeconds, name);

        // Add default headers if not already present
        if (!client.DefaultRequestHeaders.Contains(HttpClientFactoryConstants.DefaultUserAgentHeaderName))
        {
            client.DefaultRequestHeaders.Add(HttpClientFactoryConstants.DefaultUserAgentHeaderName, HttpClientFactoryConstants.DefaultUserAgentHeaderValue);
            _logger.LogInformation("Adding default User-Agent header for client {Name}", name);
        }

        _logger.LogInformation("Exiting CreateClient.");
        return client;
    }

    /// <summary>
    /// Creates an HttpClient configured for API calls with authentication.
    /// Includes the provided API key in the X-Api-Key header.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string apiKey, string name = "authenticated")
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        ArgumentException.ThrowIfNullOrEmpty(name);

        _logger.LogInformation("Entering CreateAuthenticatedClient with {Name}", name);

        var client = CreateClient(name);
        _logger.LogInformation("Adding API key header for client {Name}", name);
        client.DefaultRequestHeaders.Add(HttpClientFactoryConstants.ApiKeyHeaderName, apiKey);

        _logger.LogInformation("Exiting CreateAuthenticatedClient.");
        return client;
    }

    /// <summary>
    /// Creates an HttpClient configured for OAuth/Bearer token authentication.
    /// </summary>
    public HttpClient CreateBearerClient(string token, string name = "bearer")
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(name);

        _logger.LogInformation("Entering CreateBearerClient with {Name}", name);

        var client = CreateClient(name);
        var bearerToken = HttpUtility.CreateBearerAuthHeader(token);
        _logger.LogInformation("Adding Bearer token header for client {Name}", name);
        client.DefaultRequestHeaders.Add(HttpClientFactoryConstants.AuthorizationHeaderName, bearerToken);

        _logger.LogInformation("Exiting CreateBearerClient.");
        return client;
    }

    /// <summary>
    /// Creates an HttpClient configured with a custom base URL.
    /// </summary>
    public HttpClient CreateClientWithBaseUrl(string baseUrl, string name = "default")
    {
        ArgumentException.ThrowIfNullOrEmpty(baseUrl);
        ArgumentException.ThrowIfNullOrEmpty(name);

        _logger.LogInformation("Entering CreateClientWithBaseUrl with {BaseUrl} and {Name}", baseUrl, name);

        var client = CreateClient(name);
        client.BaseAddress = new Uri(baseUrl);

        _logger.LogInformation("Exiting CreateClientWithBaseUrl.");
        return client;
    }
}

/// <summary>
/// Interface for custom HTTP client factory with additional helpers.
/// </summary>
public interface ICustomHttpClientFactory
{
    HttpClient CreateClient(string name = "default");
    HttpClient CreateAuthenticatedClient(string apiKey, string name = "authenticated");
    HttpClient CreateBearerClient(string token, string name = "bearer");
    HttpClient CreateClientWithBaseUrl(string baseUrl, string name = "default");
}