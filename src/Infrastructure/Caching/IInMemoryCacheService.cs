#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Caching;

/// <summary>
/// Contract for an in-memory cache service with expiration and pattern-based removal.
/// </summary>
public interface IInMemoryCacheService
{
    ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    ValueTask<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    ValueTask RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    ValueTask ClearAsync(CancellationToken cancellationToken = default);
}