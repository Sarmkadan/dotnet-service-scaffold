using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Provides extension methods for <see cref="HttpClientFactory"/>.
/// </summary>
public static class HttpClientFactoryExtensions
{
    /// <summary>
    /// Creates an <see cref="HttpClient"/> with the specified timeout.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="timeout">The timeout to set.</param>
    /// <returns>An <see cref="HttpClient"/> with the specified timeout.</returns>
    /// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
    public static HttpClient CreateClientWithTimeout(this HttpClientFactory factory, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var client = factory.CreateClient();
        client.Timeout = timeout;
        return client;
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> configured for JSON requests.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <returns>An <see cref="HttpClient"/> configured with application/json accept header.</returns>
    /// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
    public static HttpClient CreateJsonClient(this HttpClientFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    /// <summary>
    /// Creates an authenticated <see cref="HttpClient"/> with the specified timeout.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    /// <param name="username">The username for basic authentication.</param>
    /// <param name="password">The password for basic authentication.</param>
    /// <param name="timeout">The timeout to set.</param>
    /// <returns>An authenticated <see cref="HttpClient"/> with the specified timeout.</returns>
    /// <exception cref="ArgumentNullException">Thrown when factory or username or password is null.</exception>
    public static HttpClient CreateAuthenticatedClientWithTimeout(this HttpClientFactory factory, string username, string password, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var client = factory.CreateAuthenticatedClient(username, password);
        client.Timeout = timeout;
        return client;
    }
}
