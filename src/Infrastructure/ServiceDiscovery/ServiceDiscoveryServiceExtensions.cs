#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Shared.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Extension methods for <see cref="ServiceDiscoveryService"/> that provide additional
/// convenience methods for service discovery operations.
/// </summary>
public static class ServiceDiscoveryServiceExtensions
{
    /// <summary>
    /// Attempts to discover a service and returns the first healthy endpoint,
    /// or throws if no healthy instances are available.
    /// </summary>
    /// <param name="service">The service discovery service.</param>
    /// <param name="serviceName">Name of the service to discover.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The selected healthy service endpoint.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No healthy instances found for the service.</exception>
    public static async Task<ServiceDiscoveryRecord> DiscoverHealthyEndpointOrThrowAsync(
        this ServiceDiscoveryService service,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        var result = await service.SelectEndpointAsync(serviceName, cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? result.Value
            : throw new InvalidOperationException(
                $"No healthy instances available for service '{serviceName}'. Error: {result.ErrorMessage}");
    }

    /// <summary>
    /// Discovers all services and returns their statistics in a single batch operation.
    /// </summary>
    /// <param name="service">The service discovery service.</param>
    /// <param name="serviceNames">Names of the services to get statistics for.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A dictionary mapping service names to their statistics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="serviceNames"/> is <see langword="null"/>.</exception>
    public static async Task<IReadOnlyDictionary<string, ServiceDiscoveryStats>> GetServiceStatsBatchAsync(
        this ServiceDiscoveryService service,
        IEnumerable<string> serviceNames,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(serviceNames);

        var statsTasks = serviceNames
            .Select(name => service.GetServiceStatsAsync(name, cancellationToken))
            .ToList();

        await Task.WhenAll(statsTasks);

        var successfulResults = statsTasks
            .Where(t => t.IsCompletedSuccessfully)
            .Select(t => t.Result)
            .Where(r => r.IsSuccess && r.Value is not null)
            .Select(r => r.Value!)
            .ToList();

        return successfulResults
            .ToDictionary(
                r => r.ServiceName,
                r => r,
                StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Discovers all services and returns their statistics in a single batch operation.
    /// </summary>
    /// <param name="service">The service discovery service.</param>
    /// <param name="serviceNames">Names of the services to get statistics for.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A dictionary mapping service names to their statistics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static async Task<IReadOnlyDictionary<string, ServiceDiscoveryStats>> GetServiceStatsBatchAsync(
        this ServiceDiscoveryService service,
        params string[] serviceNames)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(serviceNames);

        return await service.GetServiceStatsBatchAsync(serviceNames.AsEnumerable());
    }

    /// <summary>
    /// Gets all registered service names and filters them by a predicate.
    /// </summary>
    /// <param name="service">The service discovery service.</param>
    /// <param name="predicate">Predicate to filter service names.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Filtered list of service names.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static async Task<IReadOnlyList<string>> GetRegisteredServicesAsync(
        this ServiceDiscoveryService service,
        Func<string, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(predicate);

        var result = await service.GetRegisteredServicesAsync(cancellationToken);

        return result.IsSuccess && result.Value is not null
            ? result.Value.Where(predicate).ToList().AsReadOnly()
            : throw new InvalidOperationException(
                $"Failed to retrieve registered services: {result.ErrorMessage}");
    }

    /// <summary>
    /// Discovers services matching a name pattern (wildcard support).
    /// </summary>
    /// <param name="service">The service discovery service.</param>
    /// <param name="namePattern">Service name pattern with wildcards (* and ?).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>List of discovered services matching the pattern.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="namePattern"/> is null or empty.</exception>
    public static async Task<IReadOnlyList<ServiceDiscoveryRecord>> DiscoverByPatternAsync(
        this ServiceDiscoveryService service,
        string namePattern,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(namePattern);

        var allServicesResult = await service.GetRegisteredServicesAsync(cancellationToken);

        if (!allServicesResult.IsSuccess || allServicesResult.Value is null)
            return Array.Empty<ServiceDiscoveryRecord>();

        var matchingServices = allServicesResult.Value
            .Where(name => name is not null && MatchesPattern(name, namePattern))
            .ToList();

        var discoveryTasks = matchingServices
            .Select(name => service.DiscoverAsync(name, cancellationToken))
            .ToList();

        await Task.WhenAll(discoveryTasks);

        return discoveryTasks
            .Where(t => t.IsCompletedSuccessfully)
            .SelectMany(t => t.Result.IsSuccess && t.Result.Value is not null ? t.Result.Value : Array.Empty<ServiceDiscoveryRecord>())
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Checks if a service is healthy by attempting to select an endpoint.
    /// </summary>
    /// <param name="service">The service discovery service.</param>
    /// <param name="serviceName">Name of the service to check.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns><see langword="true"/> if the service has at least one healthy instance; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="serviceName"/> is null or empty.</exception>
    public static async Task<bool> IsServiceHealthyAsync(
        this ServiceDiscoveryService service,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        var result = await service.SelectEndpointAsync(serviceName, cancellationToken);
        return result.IsSuccess;
    }

    /// <summary>
    /// Gets the total count of registered services.
    /// </summary>
    /// <param name="service">The service discovery service.</param>
    /// <returns>Count of registered services.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static async Task<int> GetRegisteredServicesCountAsync(
        this ServiceDiscoveryService service,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetRegisteredServicesAsync(cancellationToken);
        return result.IsSuccess && result.Value is not null ? result.Value.Count : 0;
    }

    // -- Private helpers -------------------------------------------------------

    private static bool MatchesPattern(string input, string pattern)
    {
        if (pattern == "*")
            return true;

        var regexPattern = System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("*", ".*")
            .Replace("?", ".");

        return System.Text.RegularExpressions.Regex.IsMatch(
            input,
            regexPattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase |
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
    }
}