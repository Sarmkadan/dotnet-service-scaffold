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
public class InMemoryCacheServiceTests : IDisposable, IInMemoryCacheServiceTests, IEquatable<InMemoryCacheServiceTests>
{
    private readonly Mock<ILogger<InMemoryCacheService>> _loggerMock;
    private readonly InMemoryCacheService _cache;
    private bool _disposed;

    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Value { get; set; }

    public InMemoryCacheServiceTests()
    {
        _loggerMock = new Mock<ILogger<InMemoryCacheService>>();
        _cache = new InMemoryCacheService(_loggerMock.Object);
    }

    public bool Equals(InMemoryCacheServiceTests? other)
    {
        if (other is null)
            return false;

        return Id == other.Id &&
               Name == other.Name &&
               Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as InMemoryCacheServiceTests);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Value);
    }

    public static bool operator ==(InMemoryCacheServiceTests? left, InMemoryCacheServiceTests? right)
    {
        return EqualityComparer<InMemoryCacheServiceTests>.Default.Equals(left, right);
    }

    public static bool operator !=(InMemoryCacheServiceTests? left, InMemoryCacheServiceTests? right)
    {
        return !(left == right);
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
        _loggerMock.Object.LogInformation("Starting cache set and get test for {CacheKey}", InMemoryCacheServiceTestsConstants.TestKey);

        // Arrange
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.TestKey, InMemoryCacheServiceTestsConstants.TestValue);
        var result = await _cache.GetAsync<string>(InMemoryCacheServiceTestsConstants.TestKey);

        // Assert
        result.Should().Be(InMemoryCacheServiceTestsConstants.TestValue);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(string.Format(InMemoryCacheServiceTestsConstants.LogCachedValueFormat, InMemoryCacheServiceTestsConstants.TestKey))),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Object.LogInformation("Completed cache set and get test for {CacheKey}", InMemoryCacheServiceTestsConstants.TestKey);
    }

    /// <summary>
    /// Tests that GetAsync returns null for non-existent keys.
    /// </summary>
    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        _loggerMock.Object.LogInformation("Starting cache miss test for {CacheKey}", InMemoryCacheServiceTestsConstants.NonExistentKey);
        _loggerMock.Object.LogWarning("Testing fallback behavior for missing cache key {CacheKey}", InMemoryCacheServiceTestsConstants.NonExistentKey);

        // Arrange & Act
        var result = await _cache.GetAsync<string>(InMemoryCacheServiceTestsConstants.NonExistentKey);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(string.Format(InMemoryCacheServiceTestsConstants.LogCacheMissFormat, InMemoryCacheServiceTestsConstants.NonExistentKey))),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Object.LogInformation("Completed cache miss test for {CacheKey}", InMemoryCacheServiceTestsConstants.NonExistentKey);
    }

    /// <summary>
    /// Tests that GetAsync returns null for empty key.
    /// </summary>
    [Fact]
    public async Task GetAsync_EmptyKey_ReturnsNull()
    {
        _loggerMock.Object.LogInformation("Starting cache lookup test for empty key");
        _loggerMock.Object.LogWarning("Testing degraded cache lookup path for empty key");

        // Arrange & Act
        var result = await _cache.GetAsync<string>(string.Empty);

        // Assert
        result.Should().BeNull();

        _loggerMock.Object.LogInformation("Completed cache lookup test for empty key");
    }

    /// <summary>
    /// Tests that GetAsync returns null for null key.
    /// </summary>
    [Fact]
    public async Task GetAsync_NullKey_ReturnsNull()
    {
        _loggerMock.Object.LogInformation("Starting cache lookup test for null key");
        _loggerMock.Object.LogWarning("Testing degraded cache lookup path for null key");

        // Arrange & Act
        var result = await _cache.GetAsync<string>(null!);

        // Assert
        result.Should().BeNull();

        _loggerMock.Object.LogInformation("Completed cache lookup test for null key");
    }

    /// <summary>
    /// Tests that SetAsync throws ArgumentException for null key.
    /// </summary>
    [Fact]
    public void SetAsync_NullKey_ThrowsArgumentException()
    {
        _loggerMock.Object.LogInformation("Starting cache set validation test for null key");
        _loggerMock.Object.LogWarning("Testing rejected cache set path for null key");

        // Arrange
        // Act
        Action act = () => _cache.SetAsync(null!, InMemoryCacheServiceTestsConstants.TestValue);

        // Assert
        act.Should().Throw<ArgumentException>(
            InMemoryCacheServiceTestsConstants.NullKeyExceptionMessage);

        _loggerMock.Object.LogInformation("Completed cache set validation test for null key");
    }

    /// <summary>
    /// Tests that SetAsync throws ArgumentException for empty key.
    /// </summary>
    [Fact]
    public void SetAsync_EmptyKey_ThrowsArgumentException()
    {
        _loggerMock.Object.LogInformation("Starting cache set validation test for empty key");
        _loggerMock.Object.LogWarning("Testing rejected cache set path for empty key");

        // Arrange
        // Act
        Action act = () => _cache.SetAsync(string.Empty, InMemoryCacheServiceTestsConstants.TestValue);

        // Assert
        act.Should().Throw<ArgumentException>(
            InMemoryCacheServiceTestsConstants.EmptyKeyExceptionMessage);

        _loggerMock.Object.LogInformation("Completed cache set validation test for empty key");
    }

    /// <summary>
    /// Tests that values with expiration are automatically removed when expired.
    /// </summary>
    [Fact]
    public async Task SetAsync_WithExpiration_EntryExpiresAndIsRemoved()
    {
        _loggerMock.Object.LogInformation(
            "Starting cache expiration test for {CacheKey} with delay {ExpirationDelayMs}",
            InMemoryCacheServiceTestsConstants.ExpiringKey,
            InMemoryCacheServiceTestsConstants.ExpirationDelayMs);

        // Arrange
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.ExpiringKey, InMemoryCacheServiceTestsConstants.ExpiringValue, TimeSpan.FromMilliseconds(InMemoryCacheServiceTestsConstants.ExpirationDelayMs));

        // Assert - should be present immediately
        var result = await _cache.GetAsync<string>(InMemoryCacheServiceTestsConstants.ExpiringKey);
        result.Should().Be(InMemoryCacheServiceTestsConstants.ExpiringValue);

        // Wait for expiration
        await Task.Delay(InMemoryCacheServiceTestsConstants.ExpirationWaitMs);
        _loggerMock.Object.LogWarning(
            "Cache entry {CacheKey} reached its expiration fallback path after {ExpirationWaitMs}",
            InMemoryCacheServiceTestsConstants.ExpiringKey,
            InMemoryCacheServiceTestsConstants.ExpirationWaitMs);

        // Assert - should be null after expiration
        result = await _cache.GetAsync<string>(InMemoryCacheServiceTestsConstants.ExpiringKey);
        result.Should().BeNull();

        _loggerMock.Object.LogInformation("Completed cache expiration test for {CacheKey}", InMemoryCacheServiceTestsConstants.ExpiringKey);
    }

    /// <summary>
    /// Tests that ExistsAsync returns true for existing keys and false for non-existent keys.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_KeyBehavior_ReturnsCorrectResult()
    {
        _loggerMock.Object.LogInformation(
            "Starting cache existence test for existing key {ExistingKey} and missing key {MissingKey}",
            InMemoryCacheServiceTestsConstants.ExistingKey,
            InMemoryCacheServiceTestsConstants.NonExistentKey);

        // Arrange
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.ExistingKey, InMemoryCacheServiceTestsConstants.TestValue);

        // Act & Assert
        var exists = await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.ExistingKey);
        exists.Should().BeTrue();

        exists = await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.NonExistentKey);
        exists.Should().BeFalse();
        _loggerMock.Object.LogWarning("Cache existence check used missing-key fallback for {CacheKey}", InMemoryCacheServiceTestsConstants.NonExistentKey);

        _loggerMock.Object.LogInformation("Completed cache existence behavior test");
    }

    /// <summary>
    /// Tests that ExistsAsync returns false for expired entries.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ExpiredEntry_ReturnsFalse()
    {
        _loggerMock.Object.LogInformation("Starting expired entry existence test for {CacheKey}", InMemoryCacheServiceTestsConstants.ExpiringKey);

        // Arrange
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.ExpiringKey, InMemoryCacheServiceTestsConstants.TestValue, TimeSpan.FromMilliseconds(InMemoryCacheServiceTestsConstants.ExpirationDelayMs));

        // Wait for expiration
        await Task.Delay(InMemoryCacheServiceTestsConstants.ExpirationWaitMs);
        _loggerMock.Object.LogWarning("Checking degraded path for expired cache key {CacheKey}", InMemoryCacheServiceTestsConstants.ExpiringKey);

        // Act & Assert
        var exists = await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.ExpiringKey);
        exists.Should().BeFalse();

        _loggerMock.Object.LogInformation("Completed expired entry existence test for {CacheKey}", InMemoryCacheServiceTestsConstants.ExpiringKey);
    }

    /// <summary>
    /// Tests that RemoveAsync removes an entry from the cache.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_RemovesEntryFromCache()
    {
        _loggerMock.Object.LogInformation("Starting cache removal test for {CacheKey}", InMemoryCacheServiceTestsConstants.RemovableKey);

        // Arrange
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.RemovableKey, InMemoryCacheServiceTestsConstants.RemovableValue);
        var existsBefore = await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.RemovableKey);
        existsBefore.Should().BeTrue();

        // Act
        await _cache.RemoveAsync(InMemoryCacheServiceTestsConstants.RemovableKey);

        // Assert
        var existsAfter = await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.RemovableKey);
        existsAfter.Should().BeFalse();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(string.Format(InMemoryCacheServiceTestsConstants.LogRemovedCacheEntryFormat, InMemoryCacheServiceTestsConstants.RemovableKey))),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Object.LogInformation("Completed cache removal test for {CacheKey}", InMemoryCacheServiceTestsConstants.RemovableKey);
    }

    /// <summary>
    /// Tests that RemoveAsync with null key doesn't throw.
    /// </summary>
    [Fact]
    public void RemoveAsync_NullKey_DoesNotThrow()
    {
        _loggerMock.Object.LogInformation("Starting cache removal test for null key");
        _loggerMock.Object.LogWarning("Testing no-op cache removal fallback for null key");

        // Act - should not throw
        Action act = () => _cache.RemoveAsync(null!);
        act.Should().NotThrow();

        _loggerMock.Object.LogInformation("Completed cache removal test for null key");
    }

    /// <summary>
    /// Tests that RemoveAsync with empty key doesn't throw.
    /// </summary>
    [Fact]
    public void RemoveAsync_EmptyKey_DoesNotThrow()
    {
        _loggerMock.Object.LogInformation("Starting cache removal test for empty key");
        _loggerMock.Object.LogWarning("Testing no-op cache removal fallback for empty key");

        // Act - should not throw
        Action act = () => _cache.RemoveAsync(string.Empty);
        act.Should().NotThrow();

        _loggerMock.Object.LogInformation("Completed cache removal test for empty key");
    }

    /// <summary>
    /// Tests that ClearAsync removes all entries from the cache.
    /// </summary>
    [Fact]
    public async Task ClearAsync_RemovesAllEntries()
    {
        _loggerMock.Object.LogInformation("Starting cache clear test with {EntryCount} entries", 3);

        // Arrange - add multiple entries
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.Key1, InMemoryCacheServiceTestsConstants.Value1);
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.Key2, InMemoryCacheServiceTestsConstants.Value2);
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.Key3, InMemoryCacheServiceTestsConstants.Value3);

        // Verify entries exist
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.Key1)).Should().BeTrue();
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.Key2)).Should().BeTrue();
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.Key3)).Should().BeTrue();

        // Act
        await _cache.ClearAsync();

        // Assert - all entries should be gone
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.Key1)).Should().BeFalse();
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.Key2)).Should().BeFalse();
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.Key3)).Should().BeFalse();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(InMemoryCacheServiceTestsConstants.LogClearedCache)),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Object.LogInformation("Completed cache clear test with {EntryCount} entries removed", 3);
    }

    /// <summary>
    /// Tests that GetOrSetAsync returns cached value on hit.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_CacheHit_ReturnsCachedValueWithoutCallingFactory()
    {
        _loggerMock.Object.LogInformation("Starting get-or-set cache hit test for {CacheKey}", InMemoryCacheServiceTestsConstants.FactoryKey);

        // Arrange
        var factoryCallCount = 0;

        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.FactoryKey, InMemoryCacheServiceTestsConstants.CachedValue);

        // Act - factory should not be called on cache hit
        var result = await _cache.GetOrSetAsync(
            InMemoryCacheServiceTestsConstants.FactoryKey,
            () =>
            {
                factoryCallCount++;
                return Task.FromResult(InMemoryCacheServiceTestsConstants.NewValue);
            });

        // Assert
        result.Should().Be(InMemoryCacheServiceTestsConstants.CachedValue);
        factoryCallCount.Should().Be(0);

        _loggerMock.Object.LogInformation(
            "Completed get-or-set cache hit test for {CacheKey} with {FactoryCallCount} factory calls",
            InMemoryCacheServiceTestsConstants.FactoryKey,
            factoryCallCount);
    }

    /// <summary>
    /// Tests that GetOrSetAsync calls factory and caches result on cache miss.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_CacheMiss_CallsFactoryAndCachesResult()
    {
        _loggerMock.Object.LogInformation("Starting get-or-set cache miss test for {CacheKey}", InMemoryCacheServiceTestsConstants.FactoryKey);
        _loggerMock.Object.LogWarning("Cache miss for {CacheKey} requires factory fallback", InMemoryCacheServiceTestsConstants.FactoryKey);

        // Arrange
        var factoryCallCount = 0;

        // Act - factory should be called on cache miss
        var result = await _cache.GetOrSetAsync(
            InMemoryCacheServiceTestsConstants.FactoryKey,
            () =>
            {
                factoryCallCount++;
                return Task.FromResult(InMemoryCacheServiceTestsConstants.ComputedValue);
            });

        // Assert
        result.Should().Be(InMemoryCacheServiceTestsConstants.ComputedValue);
        factoryCallCount.Should().Be(1);

        // Verify it was cached
        var cachedResult = await _cache.GetAsync<string>(InMemoryCacheServiceTestsConstants.FactoryKey);
        cachedResult.Should().Be(InMemoryCacheServiceTestsConstants.ComputedValue);

        _loggerMock.Object.LogInformation(
            "Completed get-or-set cache miss test for {CacheKey} with {FactoryCallCount} factory calls",
            InMemoryCacheServiceTestsConstants.FactoryKey,
            factoryCallCount);
    }

    /// <summary>
    /// Tests that GetOrSetAsync with expiration caches the result with expiration.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_WithExpiration_CachesWithExpiration()
    {
        _loggerMock.Object.LogInformation(
            "Starting expiring get-or-set test for {CacheKey} with delay {ExpirationDelayMs}",
            InMemoryCacheServiceTestsConstants.ExpiringFactoryKey,
            InMemoryCacheServiceTestsConstants.ExpirationDelayMs);

        // Arrange
        // Act
        var result = await _cache.GetOrSetAsync(
            InMemoryCacheServiceTestsConstants.ExpiringFactoryKey,
            () => Task.FromResult(InMemoryCacheServiceTestsConstants.ComputedValue),
            TimeSpan.FromMilliseconds(InMemoryCacheServiceTestsConstants.ExpirationDelayMs));

        // Assert
        result.Should().Be(InMemoryCacheServiceTestsConstants.ComputedValue);

        // Should be present immediately
        var cachedResult = await _cache.GetAsync<string>(InMemoryCacheServiceTestsConstants.ExpiringFactoryKey);
        cachedResult.Should().Be(InMemoryCacheServiceTestsConstants.ComputedValue);

        // Wait for expiration
        await Task.Delay(InMemoryCacheServiceTestsConstants.ExpirationWaitMs);
        _loggerMock.Object.LogWarning("Testing expired get-or-set fallback for {CacheKey}", InMemoryCacheServiceTestsConstants.ExpiringFactoryKey);

        // Should be null after expiration
        cachedResult = await _cache.GetAsync<string>(InMemoryCacheServiceTestsConstants.ExpiringFactoryKey);
        cachedResult.Should().BeNull();

        _loggerMock.Object.LogInformation("Completed expiring get-or-set test for {CacheKey}", InMemoryCacheServiceTestsConstants.ExpiringFactoryKey);
    }

    /// <summary>
    /// Tests RemoveByPatternAsync removes multiple entries matching a pattern.
    /// </summary>
    [Fact]
    public async Task RemoveByPatternAsync_RemovesMatchingEntries()
    {
        _loggerMock.Object.LogInformation("Starting pattern removal test for {CachePattern}", InMemoryCacheServiceTestsConstants.UserPattern);

        // Arrange - add multiple entries with matching pattern
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.User1Profile, InMemoryCacheServiceTestsConstants.Profile1);
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.User2Profile, InMemoryCacheServiceTestsConstants.Profile2);
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.User3Settings, InMemoryCacheServiceTestsConstants.Settings3);
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.Cache1Data, InMemoryCacheServiceTestsConstants.Data1);

        // Verify entries exist
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.User1Profile)).Should().BeTrue();
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.User2Profile)).Should().BeTrue();
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.User3Settings)).Should().BeTrue();

        // Act - remove all user:* entries
        await _cache.RemoveByPatternAsync(InMemoryCacheServiceTestsConstants.UserPattern);

        // Assert - user entries should be gone, others remain
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.User1Profile)).Should().BeFalse();
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.User2Profile)).Should().BeFalse();
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.User3Settings)).Should().BeFalse();
        (await _cache.ExistsAsync(InMemoryCacheServiceTestsConstants.Cache1Data)).Should().BeTrue();

        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(string.Format(InMemoryCacheServiceTestsConstants.LogRemovedPatternFormat, InMemoryCacheServiceTestsConstants.RemovedPatternEntryCount, InMemoryCacheServiceTestsConstants.UserPattern))),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Object.LogInformation(
            "Completed pattern removal test for {CachePattern} with {RemovedEntryCount} entries removed",
            InMemoryCacheServiceTestsConstants.UserPattern,
            InMemoryCacheServiceTestsConstants.RemovedPatternEntryCount);
    }

    /// <summary>
    /// Tests RemoveByPatternAsync with null pattern doesn't throw.
    /// </summary>
    [Fact]
    public void RemoveByPatternAsync_NullPattern_DoesNotThrow()
    {
        _loggerMock.Object.LogInformation("Starting pattern removal test for null pattern");
        _loggerMock.Object.LogWarning("Testing no-op pattern removal fallback for null pattern");

        // Act - should not throw
        Action act = () => _cache.RemoveByPatternAsync(null!);
        act.Should().NotThrow();

        _loggerMock.Object.LogInformation("Completed pattern removal test for null pattern");
    }

    /// <summary>
    /// Tests RemoveByPatternAsync with empty pattern doesn't throw.
    /// </summary>
    [Fact]
    public void RemoveByPatternAsync_EmptyPattern_DoesNotThrow()
    {
        _loggerMock.Object.LogInformation("Starting pattern removal test for empty pattern");
        _loggerMock.Object.LogWarning("Testing no-op pattern removal fallback for empty pattern");

        // Act - should not throw
        Action act = () => _cache.RemoveByPatternAsync(string.Empty);
        act.Should().NotThrow();

        _loggerMock.Object.LogInformation("Completed pattern removal test for empty pattern");
    }

    /// <summary>
    /// Tests concurrent access to the cache from multiple threads.
    /// </summary>
    [Fact]
    public async Task ConcurrentAccess_MultipleThreads_HandlesCorrectly()
    {
        _loggerMock.Object.LogInformation(
            "Starting concurrent cache access test with {ThreadCount} threads and {OperationsPerThread} operations per thread",
            InMemoryCacheServiceTestsConstants.ConcurrentThreadCount,
            InMemoryCacheServiceTestsConstants.ConcurrentOperationsPerThread);

        // Arrange
        var tasks = new List<Task>();
        var counter = 0;
        var counterLock = new object();

        // Act - multiple threads setting and getting values concurrently
        for (var i = 0; i < InMemoryCacheServiceTestsConstants.ConcurrentThreadCount; i++)
        {
            var threadId = i;
            tasks.Add(Task.Run(async () =>
            {
                for (var j = 0; j < InMemoryCacheServiceTestsConstants.ConcurrentOperationsPerThread; j++)
                {
                    var key = string.Format(InMemoryCacheServiceTestsConstants.ConcurrentKeyFormat, threadId, j);
                    var value = string.Format(InMemoryCacheServiceTestsConstants.ConcurrentValueFormat, threadId, j);

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
        counter.Should().Be(InMemoryCacheServiceTestsConstants.ConcurrentThreadCount * InMemoryCacheServiceTestsConstants.ConcurrentOperationsPerThread);

        // Verify all entries exist
        for (var i = 0; i < InMemoryCacheServiceTestsConstants.ConcurrentThreadCount; i++)
        {
            for (var j = 0; j < InMemoryCacheServiceTestsConstants.ConcurrentOperationsPerThread; j++)
            {
                var key = string.Format(InMemoryCacheServiceTestsConstants.ConcurrentKeyFormat, i, j);
                var exists = await _cache.ExistsAsync(key);
                exists.Should().BeTrue();
            }
        }

        _loggerMock.Object.LogInformation(
            "Completed concurrent cache access test with {CompletedOperationCount} operations",
            counter);
    }

    /// <summary>
    /// Tests that Dispose properly cleans up the cleanup timer.
    /// </summary>
    [Fact]
    public void Dispose_CleansUpTimer()
    {
        _loggerMock.Object.LogInformation("Starting cache disposal test");

        // Arrange
        var cache = new InMemoryCacheService(_loggerMock.Object);

        // Act
        cache.Dispose();

        // Assert - no exception should be thrown
        // The timer should be disposed
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(InMemoryCacheServiceTestsConstants.LogClearedCache)),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never); // Timer disposal doesn't log

        _loggerMock.Object.LogInformation("Completed cache disposal test");
    }

    /// <summary>
    /// Tests storing and retrieving complex objects.
    /// </summary>
    [Fact]
    public async Task SetAsync_GetAsync_ComplexObject_ReturnsCorrectInstance()
    {
        _loggerMock.Object.LogInformation(
            "Starting complex object cache test for {CacheKey} and object {ObjectId}",
            InMemoryCacheServiceTestsConstants.ComplexObjectKey,
            InMemoryCacheServiceTestsConstants.ComplexObjectId);

        // Arrange
        var complexObject = new TestCacheObject
        {
            Id = InMemoryCacheServiceTestsConstants.ComplexObjectId,
            Name = InMemoryCacheServiceTestsConstants.ComplexObjectName,
            Value = InMemoryCacheServiceTestsConstants.ComplexObjectValue
        };

        // Act
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.ComplexObjectKey, complexObject);
        var result = await _cache.GetAsync<TestCacheObject>(InMemoryCacheServiceTestsConstants.ComplexObjectKey);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(complexObject.Id);
        result.Name.Should().Be(complexObject.Name);
        result.Value.Should().Be(complexObject.Value);

        _loggerMock.Object.LogInformation(
            "Completed complex object cache test for {CacheKey} and object {ObjectId}",
            InMemoryCacheServiceTestsConstants.ComplexObjectKey,
            complexObject.Id);
    }

    /// <summary>
    /// Tests storing null values.
    /// </summary>
    [Fact]
    public async Task SetAsync_NullValue_StoresAndRetrievesNull()
    {
        _loggerMock.Object.LogInformation("Starting null value cache test for {CacheKey}", InMemoryCacheServiceTestsConstants.NullValueKey);
        _loggerMock.Object.LogWarning("Testing null value cache fallback for {CacheKey}", InMemoryCacheServiceTestsConstants.NullValueKey);

        // Arrange
        // Act - store null
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.NullValueKey, (object)null!);
        var result = await _cache.GetAsync<string>(InMemoryCacheServiceTestsConstants.NullValueKey);

        // Assert
        result.Should().BeNull();

        _loggerMock.Object.LogInformation("Completed null value cache test for {CacheKey}", InMemoryCacheServiceTestsConstants.NullValueKey);
    }

    /// <summary>
    /// Tests that cache entries without expiration don't expire.
    /// </summary>
    [Fact]
    public async Task SetAsync_WithoutExpiration_NeverExpires()
    {
        _loggerMock.Object.LogInformation(
            "Starting non-expiring cache test for {CacheKey} with wait {WaitMs}",
            InMemoryCacheServiceTestsConstants.NoExpiryKey,
            InMemoryCacheServiceTestsConstants.NoExpiryWaitMs);

        // Arrange
        await _cache.SetAsync(InMemoryCacheServiceTestsConstants.NoExpiryKey, InMemoryCacheServiceTestsConstants.NoExpiryValue);

        // Wait long enough that any expiration would have triggered
        await Task.Delay(InMemoryCacheServiceTestsConstants.NoExpiryWaitMs);

        // Act & Assert - should still be present
        var result = await _cache.GetAsync<string>(InMemoryCacheServiceTestsConstants.NoExpiryKey);
        result.Should().Be(InMemoryCacheServiceTestsConstants.NoExpiryValue);

        _loggerMock.Object.LogInformation("Completed non-expiring cache test for {CacheKey}", InMemoryCacheServiceTestsConstants.NoExpiryKey);
    }

    private class TestCacheObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Value { get; set; }
    }
}
