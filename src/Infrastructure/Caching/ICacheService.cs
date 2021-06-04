// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Caching;

/// <summary>
/// Interface for a distributed cache service. Abstracts the underlying cache implementation
/// (Redis, in-memory, etc.) and provides a unified API for cache operations.
/// ValueTask is used throughout: the in-memory implementation completes synchronously on every
/// hot path, so callers avoid async-state-machine heap allocation on cache hits.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache. Returns null if key doesn't exist.
    /// </summary>
    ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in the cache with an optional expiration time.
    /// </summary>
    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from the cache, or sets it if it doesn't exist using the factory function.
    /// </summary>
    ValueTask<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes all cached values matching a pattern.
    /// </summary>
    ValueTask RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cached values.
    /// </summary>
    ValueTask ClearAsync(CancellationToken cancellationToken = default);
}
