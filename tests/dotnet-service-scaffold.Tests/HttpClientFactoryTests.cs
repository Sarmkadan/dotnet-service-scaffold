#nullable enable

using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        _loggerMock.Object.LogInformation("Disposing test resources for {TestClass}", nameof(HttpClientFactoryTests));
        _serviceProvider?.Dispose();
        _loggerMock.Object.LogInformation("Disposed test resources for {TestClass}", nameof(HttpClientFactoryTests));
    }

    [Fact]
    public void CreateClient_WithDefaultName_ReturnsConfiguredHttpClient()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateClient_WithDefaultName_ReturnsConfiguredHttpClient), HttpClientFactoryTestsConstants.DefaultClientName);
        // Act
        var client = _httpClientFactory.CreateClient();

        // Assert
        client.Should().NotBeNull();
        client.Timeout.Should().Be(HttpClientFactoryTestsConstants.DefaultTimeout);
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.UserAgentHeaderName);
        client.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.UserAgentValue);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateClient_WithDefaultName_ReturnsConfiguredHttpClient), HttpClientFactoryTestsConstants.DefaultClientName);
    }

    [Fact]
    public void CreateClient_WithCustomName_ReturnsConfiguredHttpClient()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateClient_WithCustomName_ReturnsConfiguredHttpClient), HttpClientFactoryTestsConstants.CustomClientName);
        // Act
        var client = _httpClientFactory.CreateClient(HttpClientFactoryTestsConstants.CustomClientName);

        // Assert
        client.Should().NotBeNull();
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.CustomClientName), Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateClient_WithCustomName_ReturnsConfiguredHttpClient), HttpClientFactoryTestsConstants.CustomClientName);
    }

    [Fact]
    public void CreateClient_WhenUserAgentHeaderAlreadyExists_DoesNotOverride()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateClient_WhenUserAgentHeaderAlreadyExists_DoesNotOverride), HttpClientFactoryTestsConstants.ExistingClientName);
        _loggerMock.Object.LogWarning("Client {ClientName} already has a user-agent header; verifying the existing value is preserved", HttpClientFactoryTestsConstants.ExistingClientName);
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
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateClient_WhenUserAgentHeaderAlreadyExists_DoesNotOverride), HttpClientFactoryTestsConstants.ExistingClientName);
    }

    [Fact]
    public void CreateAuthenticatedClient_WithValidApiKey_AddsApiKeyHeader()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateAuthenticatedClient_WithValidApiKey_AddsApiKeyHeader), HttpClientFactoryTestsConstants.AuthenticatedClientName);
        // Arrange
        var apiKey = HttpClientFactoryTestsConstants.TestApiKey;

        // Act
        var client = _httpClientFactory.CreateAuthenticatedClient(apiKey);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.ApiKeyHeaderName);
        client.DefaultRequestHeaders.GetValues(HttpClientFactoryTestsConstants.ApiKeyHeaderName).First().Should().Be(apiKey);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateAuthenticatedClient_WithValidApiKey_AddsApiKeyHeader), HttpClientFactoryTestsConstants.AuthenticatedClientName);
    }

    [Fact]
    public void CreateAuthenticatedClient_WithCustomName_UsesCustomName()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateAuthenticatedClient_WithCustomName_UsesCustomName), HttpClientFactoryTestsConstants.CustomAuthClientName);
        // Arrange
        var apiKey = HttpClientFactoryTestsConstants.TestApiKeyBase;

        // Act
        var client = _httpClientFactory.CreateAuthenticatedClient(apiKey, HttpClientFactoryTestsConstants.CustomAuthClientName);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.CustomAuthClientName), Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateAuthenticatedClient_WithCustomName_UsesCustomName), HttpClientFactoryTestsConstants.CustomAuthClientName);
    }

    [Fact]
    public void CreateBearerClient_WithValidToken_AddsAuthorizationHeader()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateBearerClient_WithValidToken_AddsAuthorizationHeader), HttpClientFactoryTestsConstants.BearerClientName);
        // Arrange
        var token = HttpClientFactoryTestsConstants.TestBearerToken;

        // Act
        var client = _httpClientFactory.CreateBearerClient(token);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Should().Contain(h => h.Key == HttpClientFactoryTestsConstants.AuthorizationHeaderName);
        client.DefaultRequestHeaders.Authorization?.Scheme.Should().Be(HttpClientFactoryTestsConstants.BearerScheme);
        client.DefaultRequestHeaders.Authorization?.Parameter.Should().Be(token);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateBearerClient_WithValidToken_AddsAuthorizationHeader), HttpClientFactoryTestsConstants.BearerClientName);
    }

    [Fact]
    public void CreateBearerClient_WithCustomName_UsesCustomName()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateBearerClient_WithCustomName_UsesCustomName), HttpClientFactoryTestsConstants.CustomBearerClientName);
        // Arrange
        var token = HttpClientFactoryTestsConstants.TestToken;

        // Act
        var client = _httpClientFactory.CreateBearerClient(token, HttpClientFactoryTestsConstants.CustomBearerClientName);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.CustomBearerClientName), Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateBearerClient_WithCustomName_UsesCustomName), HttpClientFactoryTestsConstants.CustomBearerClientName);
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithValidUrl_SetsBaseAddress()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with base URL {BaseUrl}", nameof(CreateClientWithBaseUrl_WithValidUrl_SetsBaseAddress), HttpClientFactoryTestsConstants.ExampleBaseUrl);
        // Arrange
        var baseUrl = HttpClientFactoryTestsConstants.ExampleBaseUrl;

        // Act
        var client = _httpClientFactory.CreateClientWithBaseUrl(baseUrl);

        // Assert
        client.Should().NotBeNull();
        client.BaseAddress.Should().Be(new Uri(baseUrl));
        _loggerMock.Object.LogInformation("Completed {TestMethod} with base URL {BaseUrl}", nameof(CreateClientWithBaseUrl_WithValidUrl_SetsBaseAddress), baseUrl);
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithCustomName_UsesCustomName()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateClientWithBaseUrl_WithCustomName_UsesCustomName), HttpClientFactoryTestsConstants.CustomBaseClientName);
        // Arrange
        var baseUrl = HttpClientFactoryTestsConstants.ExampleBaseUrl;

        // Act
        var client = _httpClientFactory.CreateClientWithBaseUrl(baseUrl, HttpClientFactoryTestsConstants.CustomBaseClientName);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.CustomBaseClientName), Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName} and base URL {BaseUrl}", nameof(CreateClientWithBaseUrl_WithCustomName_UsesCustomName), HttpClientFactoryTestsConstants.CustomBaseClientName, baseUrl);
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithTrailingSlash_HandlesCorrectly()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with base URL {BaseUrl}", nameof(CreateClientWithBaseUrl_WithTrailingSlash_HandlesCorrectly), HttpClientFactoryTestsConstants.ExampleBaseUrlWithTrailingSlash);
        // Arrange
        var baseUrl = HttpClientFactoryTestsConstants.ExampleBaseUrlWithTrailingSlash;

        // Act
        var client = _httpClientFactory.CreateClientWithBaseUrl(baseUrl);

        // Assert
        client.BaseAddress.Should().Be(new Uri(baseUrl));
        _loggerMock.Object.LogInformation("Completed {TestMethod} with base URL {BaseUrl}", nameof(CreateClientWithBaseUrl_WithTrailingSlash_HandlesCorrectly), baseUrl);
    }

    [Fact]
    public void CreateBearerClient_WithNullToken_ThrowsArgumentException()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod}", nameof(CreateBearerClient_WithNullToken_ThrowsArgumentException));
        _loggerMock.Object.LogWarning("Attempting bearer client creation with a null token in {TestMethod}", nameof(CreateBearerClient_WithNullToken_ThrowsArgumentException));
        // Arrange
        string? nullToken = null;

        // Act
        Action act = () => _httpClientFactory.CreateBearerClient(nullToken!);

        // Assert
        act.Should().Throw<ArgumentException>();
        _loggerMock.Object.LogInformation("Completed {TestMethod}", nameof(CreateBearerClient_WithNullToken_ThrowsArgumentException));
    }

    [Fact]
    public void CreateBearerClient_WithEmptyToken_ThrowsArgumentException()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod}", nameof(CreateBearerClient_WithEmptyToken_ThrowsArgumentException));
        _loggerMock.Object.LogWarning("Attempting bearer client creation with an empty token in {TestMethod}", nameof(CreateBearerClient_WithEmptyToken_ThrowsArgumentException));
        // Arrange
        var emptyToken = HttpClientFactoryTestsConstants.EmptyString;

        // Act
        Action act = () => _httpClientFactory.CreateBearerClient(emptyToken);

        // Assert
        act.Should().Throw<ArgumentException>();
        _loggerMock.Object.LogInformation("Completed {TestMethod}", nameof(CreateBearerClient_WithEmptyToken_ThrowsArgumentException));
    }

    [Fact]
    public void CreateBearerClient_WithWhitespaceToken_ThrowsArgumentException()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod}", nameof(CreateBearerClient_WithWhitespaceToken_ThrowsArgumentException));
        _loggerMock.Object.LogWarning("Attempting bearer client creation with a whitespace token in {TestMethod}", nameof(CreateBearerClient_WithWhitespaceToken_ThrowsArgumentException));
        // Arrange
        var whitespaceToken = HttpClientFactoryTestsConstants.WhitespaceString;

        // Act
        Action act = () => _httpClientFactory.CreateBearerClient(whitespaceToken);

        // Assert
        act.Should().Throw<ArgumentException>();
        _loggerMock.Object.LogInformation("Completed {TestMethod}", nameof(CreateBearerClient_WithWhitespaceToken_ThrowsArgumentException));
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithNullBaseUrl_ThrowsArgumentNullException()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod}", nameof(CreateClientWithBaseUrl_WithNullBaseUrl_ThrowsArgumentNullException));
        _loggerMock.Object.LogWarning("Attempting client creation with a null base URL in {TestMethod}", nameof(CreateClientWithBaseUrl_WithNullBaseUrl_ThrowsArgumentNullException));
        // Arrange
        string? nullBaseUrl = null;

        // Act
        Action act = () => _httpClientFactory.CreateClientWithBaseUrl(nullBaseUrl!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
        _loggerMock.Object.LogInformation("Completed {TestMethod}", nameof(CreateClientWithBaseUrl_WithNullBaseUrl_ThrowsArgumentNullException));
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithEmptyBaseUrl_ThrowsUriFormatException()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod}", nameof(CreateClientWithBaseUrl_WithEmptyBaseUrl_ThrowsUriFormatException));
        _loggerMock.Object.LogWarning("Attempting client creation with an empty base URL in {TestMethod}", nameof(CreateClientWithBaseUrl_WithEmptyBaseUrl_ThrowsUriFormatException));
        // Arrange
        var emptyBaseUrl = HttpClientFactoryTestsConstants.EmptyString;

        // Act
        Action act = () => _httpClientFactory.CreateClientWithBaseUrl(emptyBaseUrl);

        // Assert
        act.Should().Throw<UriFormatException>();
        _loggerMock.Object.LogInformation("Completed {TestMethod}", nameof(CreateClientWithBaseUrl_WithEmptyBaseUrl_ThrowsUriFormatException));
    }

    [Fact]
    public void CreateClientWithBaseUrl_WithInvalidUrl_ThrowsUriFormatException()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with base URL {BaseUrl}", nameof(CreateClientWithBaseUrl_WithInvalidUrl_ThrowsUriFormatException), HttpClientFactoryTestsConstants.InvalidUrlString);
        _loggerMock.Object.LogWarning("Attempting client creation with invalid base URL {BaseUrl}", HttpClientFactoryTestsConstants.InvalidUrlString);
        // Arrange
        var invalidUrl = HttpClientFactoryTestsConstants.InvalidUrlString;

        // Act
        Action act = () => _httpClientFactory.CreateClientWithBaseUrl(invalidUrl);

        // Assert
        act.Should().Throw<UriFormatException>();
        _loggerMock.Object.LogInformation("Completed {TestMethod} with base URL {BaseUrl}", nameof(CreateClientWithBaseUrl_WithInvalidUrl_ThrowsUriFormatException), invalidUrl);
    }

    [Fact]
    public void CreateClient_WithMultipleCalls_ReturnsIndependentClients()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client names {FirstClientName} and {SecondClientName}", nameof(CreateClient_WithMultipleCalls_ReturnsIndependentClients), HttpClientFactoryTestsConstants.TestClientName1, HttpClientFactoryTestsConstants.TestClientName2);
        // Act
        var client1 = _httpClientFactory.CreateClient(HttpClientFactoryTestsConstants.TestClientName1);
        var client2 = _httpClientFactory.CreateClient(HttpClientFactoryTestsConstants.TestClientName2);

        // Assert
        client1.Should().NotBeSameAs(client2);
        client1.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.UserAgentValue);
        client2.DefaultRequestHeaders.UserAgent.ToString().Should().Be(HttpClientFactoryTestsConstants.UserAgentValue);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client names {FirstClientName} and {SecondClientName}", nameof(CreateClient_WithMultipleCalls_ReturnsIndependentClients), HttpClientFactoryTestsConstants.TestClientName1, HttpClientFactoryTestsConstants.TestClientName2);
    }

    [Fact]
    public void CreateAuthenticatedClient_AfterCreateClient_HasBothHeaders()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with authenticated client name {ClientName}", nameof(CreateAuthenticatedClient_AfterCreateClient_HasBothHeaders), HttpClientFactoryTestsConstants.AuthenticatedClientName);
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
        _loggerMock.Object.LogInformation("Completed {TestMethod} with authenticated client name {ClientName}", nameof(CreateAuthenticatedClient_AfterCreateClient_HasBothHeaders), HttpClientFactoryTestsConstants.AuthenticatedClientName);
    }

    [Fact]
    public void CreateBearerClient_AfterCreateClient_HasBothHeaders()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with bearer client name {ClientName}", nameof(CreateBearerClient_AfterCreateClient_HasBothHeaders), HttpClientFactoryTestsConstants.BearerClientName);
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
        _loggerMock.Object.LogInformation("Completed {TestMethod} with bearer client name {ClientName}", nameof(CreateBearerClient_AfterCreateClient_HasBothHeaders), HttpClientFactoryTestsConstants.BearerClientName);
    }

    [Fact]
    public void CreateClientWithBaseUrl_AfterCreateClient_HasBaseAddressAndUserAgent()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with base URL {BaseUrl}", nameof(CreateClientWithBaseUrl_AfterCreateClient_HasBaseAddressAndUserAgent), HttpClientFactoryTestsConstants.ExampleBaseUrl);
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
        _loggerMock.Object.LogInformation("Completed {TestMethod} with base URL {BaseUrl}", nameof(CreateClientWithBaseUrl_AfterCreateClient_HasBaseAddressAndUserAgent), baseUrl);
    }

    [Fact]
    public void CreateClient_TimeoutIsSetTo30Seconds()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with expected timeout {Timeout}", nameof(CreateClient_TimeoutIsSetTo30Seconds), HttpClientFactoryTestsConstants.DefaultTimeout);
        // Act
        var client = _httpClientFactory.CreateClient();

        // Assert
        client.Timeout.Should().Be(HttpClientFactoryTestsConstants.DefaultTimeout);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with actual timeout {Timeout}", nameof(CreateClient_TimeoutIsSetTo30Seconds), client.Timeout);
    }

    [Fact]
    public void CreateClient_WithDefaultName_CreatesClientWithDefaultName()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateClient_WithDefaultName_CreatesClientWithDefaultName), HttpClientFactoryTestsConstants.DefaultClientName);
        // Act
        var client = _httpClientFactory.CreateClient();

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.DefaultClientName), Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateClient_WithDefaultName_CreatesClientWithDefaultName), HttpClientFactoryTestsConstants.DefaultClientName);
    }

    [Fact]
    public void CreateAuthenticatedClient_UsesDefaultAuthenticatedName()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateAuthenticatedClient_UsesDefaultAuthenticatedName), HttpClientFactoryTestsConstants.AuthenticatedClientName);
        // Arrange
        var apiKey = HttpClientFactoryTestsConstants.TestKey;

        // Act
        var client = _httpClientFactory.CreateAuthenticatedClient(apiKey);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.AuthenticatedClientName), Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateAuthenticatedClient_UsesDefaultAuthenticatedName), HttpClientFactoryTestsConstants.AuthenticatedClientName);
    }

    [Fact]
    public void CreateBearerClient_UsesDefaultBearerName()
    {
        _loggerMock.Object.LogInformation("Starting {TestMethod} with client name {ClientName}", nameof(CreateBearerClient_UsesDefaultBearerName), HttpClientFactoryTestsConstants.BearerClientName);
        // Arrange
        var token = HttpClientFactoryTestsConstants.TestToken;

        // Act
        var client = _httpClientFactory.CreateBearerClient(token);

        // Assert
        _httpClientFactoryMock.Verify(f => f.CreateClient(HttpClientFactoryTestsConstants.BearerClientName), Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestMethod} with client name {ClientName}", nameof(CreateBearerClient_UsesDefaultBearerName), HttpClientFactoryTestsConstants.BearerClientName);
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
