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
        var client = _factory.CreateClient(name);

        // Set standard timeouts
        client.Timeout = TimeSpan.FromSeconds(30);

        // Add default headers if not already present
        if (!client.DefaultRequestHeaders.Contains("User-Agent"))
        {
            client.DefaultRequestHeaders.Add("User-Agent", "DotnetServiceScaffold/1.0");
        }

        return client;
    }

    /// <summary>
    /// Creates an HttpClient configured for API calls with authentication.
    /// Includes the provided API key in the X-Api-Key header.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string apiKey, string name = "authenticated")
    {
        var client = CreateClient(name);
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        return client;
    }

    /// <summary>
    /// Creates an HttpClient configured for OAuth/Bearer token authentication.
    /// </summary>
    public HttpClient CreateBearerClient(string token, string name = "bearer")
    {
        var client = CreateClient(name);
        var bearerToken = HttpUtility.CreateBearerAuthHeader(token);
        client.DefaultRequestHeaders.Add("Authorization", bearerToken);
        return client;
    }

    /// <summary>
    /// Creates an HttpClient configured with a custom base URL.
    /// </summary>
    public HttpClient CreateClientWithBaseUrl(string baseUrl, string name = "default")
    {
        var client = CreateClient(name);
        client.BaseAddress = new Uri(baseUrl);
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
