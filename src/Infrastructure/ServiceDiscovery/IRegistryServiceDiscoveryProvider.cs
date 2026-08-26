#nullable enable
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Shared.Models;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery
{
    public interface IRegistryServiceDiscoveryProvider
    {
        string ProviderName { get; }
        Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveAsync(string serviceName, CancellationToken cancellationToken = default);
        Task<Result> RegisterAsync(ServiceDiscoveryRecord record, CancellationToken cancellationToken = default);
        Task<Result> DeregisterAsync(Guid instanceId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<IReadOnlyList<ServiceDiscoveryRecord>> WatchAsync(string serviceName, [EnumeratorCancellation] CancellationToken cancellationToken = default);
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<string>>> GetAllServiceNamesAsync(CancellationToken cancellationToken = default);
    }
}