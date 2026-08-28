namespace DotnetServiceScaffold.Tests
{
    public interface IExternalApiClientExtensionsTests
    {
        Task GetWithRetryAsync_ValidRequest_ReturnsDeserializedObject();
        Task GetWithRetryAsync_RequestFailsAfterAllRetries_ThrowsHttpRequestException();
        Task GetWithRetryAsync_NullClient_ThrowsArgumentNullException();
        Task GetWithRetryAsync_NullOrEmptyUrl_ThrowsArgumentException();
        Task GetWithRetryAsync_InvalidMaxRetries_ThrowsArgumentOutOfRangeException();
        Task GetWithRetryAsync_InvalidTimeoutSeconds_ThrowsArgumentOutOfRangeException();
        Task PostWithRetryAsync_ValidRequest_ReturnsDeserializedObject();
        Task PostWithRetryAsync_RequestFailsAfterAllRetries_ThrowsHttpRequestException();
        Task PostWithRetryAsync_NullClient_ThrowsArgumentNullException();
        Task PostWithRetryAsync_NullUrl_ThrowsArgumentException();
        Task PostWithRetryAsync_NullPayload_ThrowsArgumentNullException();
        Task PostWithRetryAsync_InvalidMaxRetries_ThrowsArgumentOutOfRangeException();
        Task PutWithRetryAsync_ValidRequest_ReturnsDeserializedObject();
        Task PutWithRetryAsync_RequestFailsAfterAllRetries_ThrowsHttpRequestException();
        Task PutWithRetryAsync_NullClient_ThrowsArgumentNullException();
        Task PutWithRetryAsync_NullUrl_ThrowsArgumentException();
        Task PutWithRetryAsync_NullPayload_ThrowsArgumentNullException();
        Task PutWithRetryAsync_InvalidTimeoutSeconds_ThrowsArgumentOutOfRangeException();
        Task DeleteWithRetryAsync_ValidRequest_ReturnsTrue();
    }
}