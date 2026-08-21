using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Shared.Models;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

public interface IDnsServiceDiscoveryProvider
{
    Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveAsync(
        string serviceName,
        CancellationToken cancellationToken = default);

    Task<Result> RegisterAsync(
        ServiceDiscoveryRecord record,
        CancellationToken cancellationToken = default);

    Task<Result> DeregisterAsync(
        Guid instanceId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IReadOnlyList<ServiceDiscoveryRecord>> WatchAsync(
        string serviceName,
        CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
