namespace DotnetServiceScaffold.Tests
{
    public interface IExternalApiClientTests
    {
        Task GetAsync_ValidRequest_ReturnsDeserializedObject();
        Task PostAsync_ValidRequest_ReturnsDeserializedObject();
        Task DeleteAsync_ValidRequest_ReturnsTrue();
        Task GetAsync_UnsuccessfulResponse_ThrowsHttpRequestException();
        int Id { get; set; }
        string? Name { get; set; }
        string? Status { get; set; }
    }
}