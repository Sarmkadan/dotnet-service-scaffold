#nullable enable

using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Unit tests for HttpClientFactory class.
/// Tests the actual behavior of HttpClient creation methods including headers, timeouts, and configurations.
/// </summary>
public class HttpClientFactoryTests : IDisposable, IHttpClientFactoryTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<HttpClientFactory>> _loggerMock;
    private readonly HttpClientFactory _httpClientFactory;

    public HttpClientFactoryTests()
    {
        var services = new ServiceCollection();

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<HttpClientFactory>>();

        // Setup mock to return a new HttpClient instance for each call
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns<string>(name => {
                var mockHandler = new MockHttpMessageHandler();
                var httpClient = new HttpClient(mockHandler);
                return httpClient;
            });

        _serviceProvider = services.BuildServiceProvider();

        _httpClientFactory = new HttpClientFactory(_httpClientFactoryMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public void CreateClient_WithDefaultName_ReturnsConfiguredHttpClient()
    {
        // Act
        var client = _httpClientFactory.CreateClient();

        // Assert
        client.Should().NotBeNull();
        client.Timeout.Should().Be(HttpClientFactoryTestsConstants.DefaultTimeout);
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.UserAgentHeaderName);
        client.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.UserAgentValue);
    }

    [Fact]
    public void CreateClient_WithCustomName_ReturnsConfiguredHttpClient()
    {
        // Act
        var client = _httpClientFactory.CreateClient(HttpClientFactoryTestsConstants.CustomClientName);

        // Assert
        client.Should().NotBeNull();
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.CustomClientName), Times.Once);
    }

    [Fact]
    public void CreateClient_WhenUserAgentHeaderAlreadyExists_DoesNotOverride()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(mockHandler);
        httpClient.DefaultRequestHeaders.Add(HttpClientFactoryTestsConstants.UserAgentHeaderName, HttpClientFactoryTestsConstants.ExistingAgentValue);

        _httpClientFactoryMock.Setup(f => f.CreateClient(HttpClientFactoryTestsConstants.ExistingClientName))
            .Returns(httpClient);

        // Act
        var client = _httpClientFactory.CreateClient(HttpClientFactoryTestsConstants.ExistingClientName);

        // Assert
        client.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.ExistingAgentValue);
    }

    [Fact]
    public void CreateAuthenticatedClient_WithValidApiKey_AddsApiKeyHeader()
    {
        // Arrange
        var apiKey = HttpClientFactoryTestsConstants.TestApiKey;

        // Act
        var client = _httpClientFactory.CreateAuthenticatedClient(apiKey);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.ApiKeyHeaderName);
        client.DefaultRequestHeaders.GetValues(HttpClientFactoryTestsConstants.ApiKeyHeaderName).First().Should().Be(apiKey);
    }

    [Fact]
    public void CreateAuthenticatedClient_WithCustomName_UsesCustomName()
    {
        // Arrange
        var apiKey = HttpClientFactoryTestsConstants.TestApiKeyBase;

        // Act
        var client = _httpClientFactory.CreateAuthenticatedClient(apiKey, HttpClientFactoryTestsConstants.CustomAuthClientName);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.CustomAuthClientName), Times.Once);
    }

    [Fact]
    public void CreateBearerClient_WithValidToken_AddsAuthorizationHeader()
    {
        // Arrange
        var token = HttpClientFactoryTestsConstants.TestBearerToken;

        // Act
        var client = _httpClientFactory.CreateBearerClient(token);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.AuthorizationHeaderName);
        client.DefaultRequestHeaders.Authorization?.Scheme.Should().Be(HttpClientFactoryTestsConstants.BearerScheme);
        client.DefaultRequestHeaders.Authorization?.Parameter.Should().Be(token);
    }

    [Fact]
    public void CreateBearerClient_WithCustomName_UsesCustomName()
    {
        // Arrange
        var token = HttpClientFactoryTestsConstants.TestToken;

        // Act
        var client = _httpClientFactory.CreateBearerClient(token, HttpClientFactoryTestsConstants.CustomBearerClientName);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.CustomBearerClientName), Times.Once);
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithValidUrl_SetsBaseAddress()
    {
        // Arrange
        var baseUrl = HttpClientFactoryTestsConstants.ExampleBaseUrl;

        // Act
        var client = _httpClientFactory.CreateClientWithBaseUrl(baseUrl);

        // Assert
        client.Should().NotBeNull();
        client.BaseAddress.Should().Be(new Uri(baseUrl));
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithCustomName_UsesCustomName()
    {
        // Arrange
        var baseUrl = HttpClientFactoryTestsConstants.ExampleBaseUrl;

        // Act
        var client = _httpClientFactory.CreateClientWithBaseUrl(baseUrl, HttpClientFactoryTestsConstants.CustomBaseClientName);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.CustomBaseClientName), Times.Once);
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithTrailingSlash_HandlesCorrectly()
    {
        // Arrange
        var baseUrl = HttpClientFactoryTestsConstants.ExampleBaseUrlWithTrailingSlash;

        // Act
        var client = _httpClientFactory.CreateClientWithBaseUrl(baseUrl);

        // Assert
        client.BaseAddress.Should().Be(new Uri(baseUrl));
    }

    [Fact]
    public void CreateBearerClient_WithNullToken_ThrowsArgumentException()
    {
        // Arrange
        string? nullToken = null;

        // Act
        Action act = () => _httpClientFactory.CreateBearerClient(nullToken!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateBearerClient_WithEmptyToken_ThrowsArgumentException()
    {
        // Arrange
        var emptyToken = HttpClientFactoryTestsConstants.EmptyString;

        // Act
        Action act = () => _httpClientFactory.CreateBearerClient(emptyToken);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateBearerClient_WithWhitespaceToken_ThrowsArgumentException()
    {
        // Arrange
        var whitespaceToken = HttpClientFactoryTestsConstants.WhitespaceString;

        // Act
        Action act = () => _httpClientFactory.CreateBearerClient(whitespaceToken);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithNullBaseUrl_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullBaseUrl = null;

        // Act
        Action act = () => _httpClientFactory.CreateClientWithBaseUrl(nullBaseUrl!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithEmptyBaseUrl_ThrowsUriFormatException()
    {
        // Arrange
        var emptyBaseUrl = HttpClientFactoryTestsConstants.EmptyString;

        // Act
        Action act = () => _httpClientFactory.CreateClientWithBaseUrl(emptyBaseUrl);

        // Assert
        act.Should().Throw<UriFormatException>();
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithInvalidUrl_ThrowsUriFormatException()
    {
        // Arrange
        var invalidUrl = HttpClientFactoryTestsConstants.InvalidUrlString;

        // Act
        Action act = () => _httpClientFactory.CreateClientWithBaseUrl(invalidUrl);

        // Assert
        act.Should().Throw<UriFormatException>();
    }

    [Fact]
    public void CreateClient_WithMultipleCalls_ReturnsIndependentClients()
    {
        // Act
        var client1 = _httpClientFactory.CreateClient(HttpClientFactoryTestsConstants.TestClientName1);
        var client2 = _httpClientFactory.CreateClient(HttpClientFactoryTestsConstants.TestClientName2);

        // Assert
        client1.Should().NotBeSameAs(client2);
        client1.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.UserAgentValue);
        client2.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.UserAgentValue);
    }

    [Fact]
    public void CreateAuthenticatedClient_AfterCreateClient_HasBothHeaders()
    {
        // Arrange
        var apiKey = HttpClientFactoryTestsConstants.TestKey;

        // Act
        var baseClient = _httpClientFactory.CreateClient();
        var authClient = _httpClientFactory.CreateAuthenticatedClient(apiKey);

        // Assert
        baseClient.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.UserAgentHeaderName);
        baseClient.DefaultRequestHeaders.Should().NotContain(h => h.Key == HttpClientFactoryTestsConstants.ApiKeyHeaderName);

        authClient.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.UserAgentHeaderName);
        authClient.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.ApiKeyHeaderName);
    }

    [Fact]
    public void CreateBearerClient_AfterCreateClient_HasBothHeaders()
    {
        // Arrange
        var token = HttpClientFactoryTestsConstants.TestToken;

        // Act
        var baseClient = _httpClientFactory.CreateClient();
        var bearerClient = _httpClientFactory.CreateBearerClient(token);

        // Assert
        baseClient.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.UserAgentHeaderName);
        baseClient.DefaultRequestHeaders.Should().NotContain(h => h.Key == HttpClientFactoryTestsConstants.AuthorizationHeaderName);

        bearerClient.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.UserAgentHeaderName);
        bearerClient.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.AuthorizationHeaderName);
    }

    [Fact]
    public void CreateClientWithBaseUrl_AfterCreateClient_HasBaseAddressAndUserAgent()
    {
        // Arrange
        var baseUrl = HttpClientFactoryTestsConstants.ExampleBaseUrl;

        // Act
        var baseClient = _httpClientFactory.CreateClient();
        var urlClient = _httpClientFactory.CreateClientWithBaseUrl(baseUrl);

        // Assert
        baseClient.BaseAddress.Should().BeNull();
        baseClient.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.UserAgentHeaderName);

        urlClient.BaseAddress.Should().Be(new Uri(baseUrl));
        urlClient.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.UserAgentHeaderName);
    }

    [Fact]
    public void CreateClient_TimeoutIsSetTo30Seconds()
    {
        // Act
        var client = _httpClientFactory.CreateClient();

        // Assert
        client.Timeout.Should().Be(HttpClientFactoryTestsConstants.DefaultTimeout);
    }

    [Fact]
    public void CreateClient_WithDefaultName_CreatesClientWithDefaultName()
    {
        // Act
        var client = _httpClientFactory.CreateClient();

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.DefaultClientName), Times.Once);
    }

    [Fact]
    public void CreateAuthenticatedClient_UsesDefaultAuthenticatedName()
    {
        // Arrange
        var apiKey = HttpClientFactoryTestsConstants.TestKey;

        // Act
        var client = _httpClientFactory.CreateAuthenticatedClient(apiKey);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.AuthenticatedClientName), Times.Once);
    }

    [Fact]
    public void CreateBearerClient_UsesDefaultBearerName()
    {
        // Arrange
        var token = HttpClientFactoryTestsConstants.TestToken;

        // Act
        var client = _httpClientFactory.CreateBearerClient(token);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.BearerClientName), Times.Once);
    }

    // Mock HttpMessageHandler to avoid actual HTTP calls
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
