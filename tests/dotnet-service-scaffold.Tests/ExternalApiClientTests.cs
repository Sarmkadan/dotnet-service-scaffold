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

/// <summary>
/// Verifies HTTP GET, POST, and DELETE behavior of <see cref="ExternalApiClient"/> and provides value-based equality for the test fixture's identifying properties.
/// </summary>
public class ExternalApiClientTests : IExternalApiClientTests, IEquatable<ExternalApiClientTests>
{
    private readonly Mock<ILogger<ExternalApiClient>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ExternalApiClient _sut;

    /// <summary>
    /// Gets or sets the identifier used when comparing test fixture instances.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the name used when comparing test fixture instances.
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// Gets or sets the status used when comparing test fixture instances.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Initializes a test fixture with a mocked HTTP message handler, a mocked logger, and an <see cref="ExternalApiClient"/> configured for localhost.
    /// </summary>
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

    /// <summary>
    /// Formats the fixture's identifier, name, and status as a concise string.
    /// </summary>
    /// <returns>A string containing the current <see cref="Id"/>, <see cref="Name"/>, and <see cref="Status"/> values.</returns>
    public override string ToString()
    {
        return $"ExternalApiClientTests {{ Id = {Id}, Name = {Name}, Status = {Status} }}";
    }

    /// <summary>
    /// Determines whether another fixture has the same identifier, name, and status.
    /// </summary>
    /// <param name="other">The fixture to compare with this instance.</param>
    /// <returns><see langword="true"/> when <paramref name="other"/> is non-null and all three values match; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ExternalApiClientTests? other)
    {
        if (other is null)
            return false;

        return Id == other.Id &&
               Name == other.Name &&
               Status == other.Status;
    }

    /// <summary>
    /// Determines whether an object is an <see cref="ExternalApiClientTests"/> instance with matching identifier, name, and status values.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns><see langword="true"/> when <paramref name="obj"/> is an equivalent fixture; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ExternalApiClientTests);
    }

    /// <summary>
    /// Computes a hash code from the fixture's identifier, name, and status.
    /// </summary>
    /// <returns>A hash code combining <see cref="Id"/>, <see cref="Name"/>, and <see cref="Status"/>.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Status);
    }

    /// <summary>
    /// Determines whether two fixture instances are equal according to the default equality comparer.
    /// </summary>
    /// <param name="left">The fixture on the left side of the comparison.</param>
    /// <param name="right">The fixture on the right side of the comparison.</param>
    /// <returns><see langword="true"/> when the fixtures are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ExternalApiClientTests? left, ExternalApiClientTests? right)
    {
        return EqualityComparer<ExternalApiClientTests>.Default.Equals(left, right);
    }

    /// <summary>
    /// Determines whether two fixture instances are unequal.
    /// </summary>
    /// <param name="left">The fixture on the left side of the comparison.</param>
    /// <param name="right">The fixture on the right side of the comparison.</param>
    /// <returns><see langword="true"/> when the fixtures are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ExternalApiClientTests? left, ExternalApiClientTests? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Verifies that a successful GET response is deserialized into an object with the expected identifier and name.
    /// </summary>
    /// <returns>A task that completes when the asynchronous GET assertions and test logging finish.</returns>
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

    /// <summary>
    /// Verifies that a successful POST response is deserialized into an object with the expected created status.
    /// </summary>
    /// <returns>A task that completes when the asynchronous POST assertions and test logging finish.</returns>
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

    /// <summary>
    /// Verifies that a DELETE response with no content is reported as successful.
    /// </summary>
    /// <returns>A task that completes when the asynchronous DELETE assertion and test logging finish.</returns>
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

    /// <summary>
    /// Verifies that an unsuccessful GET response causes <see cref="HttpRequestException"/> to be thrown.
    /// </summary>
    /// <returns>A task that completes when the asynchronous exception assertion and test logging finish.</returns>
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
