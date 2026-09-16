#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Caching;

internal static class InMemoryCacheServiceConstants
{
    public const int CleanupIntervalSeconds = 60;
    public const double RegexTimeoutSeconds = 1.0;
    public const double NoExpirationSeconds = -1;
    public const int NoExpiredEntries = 0;

    public const string GettingCacheEntryLogMessage = "Getting cache entry for key {Key}";
    public const string CacheEntryExpiredLogMessage = "Cache entry for key {Key} has expired";
    public const string CacheHitLogMessage = "Cache hit for key {Key}";
    public const string CacheMissLogMessage = "Cache miss for key {Key}";
    public const string SettingCacheEntryLogMessage = "Setting cache entry for key {Key}";
    public const string CachedValueLogMessage = "Cached value for key {Key} with expiration {ExpirationSeconds}s";
    public const string RemovingCacheEntryLogMessage = "Removing cache entry for key {Key}";
    public const string RemovedCacheEntryLogMessage = "Removed cache entry for key {Key}";
    public const string CheckingCacheEntryExistenceLogMessage = "Checking existence of cache entry for key {Key}";
    public const string CacheEntryExistsLogMessage = "Cache entry for key {Key} exists";
    public const string CacheEntryDoesNotExistLogMessage = "Cache entry for key {Key} does not exist";
    public const string GettingOrSettingCacheEntryLogMessage = "Getting or setting cache entry for key {Key}";
    public const string GetOrSetCacheHitLogMessage = "Cache hit for key {Key} in GetOrSetAsync";
    public const string GetOrSetCacheMissLogMessage = "Cache miss for key {Key} in GetOrSetAsync, invoking factory";
    public const string GetOrSetValueSetLogMessage = "Value set for key {Key} in GetOrSetAsync";
    public const string GetOrSetFactoryReturnedNullLogMessage = "Factory returned null for key {Key} in GetOrSetAsync";
    public const string RemovedMatchingCacheEntriesLogMessage = "Removed {Count} cache entries matching pattern {Pattern}";
    public const string RemoveByPatternErrorLogMessage = "Error removing cache entries by pattern {Pattern}";
    public const string ClearedCacheLogMessage = "Cleared cache ({Count} entries removed)";
    public const string CleanedUpExpiredEntriesLogMessage = "Cleaned up {Count} expired cache entries";
    public const string CacheServiceDisplayFormat = "InMemoryCacheService {{ CacheCount = {0} }}";
    public const string CacheEntryDisplayFormat = "CacheEntry {{ Value = {0}, CreatedAt = {1}, ExpiresAt = {2} }}";
}
