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
public class InMemoryCacheService : ICacheService, IDisposable
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly Timer? _cleanupTimer;
    private const int CleanupIntervalSeconds = 60;

    public InMemoryCacheService(ILogger<InMemoryCacheService> logger)
    {
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _logger = logger;

        // Start cleanup timer to remove expired entries periodically
        _cleanupTimer = new Timer(
            _ => CleanupExpiredEntries(),
            null,
            TimeSpan.FromSeconds(CleanupIntervalSeconds),
            TimeSpan.FromSeconds(CleanupIntervalSeconds));
    }

    /// <summary>
    /// Gets a value from the cache. Returns null if key doesn't exist or has expired.
    /// Completes synchronously — no async overhead on the hot path.
    /// </summary>
    public ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(key))
            return ValueTask.FromResult<T?>(null);

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                return ValueTask.FromResult<T?>(null);
            }

            _logger.LogDebug("Cache hit for key {Key}", key);
            return ValueTask.FromResult(entry.Value as T);
        }

        _logger.LogDebug("Cache miss for key {Key}", key);
        return ValueTask.FromResult<T?>(null);
    }

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var entry = new CacheEntry
        {
            Value = value,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
        };

        _cache.AddOrUpdate(key, entry, (_, _) => entry);

        _logger.LogDebug(
            "Cached value for key {Key} with expiration {ExpirationSeconds}s",
            key, expiration?.TotalSeconds ?? -1);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            return ValueTask.CompletedTask;

        _cache.TryRemove(key, out _);
        _logger.LogDebug("Removed cache entry for key {Key}", key);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Checks if a key exists in the cache and hasn't expired.
    /// </summary>
    public ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            return ValueTask.FromResult(false);

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                return ValueTask.FromResult(false);
            }
            return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(false);
    }

    /// <summary>
    /// Gets a value from cache or sets it using the factory if not found.
    /// Returns synchronously on a cache hit; invokes the factory asynchronously on a miss.
    /// </summary>
    public async ValueTask<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
            return cached;

        var value = await factory();
        if (value is not null)
            await SetAsync(key, value, expiration, cancellationToken);

        return value;
    }

    /// <summary>
    /// Removes all entries matching a regex pattern.
    /// </summary>
    public ValueTask RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(pattern))
            return ValueTask.CompletedTask;

        try
        {
            var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1.0));
            var keysToRemove = _cache.Keys.Where(k => regex.IsMatch(k)).ToList();

            foreach (var key in keysToRemove)
                _cache.TryRemove(key, out _);

            _logger.LogDebug("Removed {Count} cache entries matching pattern {Pattern}", keysToRemove.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern {Pattern}", pattern);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Clears all cached values.
    /// </summary>
    public ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        var count = _cache.Count;
        _cache.Clear();
        _logger.LogInformation("Cleared cache ({Count} entries removed)", count);

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

        if (expiredKeys.Count > 0)
            _logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
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
}
