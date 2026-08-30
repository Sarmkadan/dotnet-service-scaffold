using System;
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

public class ExternalApiClientExtensionsTests : IEquatable<ExternalApiClientExtensionsTests>, IExternalApiClientExtensionsTests
{
    private readonly Mock<ILogger<ExternalApiClient>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ExternalApiClient _sut;

    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }

    public bool Equals(ExternalApiClientExtensionsTests? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Name == other.Name && Status == other.Status;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ExternalApiClientExtensionsTests);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Status);
    }

    public static bool operator ==(ExternalApiClientExtensionsTests? left, ExternalApiClientExtensionsTests? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ExternalApiClientExtensionsTests? left, ExternalApiClientExtensionsTests? right)
    {
        return !Equals(left, right);
    }

    public ExternalApiClientExtensionsTests()
    {
        _loggerMock = new Mock<ILogger<ExternalApiClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(ExternalApiClientExtensionsTestsConstants.LocalhostBaseAddress)
        };

        _sut = new ExternalApiClient(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetWithRetryAsync_ValidRequest_ReturnsDeserializedObject()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;
        var expectedResponse = new { Id = 1, Name = "Test" };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.InternalServerError,
                Content = new StringContent(ExternalApiClientExtensionsTestsConstants.ErrorContent)
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.Ok,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _sut.GetWithRetryAsync<TestObject>(url);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetWithRetryAsync_RequestFailsAfterAllRetries_ThrowsHttpRequestException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.InternalServerError,
                Content = new StringContent(ExternalApiClientExtensionsTestsConstants.ErrorContent)
            });

        // Act
        Func<Task> act = () => _sut.GetWithRetryAsync<TestObject>(url, maxRetries: ExternalApiClientExtensionsTestsConstants.DefaultRetryCount);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage(ExternalApiClientExtensionsTestsConstants.FailedAfterRetriesMessage);
    }

    [Fact]
    public async Task GetWithRetryAsync_NullClient_ThrowsArgumentNullException()
    {
        // Arrange
        ExternalApiClient? nullClient = null;
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;

        // Act
        Func<Task> act = () => nullClient!.GetWithRetryAsync<TestObject>(url);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetWithRetryAsync_NullOrEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var url = "";

        // Act
        Func<Task> act = () => _sut.GetWithRetryAsync<TestObject>(url);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetWithRetryAsync_InvalidMaxRetries_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;

        // Act
        Func<Task> act = () => _sut.GetWithRetryAsync<TestObject>(url, maxRetries: ExternalApiClientExtensionsTestsConstants.InvalidZero);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetWithRetryAsync_InvalidTimeoutSeconds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;

        // Act
        Func<Task> act = () => _sut.GetWithRetryAsync<TestObject>(url, timeoutSeconds: ExternalApiClientExtensionsTestsConstants.InvalidZero);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task PostWithRetryAsync_ValidRequest_ReturnsDeserializedObject()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;
        var payload = new { Id = 1, Name = "Test" };
        var expectedResponse = new { Status = "Created" };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.InternalServerError,
                Content = new StringContent(ExternalApiClientExtensionsTestsConstants.ErrorContent)
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.Created,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _sut.PostWithRetryAsync<ResponseObject>(url, payload);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Created");
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task PostWithRetryAsync_RequestFailsAfterAllRetries_ThrowsHttpRequestException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;
        var payload = new { Id = 1 };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.InternalServerError,
                Content = new StringContent(ExternalApiClientExtensionsTestsConstants.ErrorContent)
            });

        // Act
        Func<Task> act = () => _sut.PostWithRetryAsync<ResponseObject>(url, payload, maxRetries: ExternalApiClientExtensionsTestsConstants.SingleRetryCount);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*failed after 1 retries*");
    }

    [Fact]
    public async Task PostWithRetryAsync_NullClient_ThrowsArgumentNullException()
    {
        // Arrange
        ExternalApiClient? nullClient = null;
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;
        var payload = new { Id = 1 };

        // Act
        Func<Task> act = () => nullClient!.PostWithRetryAsync<ResponseObject>(url, payload);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PostWithRetryAsync_NullUrl_ThrowsArgumentException()
    {
        // Arrange
        var payload = new { Id = 1 };

        // Act
        Func<Task> act = () => _sut.PostWithRetryAsync<ResponseObject>(null!, payload);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PostWithRetryAsync_NullPayload_ThrowsArgumentNullException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;
        object? nullPayload = null;

        // Act
        Func<Task> act = () => _sut.PostWithRetryAsync<ResponseObject>(url, nullPayload!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PostWithRetryAsync_InvalidMaxRetries_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;
        var payload = new { Id = 1 };

        // Act
        Func<Task> act = () => _sut.PostWithRetryAsync<ResponseObject>(url, payload, maxRetries: ExternalApiClientExtensionsTestsConstants.InvalidNegativeOne);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task PutWithRetryAsync_ValidRequest_ReturnsDeserializedObject()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;
        var payload = new { Id = 1, Name = "Updated" };
        var expectedResponse = new { Status = "Updated" };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.InternalServerError,
                Content = new StringContent(ExternalApiClientExtensionsTestsConstants.ErrorContent)
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.Ok,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _sut.PutWithRetryAsync<ResponseObject>(url, payload);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Updated");
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task PutWithRetryAsync_RequestFailsAfterAllRetries_ThrowsHttpRequestException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;
        var payload = new { Id = 1 };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.InternalServerError,
                Content = new StringContent(ExternalApiClientExtensionsTestsConstants.ErrorContent)
            });

        // Act
        Func<Task> act = () => _sut.PutWithRetryAsync<ResponseObject>(url, payload, maxRetries: ExternalApiClientExtensionsTestsConstants.SingleRetryCount);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*failed after 1 retries*");
    }

    [Fact]
    public async Task PutWithRetryAsync_NullClient_ThrowsArgumentNullException()
    {
        // Arrange
        ExternalApiClient? nullClient = null;
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;
        var payload = new { Id = 1 };

        // Act
        Func<Task> act = () => nullClient!.PutWithRetryAsync<ResponseObject>(url, payload);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PutWithRetryAsync_NullUrl_ThrowsArgumentException()
    {
        // Arrange
        var payload = new { Id = 1 };

        // Act
        Func<Task> act = () => _sut.PutWithRetryAsync<ResponseObject>(null!, payload);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PutWithRetryAsync_NullPayload_ThrowsArgumentNullException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;
        object? nullPayload = null;

        // Act
        Func<Task> act = () => _sut.PutWithRetryAsync<ResponseObject>(url, nullPayload!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PutWithRetryAsync_InvalidTimeoutSeconds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;
        var payload = new { Id = 1 };

        // Act
        Func<Task> act = () => _sut.PutWithRetryAsync<ResponseObject>(url, payload, timeoutSeconds: ExternalApiClientExtensionsTestsConstants.InvalidNegativeFive);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task DeleteWithRetryAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.Ok
            });

        // Act
        var result = await _sut.DeleteWithRetryAsync(url);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWithRetryAsync_FailedRequestAfterAllRetries_ReturnsFalse()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.InternalServerError,
                Content = new StringContent(ExternalApiClientExtensionsTestsConstants.ErrorContent)
            });

        // Act
        var result = await _sut.DeleteWithRetryAsync(url, maxRetries: ExternalApiClientExtensionsTestsConstants.TripleRetryCount);

        // Assert - DeleteWithRetryAsync returns false when base DeleteAsync returns false after all retries
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteWithRetryAsync_NullClient_ThrowsArgumentNullException()
    {
        // Arrange
        ExternalApiClient? nullClient = null;
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;

        // Act
        Func<Task> act = () => nullClient!.DeleteWithRetryAsync(url);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteWithRetryAsync_NullOrEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var url = "";

        // Act
        Func<Task> act = () => _sut.DeleteWithRetryAsync(url);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteWithRetryAsync_InvalidMaxRetries_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;

        // Act
        Func<Task> act = () => _sut.DeleteWithRetryAsync(url, maxRetries: ExternalApiClientExtensionsTestsConstants.InvalidNegativeTen);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task DeleteWithRetryAsync_InvalidTimeoutSeconds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpointWithId;

        // Act
        Func<Task> act = () => _sut.DeleteWithRetryAsync(url, timeoutSeconds: ExternalApiClientExtensionsTestsConstants.InvalidZero);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetWithRetryAsync_WithHeaders_SendsHeadersWithRequest()
    {
        // Arrange
        var url = ExternalApiClientExtensionsTestsConstants.TestApiEndpoint;
        var headers = new Dictionary<string, string> { { ExternalApiClientExtensionsTestsConstants.AuthorizationHeader, ExternalApiClientExtensionsTestsConstants.BearerTokenValue }, { ExternalApiClientExtensionsTestsConstants.CustomHeader, ExternalApiClientExtensionsTestsConstants.CustomHeaderValue } };
        var expectedResponse = new { Id = 1 };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Headers.Contains("Authorization") &&
                    msg.Headers.GetValues("Authorization").First() == "Bearer token" &&
                    msg.Headers.Contains("X-Custom") &&
                    msg.Headers.GetValues("X-Custom").First() == "value"),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = ExternalApiClientExtensionsTestsConstants.Ok,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _sut.GetWithRetryAsync<TestObject>(url, headers: headers);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
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
