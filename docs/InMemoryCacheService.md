# InMemoryCacheService

The `InMemoryCacheService` provides a lightweight, transient caching mechanism stored entirely within the application's memory space. Designed for high-speed read/write operations, it supports generic type storage, expiration handling, and pattern-based removal without requiring external infrastructure. This service implements `IDisposable` to manage resource cleanup and exposes metadata regarding cache entry creation and expiration times.

## API

### Constructors

#### `public InMemoryCacheService()`
Initializes a new instance of the `InMemoryCacheService` class. This constructor sets up the internal storage structures required to hold cached items.

### Methods

#### `public ValueTask<T?> GetAsync<T>(string key)`
Retrieves a cached item by its unique key.
*   **Parameters**: `key` (string) – The unique identifier for the cached item.
*   **Returns**: A `ValueTask` containing the cached value of type `T`, or `default` if the key does not exist or has expired.
*   **Throws**: Throws `ArgumentNullException` if `key` is null.

#### `public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null)`
Stores an item in the cache with an optional expiration duration.
*   **Parameters**: 
    *   `key` (string) – The unique identifier for the item.
    *   `value` (T) – The data to store.
    *   `expiration` (TimeSpan?, optional) – The duration after which the item becomes invalid. If null, the item may persist until explicitly removed or the service is cleared.
*   **Returns**: A `ValueTask` that completes when the item is stored.
*   **Throws**: Throws `ArgumentNullException` if `key` is null.

#### `public ValueTask RemoveAsync(string key)`
Removes a specific item from the cache.
*   **Parameters**: `key` (string) – The unique identifier of the item to remove.
*   **Returns**: A `ValueTask` that completes when the removal is processed.
*   **Throws**: Throws `ArgumentNullException` if `key` is null.

#### `public ValueTask<bool> ExistsAsync(string key)`
Checks whether a key exists in the cache and has not expired.
*   **Parameters**: `key` (string) – The unique identifier to check.
*   **Returns**: A `ValueTask<bool>` indicating `true` if the item exists and is valid, otherwise `false`.
*   **Throws**: Throws `ArgumentNullException` if `key` is null.

#### `public async ValueTask<T?> GetOrSetAsync<T>(string key, Func<ValueTask<T>> factory, TimeSpan? expiration = null)`
Retrieves an item if it exists; otherwise, executes a factory function to generate the value, stores it, and returns it.
*   **Parameters**: 
    *   `key` (string) – The unique identifier.
    *   `factory` (Func<ValueTask<T>>) – An asynchronous function to create the value if missing.
    *   `expiration` (TimeSpan?, optional) – Expiration duration for newly created items.
*   **Returns**: A `ValueTask` containing the existing or newly created value of type `T`.
*   **Throws**: Throws `ArgumentNullException` if `key` or `factory` is null.

#### `public ValueTask RemoveByPatternAsync(string pattern)`
Removes all cache entries where the key matches a specified wildcard pattern.
*   **Parameters**: `pattern` (string) – A pattern string (e.g., `user:*`) used to match keys.
*   **Returns**: A `ValueTask` that completes when matching items are removed.
*   **Throws**: Throws `ArgumentNullException` if `pattern` is null.

#### `public ValueTask ClearAsync()`
Removes all items from the cache immediately.
*   **Returns**: A `ValueTask` that completes when the cache is emptied.

#### `public void Dispose()`
Releases unmanaged resources and clears the internal cache storage. This method should be called when the service is no longer needed.

### Properties

#### `public object? Value`
Gets or sets the raw object value associated with the current context or instance scope, depending on implementation specifics. Note that in a multi-key service context, this property typically reflects the value of the most recently accessed or scoped item.

#### `public DateTime CreatedAt`
Gets the timestamp indicating when the current cache instance or the specific tracked entry was initialized.

#### `public DateTime? ExpiresAt`
Gets the specific absolute time at which the current tracked entry will expire. Returns `null` if the entry does not have a fixed expiration time.

## Usage

### Basic Caching with Expiration
The following example demonstrates storing a user profile object with a 10-minute expiration and retrieving it later.

```csharp
using var cache = new InMemoryCacheService();
var userId = "user_123";
var profile = new UserProfile { Id = userId, Name = "Alice" };

// Set the item with a 10-minute expiration
await cache.SetAsync(userId, profile, TimeSpan.FromMinutes(10));

// Retrieve the item
var cachedProfile = await cache.GetAsync<UserProfile>(userId);

if (cachedProfile != null)
{
    Console.WriteLine($"Loaded profile for {cachedProfile.Name}");
}
```

### Get-Or-Set Pattern
This example illustrates the `GetOrSetAsync` method, which avoids race conditions by ensuring the factory function only executes if the key is missing.

```csharp
var cache = new InMemoryCacheService();
var dataKey = "config_settings";

// Define a factory to load data from a slow source if not cached
Func<ValueTask<Settings>> loadSettings = async () => 
{
    await Task.Delay(100); // Simulate I/O
    return new Settings { MaxRetries = 5 };
};

// Atomically get existing value or set new one with 5-minute expiration
var settings = await cache.GetOrSetAsync(
    dataKey, 
    loadSettings, 
    TimeSpan.FromMinutes(5)
);

Console.WriteLine($"Max Retries: {settings.MaxRetries}");
```

## Notes

*   **Thread Safety**: While the use of `ValueTask` suggests optimization for asynchronous flows, the internal implementation of `InMemoryCacheService` relies on standard in-memory collections. Concurrent write operations (e.g., simultaneous `SetAsync` calls for the same key) may result in race conditions unless external synchronization is applied. Read operations (`GetAsync`, `ExistsAsync`) are generally safe but may return stale data if a write occurs simultaneously.
*   **Volatility**: As an in-memory solution, all data is lost upon application restart, process crash, or explicit calls to `Dispose` or `ClearAsync`. It is not suitable for persistent storage requirements.
*   **Expiration Logic**: Expiration is evaluated at retrieval time (`GetAsync`, `ExistsAsync`). An item may physically remain in the internal collection after its `ExpiresAt` time until it is accessed or the cache is cleared.
*   **Pattern Matching**: The `RemoveByPatternAsync` method typically supports simple wildcard matching (e.g., `*`). Complex regex patterns are not guaranteed to be supported and may throw depending on the underlying string matching implementation.
*   **Disposable Lifecycle**: The service implements `IDisposable`. Consumers should ensure the service is disposed of correctly, preferably via a `using` statement, to guarantee immediate memory release of the internal dictionary structures.
