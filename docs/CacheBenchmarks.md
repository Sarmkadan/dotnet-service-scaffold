# CacheBenchmarks

`CacheBenchmarks` is a benchmarking utility class designed to measure the performance and behavior of caching operations within the `dotnet-service-scaffold` project. It simulates cache hits, misses, and other common caching scenarios to evaluate the efficiency and correctness of the underlying cache implementation. The class provides methods to set up and clean up test data, as well as to execute benchmarked operations such as retrieving cached items, checking for existence, and conditionally setting values.

## API

### `public async Task Setup`
Initializes the benchmark environment by preparing test data and ensuring the cache is in a known state. This method should be called before executing any benchmark operations to avoid skewed results.

- **Parameters**: None.
- **Return Value**: `Task` representing the asynchronous operation.
- **Throws**: May throw exceptions if the setup process fails (e.g., data initialization errors).

---

### `public void Cleanup`
Resets or clears the benchmark environment, removing any test data or cached items created during benchmark execution. This method should be called after benchmark operations to ensure a clean state for subsequent runs.

- **Parameters**: None.
- **Return Value**: Void.
- **Throws**: May throw exceptions if cleanup operations fail (e.g., cache eviction errors).

---

### `public ValueTask<CachedServiceList?> CacheHit`
Simulates a cache hit scenario by retrieving a pre-existing item from the cache. This method is used to benchmark the performance of successful cache retrievals.

- **Parameters**: None.
- **Return Value**: `ValueTask<CachedServiceList?>` containing the cached item if found, or `null` if the item does not exist.
- **Throws**: May throw exceptions if the cache operation fails (e.g., cache unavailability).

---

### `public ValueTask<CachedServiceList?> CacheMiss`
Simulates a cache miss scenario by attempting to retrieve a non-existent item from the cache. This method is used to benchmark the performance of failed cache retrievals.

- **Parameters**: None.
- **Return Value**: `ValueTask<CachedServiceList?>` returning `null` as the item is not present.
- **Throws**: May throw exceptions if the cache operation fails (e.g., cache unavailability).

---

### `public ValueTask CacheSet`
Stores a predefined item in the cache. This method is used to benchmark the performance of cache insertion operations.

- **Parameters**: None.
- **Return Value**: `ValueTask` representing the asynchronous operation.
- **Throws**: May throw exceptions if the cache insertion fails (e.g., invalid data or cache unavailability).

---

### `public ValueTask<bool> Exists`
Checks whether a predefined item exists in the cache. This method is used to benchmark the performance of cache existence checks.

- **Parameters**: None.
- **Return Value**: `ValueTask<bool>` returning `true` if the item exists, otherwise `false`.
- **Throws**: May throw exceptions if the cache operation fails (e.g., cache unavailability).

---

### `public ValueTask<CachedServiceList?> GetOrSetHit`
Retrieves a pre-existing item from the cache or sets it if it does not exist. This method simulates a "get-or-set" operation where the item is already cached, benchmarking the performance of successful retrievals.

- **Parameters**: None.
- **Return Value**: `ValueTask<CachedServiceList?>` containing the cached item.
- **Throws**: May throw exceptions if the cache operation fails (e.g., cache unavailability or data validation errors).

---

### `public ValueTask<CachedServiceList?> GetOrSetMiss`
Attempts to retrieve a non-existent item from the cache and sets it if missing. This method simulates a "get-or-set" operation where the item is not initially cached, benchmarking the performance of conditional insertion and retrieval.

- **Parameters**: None.
- **Return Value**: `ValueTask<CachedServiceList?>` containing the newly cached item.
- **Throws**: May throw exceptions if the cache operation fails (e.g., cache unavailability or data validation errors).

---

### `public List<CachedService> Services`
A collection of `CachedService` objects used as test data for benchmarking operations. This property is populated during setup and may be modified during benchmark execution.

- **Parameters**: None.
- **Return Value**: `List<CachedService>` containing the test data.

---

### `public string Id`
A unique identifier for the benchmark instance, used to distinguish between different benchmark runs or configurations.

- **Parameters**: None.
- **Return Value**: `string` representing the identifier.

---

### `public string Name`
A human-readable name for the benchmark instance, describing its purpose or configuration.

- **Parameters**: None.
- **Return Value**: `string` representing the name.

---

### `public bool IsHealthy`
Indicates whether the benchmark instance is in a valid state for execution. This property may be used to skip or abort benchmark runs if the environment is misconfigured.

- **Parameters**: None.
- **Return Value**: `bool` returning `true` if the benchmark is healthy, otherwise `false`.
