#nullable enable

using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http.Headers;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Extension methods for HttpClientFactoryTests to provide common testing utilities.
/// </summary>
public static class HttpClientFactoryTestsExtensions
{
    /// <summary>
    /// Creates a configured HttpClientFactory instance for testing with default mock setup.
    /// </summary>
    /// <returns>A tuple containing the HttpClientFactory and its dependencies for test customization.</returns>
    /// <exception cref="InvalidOperationException">If service provider creation fails.</exception>
    public static (HttpClientFactory Factory, Mock<IHttpClientFactory> HttpClientFactoryMock, Mock<ILogger<HttpClientFactory>> LoggerMock, ServiceProvider ServiceProvider) CreateTestHttpClientFactory(this HttpClientFactoryTests _)
    {
        var services = new ServiceCollection();

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var loggerMock = new Mock<ILogger<HttpClientFactory>>();

        // Setup mock to return a new HttpClient instance for each call
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns<string>(name => {
                var mockHandler = new MockHttpMessageHandler();
                var httpClient = new HttpClient(mockHandler);
                return httpClient;
            });

        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = new HttpClientFactory(httpClientFactoryMock.Object, loggerMock.Object);

        return (httpClientFactory, httpClientFactoryMock, loggerMock, serviceProvider);
    }

    /// <summary>
    /// Asserts that the HttpClient has the expected default configuration (User-Agent header and timeout).
    /// </summary>
    /// <param name="client">The HttpClient to assert.</param>
    /// <exception cref="ArgumentNullException">If client is null.</exception>
    public static void AssertHasDefaultConfiguration(this HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        client.Should().NotBeNull();
        client.Timeout.Should().Be(HttpClientFactoryTestsConstants.DefaultTimeout);
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.UserAgentHeaderName);
        client.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.UserAgentValue);
    }

    /// <summary>
    /// Asserts that the HttpClient has the specified API key header with the expected value.
    /// </summary>
    /// <param name="client">The HttpClient to assert.</param>
    /// <param name="expectedApiKey">The expected API key value.</param>
    /// <exception cref="ArgumentNullException">If client is null.</exception>
    /// <exception cref="ArgumentException">If expectedApiKey is null, empty, or whitespace.</exception>
    public static void AssertHasApiKeyHeader(this HttpClient client, string expectedApiKey)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(expectedApiKey);

        client.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.ApiKeyHeaderName);
        client.DefaultRequestHeaders.GetValues(HttpClientFactoryTestsConstants.ApiKeyHeaderName).First().Should().Be(expectedApiKey);
    }

    /// <summary>
    /// Asserts that the HttpClient has the specified Bearer token in the Authorization header.
    /// </summary>
    /// <param name="client">The HttpClient to assert.</param>
    /// <param name="expectedToken">The expected Bearer token value.</param>
    /// <exception cref="ArgumentNullException">If client is null.</exception>
    /// <exception cref="ArgumentException">If expectedToken is null, empty, or whitespace.</exception>
    public static void AssertHasBearerToken(this HttpClient client, string expectedToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(expectedToken);

        client.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.AuthorizationHeaderName);
        client.DefaultRequestHeaders.Authorization?.Scheme.Should().Be(HttpClientFactoryTestsConstants.BearerScheme);
        client.DefaultRequestHeaders.Authorization?.Parameter.Should().Be(expectedToken);
    }

    /// <summary>
    /// Creates two HttpClient instances and asserts they are independent (different instances).
    /// </summary>
    /// <param factory="factory">The HttpClientFactory to use for creating clients.</param>
    /// <param name="firstClientName">Name for the first client.</param>
    /// <param name="secondClientName">Name for the second client.</param>
    /// <returns>A tuple containing both created HttpClient instances.</returns>
    /// <exception cref="ArgumentNullException">If factory is null.</exception>
    /// <exception cref="ArgumentException">If either client name is null, empty, or whitespace.</exception>
    public static (HttpClient FirstClient, HttpClient SecondClient) CreateAndAssertIndependentClients(this HttpClientFactory factory, string firstClientName, string secondClientName)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentException.ThrowIfNullOrEmpty(firstClientName);
        ArgumentException.ThrowIfNullOrEmpty(secondClientName);

        var client1 = factory.CreateClient(firstClientName);
        var client2 = factory.CreateClient(secondClientName);

        client1.Should().NotBeSameAs(client2);
        client1.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.UserAgentValue);
        client2.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.UserAgentValue);

        return (client1, client2);
    }
}

/// <summary>
/// Mock HttpMessageHandler to avoid actual HTTP calls in tests.
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    }
}