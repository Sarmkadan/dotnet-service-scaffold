#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Caching;

/// <summary>
/// In-memory cache implementation using ConcurrentDictionary. Suitable for single-node
/// deployments or development. For distributed deployments, use Redis implementation.
/// Implements automatic expiration and cleanup of expired entries.
/// All synchronous-completing paths return ValueTask.FromResult to avoid async state-machine
/// heap allocation on cache hits.
/// </summary>
public class InMemoryCacheService : ICacheService, IDisposable, IInMemoryCacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly Timer? _cleanupTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryCacheService"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record cache operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <see langword="null"/>.</exception>
    public InMemoryCacheService(ILogger<InMemoryCacheService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _logger = logger;

        // Start cleanup timer to remove expired entries periodically
        _cleanupTimer = new Timer(
            _ => CleanupExpiredEntries(),
            null,
            TimeSpan.FromSeconds(InMemoryCacheServiceConstants.CleanupIntervalSeconds),
            TimeSpan.FromSeconds(InMemoryCacheServiceConstants.CleanupIntervalSeconds));
    }

    /// <summary>
    /// Gets a value from the cache. Returns null if key doesn't exist or has expired.
    /// Completes synchronously — no async overhead on the hot path.
    /// </summary>
    /// <typeparam name="T">The reference type of the cached value.</typeparam>
    /// <param name="key">The key of the cached value.</param>
    /// <param name="cancellationToken">The token associated with the operation.</param>
    /// <returns>The cached value when found and unexpired; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <see langword="null"/> or empty.</exception>
    public ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _logger.LogInformation(InMemoryCacheServiceConstants.GettingCacheEntryLogMessage, key);

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                _logger.LogInformation(InMemoryCacheServiceConstants.CacheEntryExpiredLogMessage, key);
                return ValueTask.FromResult<T?>(null);
            }

            _logger.LogInformation(InMemoryCacheServiceConstants.CacheHitLogMessage, key);
            _logger.LogDebug(InMemoryCacheServiceConstants.CacheHitLogMessage, key);
            return ValueTask.FromResult(entry.Value as T);
        }

        _logger.LogInformation(InMemoryCacheServiceConstants.CacheMissLogMessage, key);
        _logger.LogDebug(InMemoryCacheServiceConstants.CacheMissLogMessage, key);
        return ValueTask.FromResult<T?>(null);
    }

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    /// <typeparam name="T">The reference type of the value to cache.</typeparam>
    /// <param name="key">The key under which to store the value.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">The optional duration after which the value expires.</param>
    /// <param name="cancellationToken">The token associated with the operation.</param>
    /// <returns>A value task that represents the completed operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
        _logger.LogInformation(InMemoryCacheServiceConstants.SettingCacheEntryLogMessage, key);

        var entry = new CacheEntry
        {
            Value = value,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
        };

        _cache.AddOrUpdate(key, entry, (_, _) => entry);

        _logger.LogInformation(InMemoryCacheServiceConstants.CachedValueLogMessage, key, expiration?.TotalSeconds ?? InMemoryCacheServiceConstants.NoExpirationSeconds);
        _logger.LogDebug(
            InMemoryCacheServiceConstants.CachedValueLogMessage,
            key, expiration?.TotalSeconds ?? InMemoryCacheServiceConstants.NoExpirationSeconds);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <param name="cancellationToken">The token associated with the operation.</param>
    /// <returns>A value task that represents the completed operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <see langword="null"/> or empty.</exception>
    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _logger.LogInformation(InMemoryCacheServiceConstants.RemovingCacheEntryLogMessage, key);

        _cache.TryRemove(key, out _);
        _logger.LogInformation(InMemoryCacheServiceConstants.RemovedCacheEntryLogMessage, key);
        _logger.LogDebug(InMemoryCacheServiceConstants.RemovedCacheEntryLogMessage, key);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Checks if a key exists in the cache and hasn't expired.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="cancellationToken">The token associated with the operation.</param>
    /// <returns><see langword="true"/> when the key exists and is unexpired; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <see langword="null"/> or empty.</exception>
    public ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _logger.LogInformation(InMemoryCacheServiceConstants.CheckingCacheEntryExistenceLogMessage, key);

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                _logger.LogInformation(InMemoryCacheServiceConstants.CacheEntryExpiredLogMessage, key);
                return ValueTask.FromResult(false);
            }
            _logger.LogInformation(InMemoryCacheServiceConstants.CacheEntryExistsLogMessage, key);
            return ValueTask.FromResult(true);
        }

        _logger.LogInformation(InMemoryCacheServiceConstants.CacheEntryDoesNotExistLogMessage, key);
        return ValueTask.FromResult(false);
    }

    /// <summary>
    /// Gets a value from cache or sets it using the factory if not found.
    /// Returns synchronously on a cache hit; invokes the factory asynchronously on a miss.
    /// </summary>
    /// <typeparam name="T">The reference type of the cached value.</typeparam>
    /// <param name="key">The key of the cached value.</param>
    /// <param name="factory">The asynchronous factory used to create the value on a cache miss.</param>
    /// <param name="expiration">The optional duration after which a created value expires.</param>
    /// <param name="cancellationToken">The token associated with the operation.</param>
    /// <returns>The cached or created value, or <see langword="null"/> if the factory returns <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is <see langword="null"/>.</exception>
    public async ValueTask<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(factory);
        _logger.LogInformation(InMemoryCacheServiceConstants.GettingOrSettingCacheEntryLogMessage, key);

        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation(InMemoryCacheServiceConstants.GetOrSetCacheHitLogMessage, key);
            return cached;
        }

        _logger.LogInformation(InMemoryCacheServiceConstants.GetOrSetCacheMissLogMessage, key);
        var value = await factory();
        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
            _logger.LogInformation(InMemoryCacheServiceConstants.GetOrSetValueSetLogMessage, key);
        }
        else
        {
            _logger.LogWarning(InMemoryCacheServiceConstants.GetOrSetFactoryReturnedNullLogMessage, key);
        }

        return value;
    }

    /// <summary>
    /// Removes all entries matching a regex pattern.
    /// </summary>
    /// <param name="pattern">The regular expression used to match cache keys.</param>
    /// <param name="cancellationToken">The token associated with the operation.</param>
    /// <returns>A value task that represents the completed operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pattern"/> is <see langword="null"/> or empty.</exception>
    public ValueTask RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern);

        try
        {
            var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(InMemoryCacheServiceConstants.RegexTimeoutSeconds));
            var keysToRemove = _cache.Keys.Where(k => regex.IsMatch(k)).ToList();

            foreach (var key in keysToRemove)
                _cache.TryRemove(key, out _);

            _logger.LogDebug(InMemoryCacheServiceConstants.RemovedMatchingCacheEntriesLogMessage, keysToRemove.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, InMemoryCacheServiceConstants.RemoveByPatternErrorLogMessage, pattern);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Clears all cached values.
    /// </summary>
    /// <param name="cancellationToken">The token associated with the operation.</param>
    /// <returns>A value task that represents the completed operation.</returns>
    public ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        var count = _cache.Count;
        _cache.Clear();
        _logger.LogInformation(InMemoryCacheServiceConstants.ClearedCacheLogMessage, count);

        return ValueTask.CompletedTask;
    }

    private void CleanupExpiredEntries()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
            _cache.TryRemove(key, out _);

        if (expiredKeys.Count > InMemoryCacheServiceConstants.NoExpiredEntries)
            _logger.LogDebug(InMemoryCacheServiceConstants.CleanedUpExpiredEntriesLogMessage, expiredKeys.Count);
    }

    /// <summary>
    /// Releases the timer used to clean up expired cache entries.
    /// </summary>
    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    /// <summary>
    /// Returns a string that represents the cache service and its current entry count.
    /// </summary>
    /// <returns>A string containing the number of entries currently stored in the cache.</returns>
    public override string ToString() => string.Format(InMemoryCacheServiceConstants.CacheServiceDisplayFormat, _cache.Count);
}

/// <summary>
/// Internal class representing a cached entry with expiration tracking.
/// </summary>
internal class CacheEntry
{
    public object? Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    public override string ToString()
    {
        return string.Format(InMemoryCacheServiceConstants.CacheEntryDisplayFormat, Value, CreatedAt, ExpiresAt);
    }
}
