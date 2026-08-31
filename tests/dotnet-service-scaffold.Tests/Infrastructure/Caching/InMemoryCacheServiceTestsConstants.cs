#nullable enable

namespace DotnetServiceScaffold.Tests.Infrastructure.Caching;

/// <summary>
/// Constants for InMemoryCacheServiceTests to avoid magic strings and numbers.
/// </summary>
internal static class InMemoryCacheServiceTestsConstants
{
    // Common test keys
    public const string TestKey = "test-key";
    public const string NonExistentKey = "non-existent-key";
    public const string ExpiringKey = "expiring-key";
    public const string ExistingKey = "existing-key";
    public const string RemovableKey = "removable-key";
    public const string FactoryKey = "factory-key";
    public const string ExpiringFactoryKey = "expiring-factory-key";
    public const string ComplexObjectKey = "complex-object-key";
    public const string NullValueKey = "null-value-key";
    public const string NoExpiryKey = "no-expiry-key";

    // Common test values
    public const string TestValue = "test-value";
    public const string ExpiringValue = "expiring-value";
    public const string RemovableValue = "removable-value";
    public const string CachedValue = "cached-value";
    public const string NewValue = "new-value";
    public const string ComputedValue = "computed-value";
    public const string NoExpiryValue = "no-expiry-value";
    public const string Profile1 = "profile1";
    public const string Profile2 = "profile2";
    public const string Settings3 = "settings3";
    public const string Data1 = "data1";
    public const string ComplexObjectName = "Test";

    // Test data for multiple entries
    public const string Key1 = "key1";
    public const string Value1 = "value1";
    public const string Key2 = "key2";
    public const string Value2 = "value2";
    public const string Key3 = "key3";
    public const string Value3 = "value3";

    // Pattern test data
    public const string User1Profile = "user:1:profile";
    public const string User2Profile = "user:2:profile";
    public const string User3Settings = "user:3:settings";
    public const string Cache1Data = "cache:1:data";
    public const string UserPattern = "user:*";
    public const string ConcurrentKeyFormat = "concurrent-key-{0}-{1}";
    public const string ConcurrentValueFormat = "value-{0}-{1}";

    // Log message templates
    public const string LogCachedValueFormat = "Cached value for key {0}";
    public const string LogCacheMissFormat = "Cache miss for key {0}";
    public const string LogRemovedCacheEntryFormat = "Removed cache entry for key {0}";
    public const string LogClearedCache = "Cleared cache";
    public const string LogRemovedPatternFormat = "Removed {0} cache entries matching pattern {1}";

    // Timeouts and delays
    public const short ExpirationDelayMs = 10;
    public const short ExpirationWaitMs = 20;
    public const int NoExpiryWaitMs = 100;

    // Concurrent test constants
    public const int ConcurrentThreadCount = 10;
    public const int ConcurrentOperationsPerThread = 100;
    public const int RemovedPatternEntryCount = 3;

    // Complex object test data
    public const int ComplexObjectId = 1;
    public const decimal ComplexObjectValue = 42.5m;

    // Exception messages
    public const string NullKeyExceptionMessage = "because null key is not allowed";
    public const string EmptyKeyExceptionMessage = "because empty key is not allowed";
}
