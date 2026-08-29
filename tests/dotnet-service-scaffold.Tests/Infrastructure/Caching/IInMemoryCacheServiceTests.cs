#nullable enable

namespace DotnetServiceScaffold.Tests.Infrastructure.Caching;

/// <summary>
/// Interface for unit tests of <see cref="InMemoryCacheService"/>.
/// </summary>
public interface IInMemoryCacheServiceTests
{
    void Dispose();
    Task SetAsync_GetAsync_ReturnsStoredValue();
    Task GetAsync_NonExistentKey_ReturnsNull();
    Task GetAsync_EmptyKey_ReturnsNull();
    Task GetAsync_NullKey_ReturnsNull();
    void SetAsync_NullKey_ThrowsArgumentException();
    void SetAsync_EmptyKey_ThrowsArgumentException();
    Task SetAsync_WithExpiration_EntryExpiresAndIsRemoved();
    Task ExistsAsync_KeyBehavior_ReturnsCorrectResult();
    Task ExistsAsync_ExpiredEntry_ReturnsFalse();
    Task RemoveAsync_RemovesEntryFromCache();
    void RemoveAsync_NullKey_DoesNotThrow();
    void RemoveAsync_EmptyKey_DoesNotThrow();
    Task ClearAsync_RemovesAllEntries();
    Task GetOrSetAsync_CacheHit_ReturnsCachedValueWithoutCallingFactory();
    Task GetOrSetAsync_CacheMiss_CallsFactoryAndCachesResult();
    Task GetOrSetAsync_WithExpiration_CachesWithExpiration();
    Task RemoveByPatternAsync_RemovesMatchingEntries();
    void RemoveByPatternAsync_NullPattern_DoesNotThrow();
}