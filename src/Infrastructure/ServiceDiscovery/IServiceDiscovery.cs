#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Shared.Models;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Contract for a pluggable discovery backend that can resolve, register,
/// and deregister service instances.
/// </summary>
public interface IServiceDiscoveryProvider
{
    /// <summary>Gets a human-readable name that identifies this provider implementation.</summary>
    string ProviderName { get; }

    /// <summary>
    /// Resolves all healthy instances of the named service from this backend.
    /// </summary>
    /// <param name="serviceName">Logical service name to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A result containing the resolved instances, or a failure result if the backend
    /// is unreachable. An empty list is a success — it means the service is known but
    /// has no healthy instances at this moment.
    /// </returns>
    Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveAsync(
        string serviceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a service instance with this backend.
    /// </summary>
    /// <param name="record">The record describing the instance to register.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result> RegisterAsync(
        ServiceDiscoveryRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deregisters a previously registered service instance.
    /// </summary>
    /// <param name="instanceId">Unique identifier of the instance to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result> DeregisterAsync(
        Guid instanceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams real-time updates when instances for the named service change.
    /// Implementations must yield the full current snapshot on the first iteration.
    /// </summary>
    /// <param name="serviceName">Logical service name to watch.</param>
    /// <param name="cancellationToken">Cancels the watch stream.</param>
    /// <returns>
    /// An async enumerable that yields an updated instance list each time the
    /// set of known endpoints changes.
    /// </returns>
    IAsyncEnumerable<IReadOnlyList<ServiceDiscoveryRecord>> WatchAsync(
        string serviceName,
        CancellationToken cancellationToken = default);

    /// <summary>Checks whether this provider's backend is currently reachable.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// High-level orchestrator for service discovery that manages provider selection,
/// caching, load balancing, and self-registration.
/// </summary>
public interface IServiceDiscoveryService
{
    /// <summary>
    /// Resolves all healthy instances for the specified service using the configured strategy.
    /// Results are served from cache when still within the configured TTL.
    /// </summary>
    /// <param name="serviceName">Logical service name to resolve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> DiscoverAsync(
        string serviceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects a single endpoint using the configured load-balancing strategy.
    /// </summary>
    /// <param name="serviceName">Logical service name to resolve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The chosen <see cref="ServiceDiscoveryRecord"/>, or a failure result
    /// when no healthy instances are available.
    /// </returns>
    Task<Result<ServiceDiscoveryRecord>> SelectEndpointAsync(
        string serviceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers the current service instance with the active discovery backend.
    /// No-op when <see cref="SelfRegistrationOptions.Enabled"/> is <see langword="false"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result> RegisterSelfAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the current service instance from the discovery backend.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result> DeregisterSelfAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all service names currently known to the discovery registry.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result<IReadOnlyList<string>>> GetRegisteredServicesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces cache invalidation for the given service name and triggers a fresh resolution.
    /// Pass <see langword="null"/> to refresh all cached services simultaneously.
    /// </summary>
    /// <param name="serviceName">Service to refresh, or <see langword="null"/> for all.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RefreshAsync(string? serviceName = null, CancellationToken cancellationToken = default);

    /// <summary>Returns aggregated health and resolution statistics for a named service.</summary>
    /// <param name="serviceName">The service name to retrieve statistics for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result<ServiceDiscoveryStats>> GetServiceStatsAsync(
        string serviceName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregated health and resolution statistics for a named discovered service.
/// </summary>
/// <param name="ServiceName">Logical name of the service these statistics relate to.</param>
/// <param name="TotalInstances">Total number of currently resolved instances.</param>
/// <param name="HealthyInstances">Instances in <see cref="DiscoveryHealthStatus.Passing"/> state.</param>
/// <param name="DegradedInstances">Instances in <see cref="DiscoveryHealthStatus.Warning"/> state.</param>
/// <param name="CriticalInstances">Instances in <see cref="DiscoveryHealthStatus.Critical"/> state.</param>
/// <param name="LastResolvedAt">UTC timestamp of the most recent successful resolution.</param>
/// <param name="CacheExpiresAt">UTC timestamp when the current cache entry expires.</param>
/// <param name="ActiveSource">The backend that produced the current cache entry.</param>
public sealed record ServiceDiscoveryStats(
    string ServiceName,
    int TotalInstances,
    int HealthyInstances,
    int DegradedInstances,
    int CriticalInstances,
    DateTime? LastResolvedAt,
    DateTime? CacheExpiresAt,
    DiscoverySource ActiveSource)
{
    /// <summary>Percentage of instances currently considered healthy (0–100).</summary>
    public double HealthPercent => TotalInstances == 0
        ? 0.0
        : Math.Round((double)HealthyInstances / TotalInstances * 100.0, 1);

    /// <summary>
    /// <see langword="true"/> when at least one healthy instance is available for traffic routing.
    /// </summary>
    public bool HasCapacity => HealthyInstances > 0;
}
