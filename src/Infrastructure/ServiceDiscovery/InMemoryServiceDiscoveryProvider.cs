#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Shared.Models;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// In-memory service discovery provider that maintains a volatile registry of service instances.
/// Useful for testing, development, and scenarios where external dependencies are undesirable.
/// All state is lost when the application restarts.
/// </summary>
public sealed class InMemoryServiceDiscoveryProvider : IServiceDiscoveryProvider
{
    private readonly ConcurrentDictionary<string, List<ServiceDiscoveryRecord>> _services = new();
    private readonly ConcurrentDictionary<Guid, ServiceDiscoveryRecord> _instances = new();
    private readonly ILogger<InMemoryServiceDiscoveryProvider> _logger;
    private readonly object _syncLock = new();

    /// <summary>
    /// Gets the human-readable name of the in-memory service discovery provider.
    /// </summary>
    public string ProviderName => "InMemory";

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryServiceDiscoveryProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record service discovery activity.</param>
    public InMemoryServiceDiscoveryProvider(ILogger<InMemoryServiceDiscoveryProvider> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Resolves the healthy instances registered for the specified service.
    /// </summary>
    /// <param name="serviceName">The logical name of the service to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A completed task containing the healthy service records, an empty collection when no instances
    /// are registered, or a failure result when resolution fails.
    /// </returns>
    public Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(Result<IReadOnlyList<ServiceDiscoveryRecord>>.Failure("Operation cancelled.", "CANCELLED"));

        try
        {
            if (_services.TryGetValue(serviceName.ToLowerInvariant(), out var records))
            {
                var healthyRecords = records.Where(r => r.IsAlive()).ToList();
                _logger.LogDebug("InMemory resolved {Count} healthy instance(s) for {ServiceName}", healthyRecords.Count, serviceName);
                return Task.FromResult(Result<IReadOnlyList<ServiceDiscoveryRecord>>.Success(healthyRecords.AsReadOnly()));
            }

            _logger.LogDebug("InMemory found no instances for {ServiceName}", serviceName);
            return Task.FromResult(Result<IReadOnlyList<ServiceDiscoveryRecord>>.Success(Array.Empty<ServiceDiscoveryRecord>()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InMemory resolution failed for service {ServiceName}", serviceName);
            return Task.FromResult(Result<IReadOnlyList<ServiceDiscoveryRecord>>.Failure(ex));
        }
    }

    /// <summary>
    /// Registers or replaces a service instance in the in-memory registry.
    /// </summary>
    /// <param name="record">The service discovery record to register.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A completed task containing the result of the registration operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="record"/> is <see langword="null"/>.
    /// </exception>
    public Task<Result> RegisterAsync(
        ServiceDiscoveryRecord record,
        CancellationToken cancellationToken = default)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));

        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(Result.Failure("Operation cancelled.", "CANCELLED"));

        try
        {
            lock (_syncLock)
            {
                // Remove any existing instance with the same ID
                if (_instances.TryRemove(record.InstanceId, out _))
                {
                    _logger.LogDebug("InMemory replaced existing instance {InstanceId} for {ServiceName}", record.InstanceId, record.ServiceName);
                }

                // Add the new/updated instance
                _instances[record.InstanceId] = record;

                // Add to service index
                var serviceKey = record.ServiceName.ToLowerInvariant();
                if (!_services.TryGetValue(serviceKey, out var serviceList))
                {
                    serviceList = new List<ServiceDiscoveryRecord>();
                    _services[serviceKey] = serviceList;
                }
                else
                {
                    // Remove existing instance with same ID if it exists
                    serviceList.RemoveAll(r => r.InstanceId == record.InstanceId);
                }

                serviceList.Add(record);

                _logger.LogInformation("InMemory registered {ServiceName}/{InstanceId} ({Host}:{Port})",
                    record.ServiceName, record.InstanceId, record.Host, record.Port);
                return Task.FromResult(Result.Success());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InMemory registration failed for {ServiceName}/{InstanceId}", record.ServiceName, record.InstanceId);
            return Task.FromResult(Result.Failure(ex));
        }
    }

    /// <summary>
    /// Removes a service instance from the in-memory registry.
    /// </summary>
    /// <param name="instanceId">The identifier of the service instance to remove.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A completed task containing a successful result when the instance is removed or is not registered,
    /// or a failure result when deregistration fails.
    /// </returns>
    public Task<Result> DeregisterAsync(
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(Result.Failure("Operation cancelled.", "CANCELLED"));

        try
        {
            lock (_syncLock)
            {
                if (_instances.TryRemove(instanceId, out var record))
                {
                    var serviceKey = record.ServiceName.ToLowerInvariant();
                    if (_services.TryGetValue(serviceKey, out var serviceList))
                    {
                        serviceList.RemoveAll(r => r.InstanceId == instanceId);
                        if (serviceList.Count == 0)
                        {
                            _services.TryRemove(serviceKey, out _);
                        }
                    }

                    _logger.LogInformation("InMemory deregistered instance {InstanceId} for {ServiceName}", instanceId, record.ServiceName);
                    return Task.FromResult(Result.Success());
                }

                _logger.LogDebug("InMemory found no instance {InstanceId} to deregister", instanceId);
                return Task.FromResult(Result.Success()); // Idempotent - success if already removed
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InMemory deregistration failed for instance {InstanceId}", instanceId);
            return Task.FromResult(Result.Failure(ex));
        }
    }

    /// <summary>
    /// Watches the specified service and yields a snapshot when its healthy endpoint set changes.
    /// </summary>
    /// <param name="serviceName">The logical name of the service to watch.</param>
    /// <param name="cancellationToken">The token used to stop watching the service.</param>
    /// <returns>
    /// An asynchronous sequence of healthy service record snapshots produced when the endpoint set changes.
    /// </returns>
    /// <remarks>The registry is polled once per second until cancellation is requested.</remarks>
    public IAsyncEnumerable<IReadOnlyList<ServiceDiscoveryRecord>> WatchAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        return WatchInternalAsync(serviceName, cancellationToken);
    }

    private async IAsyncEnumerable<IReadOnlyList<ServiceDiscoveryRecord>> WatchInternalAsync(
        string serviceName,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var previousUris = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await ResolveAsync(serviceName, cancellationToken);
            if (result.IsSuccess && result.Value is { } current)
            {
                var currentUris = current.Select(r => r.ToEndpointUri())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (!currentUris.SetEquals(previousUris))
                {
                    previousUris = currentUris;
                    yield return current;
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// Checks whether the in-memory service discovery provider is available.
    /// </summary>
    /// <param name="cancellationToken">The token associated with the availability check.</param>
    /// <returns>A completed task containing <see langword="true"/>.</returns>
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // In-memory provider is always available
        return Task.FromResult(true);
    }
}
