#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetServiceScaffold.Tests.Infrastructure.Caching;

/// <summary>
/// Unit tests for <see cref="InMemoryCacheService"/>.
/// Tests set/get operations, expiry, remove, and concurrent access.
/// </summary>
public class InMemoryCacheServiceTests : IDisposable, IInMemoryCacheServiceTests
{
    private readonly Mock<ILogger<InMemoryCacheService>> _loggerMock;
    private readonly InMemoryCacheService _cache;
    private bool _disposed;

    public InMemoryCacheServiceTests()
    {
        _loggerMock = new Mock<ILogger<InMemoryCacheService>>();
        _cache = new InMemoryCacheService(_loggerMock.Object);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _cache.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Tests that SetAsync stores a value and GetAsync retrieves it correctly.
    /// </summary>
    [Fact]
    public async Task SetAsync_GetAsync_ReturnsStoredValue()
    {
        // Arrange
        const string key = "test-key";
        const string value = "test-value";

        // Act
        await _cache.SetAsync(key, value);
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cached value for key test-key")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetAsync returns null for non-existent keys.
    /// </summary>
    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        // Arrange & Act
        var result = await _cache.GetAsync<string>("non-existent-key");

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache miss for key non-existent-key")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetAsync returns null for empty key.
    /// </summary>
    [Fact]
    public async Task GetAsync_EmptyKey_ReturnsNull()
    {
        // Arrange & Act
        var result = await _cache.GetAsync<string>("");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetAsync returns null for null key.
    /// </summary>
    [Fact]
    public async Task GetAsync_NullKey_ReturnsNull()
    {
        // Arrange & Act
        var result = await _cache.GetAsync<string>(null!);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that SetAsync throws ArgumentException for null key.
    /// </summary>
    [Fact]
    public void SetAsync_NullKey_ThrowsArgumentException()
    {
        // Arrange
        const string value = "test-value";

        // Act
        Action act = () => _cache.SetAsync(null!, value);

        // Assert
        act.Should().Throw<ArgumentException>(
            "because null key is not allowed");
    }

    /// <summary>
    /// Tests that SetAsync throws ArgumentException for empty key.
    /// </summary>
    [Fact]
    public void SetAsync_EmptyKey_ThrowsArgumentException()
    {
        // Arrange
        const string value = "test-value";

        // Act
        Action act = () => _cache.SetAsync("", value);

        // Assert
        act.Should().Throw<ArgumentException>(
            "because empty key is not allowed");
    }

    /// <summary>
    /// Tests that values with expiration are automatically removed when expired.
    /// </summary>
    [Fact]
    public async Task SetAsync_WithExpiration_EntryExpiresAndIsRemoved()
    {
        // Arrange
        const string key = "expiring-key";
        const string value = "expiring-value";

        // Act - set with 10ms expiration
        await _cache.SetAsync(key, value, TimeSpan.FromMilliseconds(10));

        // Assert - should be present immediately
        var result = await _cache.GetAsync<string>(key);
        result.Should().Be(value);

        // Wait for expiration
        await Task.Delay(20);

        // Assert - should be null after expiration
        result = await _cache.GetAsync<string>(key);
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that ExistsAsync returns true for existing keys and false for non-existent keys.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_KeyBehavior_ReturnsCorrectResult()
    {
        // Arrange
        const string existingKey = "existing-key";
        const string nonExistentKey = "non-existent-key";

        await _cache.SetAsync(existingKey, "value");

        // Act & Assert
        var exists = await _cache.ExistsAsync(existingKey);
        exists.Should().BeTrue();

        exists = await _cache.ExistsAsync(nonExistentKey);
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Tests that ExistsAsync returns false for expired entries.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ExpiredEntry_ReturnsFalse()
    {
        // Arrange
        const string key = "expiring-key";

        await _cache.SetAsync(key, "value", TimeSpan.FromMilliseconds(10));

        // Wait for expiration
        await Task.Delay(20);

        // Act & Assert
        var exists = await _cache.ExistsAsync(key);
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Tests that RemoveAsync removes an entry from the cache.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_RemovesEntryFromCache()
    {
        // Arrange
        const string key = "removable-key";
        const string value = "removable-value";

        await _cache.SetAsync(key, value);
        var existsBefore = await _cache.ExistsAsync(key);
        existsBefore.Should().BeTrue();

        // Act
        await _cache.RemoveAsync(key);

        // Assert
        var existsAfter = await _cache.ExistsAsync(key);
        existsAfter.Should().BeFalse();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Removed cache entry for key removable-key")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RemoveAsync with null key doesn't throw.
    /// </summary>
    [Fact]
    public void RemoveAsync_NullKey_DoesNotThrow()
    {
        // Act - should not throw
        Action act = () => _cache.RemoveAsync(null!);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that RemoveAsync with empty key doesn't throw.
    /// </summary>
    [Fact]
    public void RemoveAsync_EmptyKey_DoesNotThrow()
    {
        // Act - should not throw
        Action act = () => _cache.RemoveAsync("");
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that ClearAsync removes all entries from the cache.
    /// </summary>
    [Fact]
    public async Task ClearAsync_RemovesAllEntries()
    {
        // Arrange - add multiple entries
        await _cache.SetAsync("key1", "value1");
        await _cache.SetAsync("key2", "value2");
        await _cache.SetAsync("key3", "value3");

        // Verify entries exist
        (await _cache.ExistsAsync("key1")).Should().BeTrue();
        (await _cache.ExistsAsync("key2")).Should().BeTrue();
        (await _cache.ExistsAsync("key3")).Should().BeTrue();

        // Act
        await _cache.ClearAsync();

        // Assert - all entries should be gone
        (await _cache.ExistsAsync("key1")).Should().BeFalse();
        (await _cache.ExistsAsync("key2")).Should().BeFalse();
        (await _cache.ExistsAsync("key3")).Should().BeFalse();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleared cache")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetOrSetAsync returns cached value on hit.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_CacheHit_ReturnsCachedValueWithoutCallingFactory()
    {
        // Arrange
        const string key = "factory-key";
        const string cachedValue = "cached-value";
        var factoryCallCount = 0;

        await _cache.SetAsync(key, cachedValue);

        // Act - factory should not be called on cache hit
        var result = await _cache.GetOrSetAsync(
            key,
            () =>
            {
                factoryCallCount++;
                return Task.FromResult("new-value");
            });

        // Assert
        result.Should().Be(cachedValue);
        factoryCallCount.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetOrSetAsync calls factory and caches result on cache miss.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_CacheMiss_CallsFactoryAndCachesResult()
    {
        // Arrange
        const string key = "factory-key";
        var factoryCallCount = 0;

        // Act - factory should be called on cache miss
        var result = await _cache.GetOrSetAsync(
            key,
            () =>
            {
                factoryCallCount++;
                return Task.FromResult("computed-value");
            });

        // Assert
        result.Should().Be("computed-value");
        factoryCallCount.Should().Be(1);

        // Verify it was cached
        var cachedResult = await _cache.GetAsync<string>(key);
        cachedResult.Should().Be("computed-value");
    }

    /// <summary>
    /// Tests that GetOrSetAsync with expiration caches the result with expiration.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_WithExpiration_CachesWithExpiration()
    {
        // Arrange
        const string key = "expiring-factory-key";

        // Act
        var result = await _cache.GetOrSetAsync(
            key,
            () => Task.FromResult("computed-value"),
            TimeSpan.FromMilliseconds(10));

        // Assert
        result.Should().Be("computed-value");

        // Should be present immediately
        var cachedResult = await _cache.GetAsync<string>(key);
        cachedResult.Should().Be("computed-value");

        // Wait for expiration
        await Task.Delay(20);

        // Should be null after expiration
        cachedResult = await _cache.GetAsync<string>(key);
        cachedResult.Should().BeNull();
    }

    /// <summary>
    /// Tests RemoveByPatternAsync removes multiple entries matching a pattern.
    /// </summary>
    [Fact]
    public async Task RemoveByPatternAsync_RemovesMatchingEntries()
    {
        // Arrange - add multiple entries with matching pattern
        await _cache.SetAsync("user:1:profile", "profile1");
        await _cache.SetAsync("user:2:profile", "profile2");
        await _cache.SetAsync("user:3:settings", "settings3");
        await _cache.SetAsync("cache:1:data", "data1");

        // Verify entries exist
        (await _cache.ExistsAsync("user:1:profile")).Should().BeTrue();
        (await _cache.ExistsAsync("user:2:profile")).Should().BeTrue();
        (await _cache.ExistsAsync("user:3:settings")).Should().BeTrue();

        // Act - remove all user:* entries
        await _cache.RemoveByPatternAsync("user:*");

        // Assert - user entries should be gone, others remain
        (await _cache.ExistsAsync("user:1:profile")).Should().BeFalse();
        (await _cache.ExistsAsync("user:2:profile")).Should().BeFalse();
        (await _cache.ExistsAsync("user:3:settings")).Should().BeFalse();
        (await _cache.ExistsAsync("cache:1:data")).Should().BeTrue();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Removed 3 cache entries matching pattern user:*")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests RemoveByPatternAsync with null pattern doesn't throw.
    /// </summary>
    [Fact]
    public void RemoveByPatternAsync_NullPattern_DoesNotThrow()
    {
        // Act - should not throw
        Action act = () => _cache.RemoveByPatternAsync(null!);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests RemoveByPatternAsync with empty pattern doesn't throw.
    /// </summary>
    [Fact]
    public void RemoveByPatternAsync_EmptyPattern_DoesNotThrow()
    {
        // Act - should not throw
        Action act = () => _cache.RemoveByPatternAsync("");
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests concurrent access to the cache from multiple threads.
    /// </summary>
    [Fact]
    public async Task ConcurrentAccess_MultipleThreads_HandlesCorrectly()
    {
        // Arrange
        const int threadCount = 10;
        const int operationsPerThread = 100;
        var tasks = new List<Task>();
        var counter = 0;
        var counterLock = new object();

        // Act - multiple threads setting and getting values concurrently
        for (var i = 0; i < threadCount; i++)
        {
            var threadId = i;
            tasks.Add(Task.Run(async () =>
            {
                for (var j = 0; j < operationsPerThread; j++)
                {
                    var key = $"concurrent-key-{threadId}-{j}";
                    var value = $"value-{threadId}-{j}";

                    await _cache.SetAsync(key, value);
                    var result = await _cache.GetAsync<string>(key);
                    result.Should().Be(value);

                    lock (counterLock)
                    {
                        counter++;
                    }
                }
            }));
        }

        // Wait for all threads to complete
        await Task.WhenAll(tasks);

        // Assert - all operations should have succeeded
        counter.Should().Be(threadCount * operationsPerThread);

        // Verify all entries exist
        for (var i = 0; i < threadCount; i++)
        {
            for (var j = 0; j < operationsPerThread; j++)
            {
                var key = $"concurrent-key-{i}-{j}";
                var exists = await _cache.ExistsAsync(key);
                exists.Should().BeTrue();
            }
        }
    }

    /// <summary>
    /// Tests that Dispose properly cleans up the cleanup timer.
    /// </summary>
    [Fact]
    public void Dispose_CleansUpTimer()
    {
        // Arrange
        var cache = new InMemoryCacheService(_loggerMock.Object);

        // Act
        cache.Dispose();

        // Assert - no exception should be thrown
        // The timer should be disposed
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleared cache")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never); // Timer disposal doesn't log
    }

    /// <summary>
    /// Tests storing and retrieving complex objects.
    /// </summary>
    [Fact]
    public async Task SetAsync_GetAsync_ComplexObject_ReturnsCorrectInstance()
    {
        // Arrange
        const string key = "complex-object-key";
        var complexObject = new TestCacheObject { Id = 1, Name = "Test", Value = 42.5m };

        // Act
        await _cache.SetAsync(key, complexObject);
        var result = await _cache.GetAsync<TestCacheObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(complexObject.Id);
        result.Name.Should().Be(complexObject.Name);
        result.Value.Should().Be(complexObject.Value);
    }

    /// <summary>
    /// Tests storing null values.
    /// </summary>
    [Fact]
    public async Task SetAsync_NullValue_StoresAndRetrievesNull()
    {
        // Arrange
        const string key = "null-value-key";

        // Act - store null
        await _cache.SetAsync(key, (object)null!);
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that cache entries without expiration don't expire.
    /// </summary>
    [Fact]
    public async Task SetAsync_WithoutExpiration_NeverExpires()
    {
        // Arrange
        const string key = "no-expiry-key";
        const string value = "no-expiry-value";

        await _cache.SetAsync(key, value);

        // Wait long enough that any expiration would have triggered
        await Task.Delay(100);

        // Act & Assert - should still be present
        var result = await _cache.GetAsync<string>(key);
        result.Should().Be(value);
    }

    private class TestCacheObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Value { get; set; }
    }
}
