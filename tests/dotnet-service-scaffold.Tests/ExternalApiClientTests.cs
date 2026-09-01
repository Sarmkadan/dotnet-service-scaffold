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

public class ExternalApiClientTests : IExternalApiClientTests, IEquatable<ExternalApiClientTests>
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

    public override string ToString()
    {
        return $"ExternalApiClientTests {{ Id = {Id}, Name = {Name}, Status = {Status} }}";
    }

    public bool Equals(ExternalApiClientTests? other)
    {
        if (other is null)
            return false;

        return Id == other.Id &&
               Name == other.Name &&
               Status == other.Status;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ExternalApiClientTests);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Status);
    }

    public static bool operator ==(ExternalApiClientTests? left, ExternalApiClientTests? right)
    {
        return EqualityComparer<ExternalApiClientTests>.Default.Equals(left, right);
    }

    public static bool operator !=(ExternalApiClientTests? left, ExternalApiClientTests? right)
    {
        return !(left == right);
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

        // Log start of test
        _loggerMock.Object.LogInformation("Starting test {TestMethodName}", nameof(GetAsync_ValidRequest_ReturnsDeserializedObject));

        // Act
        var result = await _sut.GetAsync<TestObject>(url);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ExternalApiClientTestsConstants.TestObjectId);
        result.Name.Should().Be(ExternalApiClientTestsConstants.TestObjectName);

        // Log end of test
        _loggerMock.Object.LogInformation("Finished test {TestMethodName}", nameof(GetAsync_ValidRequest_ReturnsDeserializedObject));
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

        // Log start of test
        _loggerMock.Object.LogInformation("Starting test {TestMethodName}", nameof(PostAsync_ValidRequest_ReturnsDeserializedObject));

        // Act
        var result = await _sut.PostAsync<ResponseObject>(url, payload);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ExternalApiClientTestsConstants.ResponseStatusCreated);

        // Log end of test
        _loggerMock.Object.LogInformation("Finished test {TestMethodName}", nameof(PostAsync_ValidRequest_ReturnsDeserializedObject));
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

        // Log start of test
        _loggerMock.Object.LogInformation("Starting test {TestMethodName}", nameof(DeleteAsync_ValidRequest_ReturnsTrue));

        // Act
        var result = await _sut.DeleteAsync(url);

        // Assert
        result.Should().BeTrue();

        // Log end of test
        _loggerMock.Object.LogInformation("Finished test {TestMethodName}", nameof(DeleteAsync_ValidRequest_ReturnsTrue));
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

        // Log start of test
        _loggerMock.Object.LogInformation("Starting test {TestMethodName} with url {Url}", nameof(GetAsync_UnsuccessfulResponse_ThrowsHttpRequestException), url);

        // Act
        Func<Task> act = () => _sut.GetAsync<TestObject>(url);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();

        // Log end of test
        _loggerMock.Object.LogInformation("Finished test {TestMethodName}", nameof(GetAsync_UnsuccessfulResponse_ThrowsHttpRequestException));
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
