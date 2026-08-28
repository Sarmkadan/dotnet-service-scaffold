using System.Net;
using System.Text.Json;
using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ExternalApiClientTests : IExternalApiClientTests
{
    private readonly Mock<ILogger<ExternalApiClient>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ExternalApiClient _sut;

    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }

    public ExternalApiClientTests()
    {
        _loggerMock = new Mock<ILogger<ExternalApiClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        _sut = new ExternalApiClient(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_ValidRequest_ReturnsDeserializedObject()
    {
        // Arrange
        var url = ExternalApiClientTestsConstants.ApiTestEndpoint;
        var expectedResponse = new { Id = ExternalApiClientTestsConstants.TestObjectId, Name = ExternalApiClientTestsConstants.TestObjectName };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _sut.GetAsync<TestObject>(url);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ExternalApiClientTestsConstants.TestObjectId);
        result.Name.Should().Be(ExternalApiClientTestsConstants.TestObjectName);
    }

    [Fact]
    public async Task PostAsync_ValidRequest_ReturnsDeserializedObject()
    {
        // Arrange
        var url = ExternalApiClientTestsConstants.ApiTestEndpoint;
        var payload = new { Id = ExternalApiClientTestsConstants.TestObjectId, Name = ExternalApiClientTestsConstants.TestObjectName };
        var expectedResponse = new { Status = ExternalApiClientTestsConstants.ResponseStatusCreated };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _sut.PostAsync<ResponseObject>(url, payload);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ExternalApiClientTestsConstants.ResponseStatusCreated);
    }

    [Fact]
    public async Task DeleteAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var url = ExternalApiClientTestsConstants.ApiTestEndpointWithId;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var result = await _sut.DeleteAsync(url);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_UnsuccessfulResponse_ThrowsHttpRequestException()
    {
        // Arrange
        var url = ExternalApiClientTestsConstants.ApiTestEndpoint;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(ExternalApiClientTestsConstants.ErrorContent)
            });

        // Act
        Func<Task> act = () => _sut.GetAsync<TestObject>(url);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class ResponseObject
    {
        public string? Status { get; set; }
    }
}
