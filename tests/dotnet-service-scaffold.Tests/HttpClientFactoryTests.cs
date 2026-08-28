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
        client.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == "User-Agent");
        client.DefaultRequestHeaders.UserAgent.ToString().Should().Be("DotnetServiceScaffold/1.0");
    }

    [Fact]
    public void CreateClient_WithCustomName_ReturnsConfiguredHttpClient()
    {
        // Act
        var client = _httpClientFactory.CreateClient("custom-client");

        // Assert
        client.Should().NotBeNull();
        _httpClientFactoryMock.Verify(f => f.CreateClient("custom-client"), Times.Once);
    }

    [Fact]
    public void CreateClient_WhenUserAgentHeaderAlreadyExists_DoesNotOverride()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(mockHandler);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Existing-Agent/1.0");

        _httpClientFactoryMock.Setup(f => f.CreateClient("existing"))
            .Returns(httpClient);

        // Act
        var client = _httpClientFactory.CreateClient("existing");

        // Assert
        client.DefaultRequestHeaders.UserAgent.ToString().Should().Be("Existing-Agent/1.0");
    }

    [Fact]
    public void CreateAuthenticatedClient_WithValidApiKey_AddsApiKeyHeader()
    {
        // Arrange
        var apiKey = "test-api-key-12345";

        // Act
        var client = _httpClientFactory.CreateAuthenticatedClient(apiKey);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == "X-Api-Key");
        client.DefaultRequestHeaders.GetValues("X-Api-Key").First().Should().Be(apiKey);
    }

    [Fact]
    public void CreateAuthenticatedClient_WithCustomName_UsesCustomName()
    {
        // Arrange
        var apiKey = "test-api-key";

        // Act
        var client = _httpClientFactory.CreateAuthenticatedClient(apiKey, "custom-auth");

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient("custom-auth"), Times.Once);
    }

    [Fact]
    public void CreateBearerClient_WithValidToken_AddsAuthorizationHeader()
    {
        // Arrange
        var token = "test-token-abc123";

        // Act
        var client = _httpClientFactory.CreateBearerClient(token);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == "Authorization");
        client.DefaultRequestHeaders.Authorization?.Scheme.Should().Be("Bearer");
        client.DefaultRequestHeaders.Authorization?.Parameter.Should().Be(token);
    }

    [Fact]
    public void CreateBearerClient_WithCustomName_UsesCustomName()
    {
        // Arrange
        var token = "test-token";

        // Act
        var client = _httpClientFactory.CreateBearerClient(token, "custom-bearer");

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient("custom-bearer"), Times.Once);
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithValidUrl_SetsBaseAddress()
    {
        // Arrange
        var baseUrl = "https://api.example.com";

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
        var baseUrl = "https://api.example.com";

        // Act
        var client = _httpClientFactory.CreateClientWithBaseUrl(baseUrl, "custom-base");

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient("custom-base"), Times.Once);
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithTrailingSlash_HandlesCorrectly()
    {
        // Arrange
        var baseUrl = "https://api.example.com/";

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
        var emptyToken = "";

        // Act
        Action act = () => _httpClientFactory.CreateBearerClient(emptyToken);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateBearerClient_WithWhitespaceToken_ThrowsArgumentException()
    {
        // Arrange
        var whitespaceToken = "   ";

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
        var emptyBaseUrl = "";

        // Act
        Action act = () => _httpClientFactory.CreateClientWithBaseUrl(emptyBaseUrl);

        // Assert
        act.Should().Throw<UriFormatException>();
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithInvalidUrl_ThrowsUriFormatException()
    {
        // Arrange
        var invalidUrl = "not-a-valid-url";

        // Act
        Action act = () => _httpClientFactory.CreateClientWithBaseUrl(invalidUrl);

        // Assert
        act.Should().Throw<UriFormatException>();
    }

    [Fact]
    public void CreateClient_WithMultipleCalls_ReturnsIndependentClients()
    {
        // Act
        var client1 = _httpClientFactory.CreateClient("client1");
        var client2 = _httpClientFactory.CreateClient("client2");

        // Assert
        client1.Should().NotBeSameAs(client2);
        client1.DefaultRequestHeaders.UserAgent.ToString().Should().Be("DotnetServiceScaffold/1.0");
        client2.DefaultRequestHeaders.UserAgent.ToString().Should().Be("DotnetServiceScaffold/1.0");
    }

    [Fact]
    public void CreateAuthenticatedClient_AfterCreateClient_HasBothHeaders()
    {
        // Arrange
        var apiKey = "test-key";

        // Act
        var baseClient = _httpClientFactory.CreateClient();
        var authClient = _httpClientFactory.CreateAuthenticatedClient(apiKey);

        // Assert
        baseClient.DefaultRequestHeaders.Should().Contain(h => h.Key == "User-Agent");
        baseClient.DefaultRequestHeaders.Should().NotContain(h => h.Key == "X-Api-Key");

        authClient.DefaultRequestHeaders.Should().Contain(h => h.Key == "User-Agent");
        authClient.DefaultRequestHeaders.Should().Contain(h => h.Key == "X-Api-Key");
    }

    [Fact]
    public void CreateBearerClient_AfterCreateClient_HasBothHeaders()
    {
        // Arrange
        var token = "test-token";

        // Act
        var baseClient = _httpClientFactory.CreateClient();
        var bearerClient = _httpClientFactory.CreateBearerClient(token);

        // Assert
        baseClient.DefaultRequestHeaders.Should().Contain(h => h.Key == "User-Agent");
        baseClient.DefaultRequestHeaders.Should().NotContain(h => h.Key == "Authorization");

        bearerClient.DefaultRequestHeaders.Should().Contain(h => h.Key == "User-Agent");
        bearerClient.DefaultRequestHeaders.Should().Contain(h => h.Key == "Authorization");
    }

    [Fact]
    public void CreateClientWithBaseUrl_AfterCreateClient_HasBaseAddressAndUserAgent()
    {
        // Arrange
        var baseUrl = "https://api.example.com";

        // Act
        var baseClient = _httpClientFactory.CreateClient();
        var urlClient = _httpClientFactory.CreateClientWithBaseUrl(baseUrl);

        // Assert
        baseClient.BaseAddress.Should().BeNull();
        baseClient.DefaultRequestHeaders.Should().Contain(h => h.Key == "User-Agent");

        urlClient.BaseAddress.Should().Be(new Uri(baseUrl));
        urlClient.DefaultRequestHeaders.Should().Contain(h => h.Key == "User-Agent");
    }

    [Fact]
    public void CreateClient_TimeoutIsSetTo30Seconds()
    {
        // Act
        var client = _httpClientFactory.CreateClient();

        // Assert
        client.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void CreateClient_WithDefaultName_CreatesClientWithDefaultName()
    {
        // Act
        var client = _httpClientFactory.CreateClient();

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient("default"), Times.Once);
    }

    [Fact]
    public void CreateAuthenticatedClient_UsesDefaultAuthenticatedName()
    {
        // Arrange
        var apiKey = "test-key";

        // Act
        var client = _httpClientFactory.CreateAuthenticatedClient(apiKey);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient("authenticated"), Times.Once);
    }

    [Fact]
    public void CreateBearerClient_UsesDefaultBearerName()
    {
        // Arrange
        var token = "test-token";

        // Act
        var client = _httpClientFactory.CreateBearerClient(token);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient("bearer"), Times.Once);
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
