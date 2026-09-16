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

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientFactory"/> class.
    /// </summary>
    /// <param name="factory">The framework HTTP client factory used to create named clients.</param>
    /// <param name="logger">The logger used to record client configuration activity.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/> or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public HttpClientFactory(IHttpClientFactory factory, ILogger<HttpClientFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(logger);
        _factory = factory;
        _logger = logger;
    }

    /// <summary>
    /// Creates an HttpClient with default configuration for external API calls.
    /// Includes standard timeout and User-Agent headers.
    /// </summary>
    /// <param name="name">The name of the configured client to create.</param>
    /// <returns>The configured HTTP client.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
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
    /// <param name="apiKey">The API key to add to the request headers.</param>
    /// <param name="name">The name of the configured client to create.</param>
    /// <returns>The configured HTTP client containing the API key header.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="apiKey"/> or <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="apiKey"/> or <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
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
    /// <param name="token">The bearer token to add to the authorization header.</param>
    /// <param name="name">The name of the configured client to create.</param>
    /// <returns>The configured HTTP client containing the bearer authorization header.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="token"/> or <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="token"/> or <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
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
    /// <param name="baseUrl">The absolute or relative URL to assign as the client's base address.</param>
    /// <param name="name">The name of the configured client to create.</param>
    /// <returns>The configured HTTP client with the specified base address.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="baseUrl"/> or <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="baseUrl"/> or <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="UriFormatException">Thrown when <paramref name="baseUrl"/> cannot be parsed as a URI.</exception>
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
