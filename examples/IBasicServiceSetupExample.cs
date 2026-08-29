public interface IBasicServiceSetupExample
{
    Task<string> RegisterServiceAsync(
        string name,
        string description,
        string healthCheckUrl,
        string ownerId,
        CancellationToken cancellationToken = default);

    Task ListServicesAsync();
    Task EnableServiceAsync(string serviceId);
    Task DisableServiceAsync(string serviceId);
    Task GetServiceDetailsAsync(string serviceId);
}