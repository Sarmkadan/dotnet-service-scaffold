#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Caching;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Extension methods for <see cref="CacheBenchmarks"/> that provide convenience operations
/// for working with cached service data in benchmark scenarios.
/// </summary>
public static class CacheBenchmarksExtensions
{
    /// <summary>
    /// Gets all healthy services from the cached service list.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>Read-only list of healthy services, or empty list if null or services are null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
    public static async ValueTask<IReadOnlyList<CachedService>> GetHealthyServicesAsync(this CacheBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        var result = await benchmarks.CacheHit();
        return result?.Services
            .Where(s => s.IsHealthy)
            .ToList()
            .AsReadOnly() ?? new List<CachedService>().AsReadOnly();
    }

    /// <summary>
    /// Gets all unhealthy services from the cached service list.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>Read-only list of unhealthy services, or empty list if null or services are null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
    public static async ValueTask<IReadOnlyList<CachedService>> GetUnhealthyServicesAsync(this CacheBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        var result = await benchmarks.CacheHit();
        return result?.Services
            .Where(s => !s.IsHealthy)
            .ToList()
            .AsReadOnly() ?? new List<CachedService>().AsReadOnly();
    }

    /// <summary>
    /// Gets the count of healthy services in the cached service list.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The count of healthy services.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
    public static async ValueTask<int> GetHealthyServiceCountAsync(this CacheBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        var result = await benchmarks.CacheHit();
        return result?.Services.Count(s => s.IsHealthy) ?? 0;
    }

    /// <summary>
    /// Gets the count of unhealthy services in the cached service list.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The count of unhealthy services.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
    public static async ValueTask<int> GetUnhealthyServiceCountAsync(this CacheBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        var result = await benchmarks.CacheHit();
        return result?.Services.Count(s => !s.IsHealthy) ?? 0;
    }

    /// <summary>
    /// Gets a service by its ID from the cached service list.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="serviceId">The service ID to find.</param>
    /// <returns>The service if found, otherwise null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="serviceId"/> is null.</exception>
    public static async ValueTask<CachedService?> GetServiceByIdAsync(this CacheBenchmarks benchmarks, string serviceId)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        ArgumentNullException.ThrowIfNull(serviceId);

        var result = await benchmarks.CacheHit();
        return result?.Services
            .FirstOrDefault(s => string.Equals(s.Id, serviceId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets a service by its name from the cached service list.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="serviceName">The service name to find.</param>
    /// <returns>The service if found, otherwise null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="serviceName"/> is null.</exception>
    public static async ValueTask<CachedService?> GetServiceByNameAsync(this CacheBenchmarks benchmarks, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        ArgumentNullException.ThrowIfNull(serviceName);

        var result = await benchmarks.CacheHit();
        return result?.Services
            .FirstOrDefault(s => string.Equals(s.Name, serviceName, StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets the percentage of healthy services in the cached service list.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The percentage of healthy services (0-100), or 0 if no services.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
    public static async ValueTask<double> GetHealthyPercentageAsync(this CacheBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        var result = await benchmarks.CacheHit();
        var services = result?.Services;
        return services?.Count > 0
            ? Math.Round(100.0 * services.Count(s => s.IsHealthy) / services.Count, 2)
            : 0.0;
    }

    /// <summary>
    /// Gets the percentage of unhealthy services in the cached service list.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The percentage of unhealthy services (0-100), or 0 if no services.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null.</exception>
    public static async ValueTask<double> GetUnhealthyPercentageAsync(this CacheBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        var result = await benchmarks.CacheHit();
        var services = result?.Services;
        return services?.Count > 0
            ? Math.Round(100.0 * services.Count(s => !s.IsHealthy) / services.Count, 2)
            : 0.0;
    }
}