# CacheBenchmarksExtensions

Provides extension methods for querying a collection of `CachedService` instances to retrieve health status summaries, individual lookups, and aggregate counts or percentages. These methods operate on an `IReadOnlyList<CachedService>` and return results asynchronously via `ValueTask`, making them suitable for use in benchmarking or monitoring scenarios where cached service data must be inspected without blocking.

## API

### GetHealthyServicesAsync

```csharp
public static async ValueTask<IReadOnlyList<CachedService>> GetHealthyServicesAsync(
    this IReadOnlyList<CachedService> services)
```

Returns a read-only list containing only those services whose health status evaluates to healthy. The exact definition of "healthy" depends on the `CachedService` implementation. If the source list is empty, the result is an empty list.

### GetUnhealthyServicesAsync

```csharp
public static async ValueTask<IReadOnlyList<CachedService>> GetUnhealthyServicesAsync(
    this IReadOnlyList<CachedService> services)
```

Returns a read-only list containing only those services whose health status evaluates to unhealthy. The exact definition of "unhealthy" depends on the `CachedService` implementation. If the source list is empty, the result is an empty list.

### GetHealthyServiceCountAsync

```csharp
public static async ValueTask<int> GetHealthyServiceCountAsync(
    this IReadOnlyList<CachedService> services)
```

Returns the total number of healthy services in the source list. The count is always non-negative.

### GetUnhealthyServiceCountAsync

```csharp
public static async ValueTask<int> GetUnhealthyServiceCountAsync(
    this IReadOnlyList<CachedService> services)
```

Returns the total number of unhealthy services in the source list. The count is always non-negative.

### GetServiceByIdAsync

```csharp
public static async ValueTask<CachedService?> GetServiceByIdAsync(
    this IReadOnlyList<CachedService> services,
    string id)
```

Looks up a service by its unique identifier. Returns the matching `CachedService` instance, or `null` if no service with the given `id` exists in the list. The `id` parameter is case-sensitive unless the underlying implementation specifies otherwise.

**Throws:** `ArgumentNullException` when `id` is `null`.

### GetServiceByNameAsync

```csharp
public static async ValueTask<CachedService?> GetServiceByNameAsync(
    this IReadOnlyList<CachedService> services,
    string name)
```

Looks up a service by its name. Returns the matching `CachedService` instance, or `null` if no service with the given `name` exists in the list. The `name` parameter is case-sensitive unless the underlying implementation specifies otherwise.

**Throws:** `ArgumentNullException` when `name` is `null`.

### GetHealthyPercentageAsync

```csharp
public static async ValueTask<double> GetHealthyPercentageAsync(
    this IReadOnlyList<CachedService> services)
```

Calculates the percentage of healthy services relative to the total number of services. Returns a value between `0.0` and `100.0`. When the source list is empty, the return value is `0.0`.

### GetUnhealthyPercentageAsync

```csharp
public static async ValueTask<double> GetUnhealthyPercentageAsync(
    this IReadOnlyList<CachedService> services)
```

Calculates the percentage of unhealthy services relative to the total number of services. Returns a value between `0.0` and `100.0`. When the source list is empty, the return value is `0.0`.

## Usage

### Example 1: Monitoring dashboard summary

```csharp
IReadOnlyList<CachedService> allServices = await cache.GetAllServicesAsync();

int healthyCount = await allServices.GetHealthyServiceCountAsync();
int unhealthyCount = await allServices.GetUnhealthyServiceCountAsync();
double healthyPct = await allServices.GetHealthyPercentageAsync();

Console.WriteLine($"Healthy: {healthyCount} ({healthyPct:F1}%)");
Console.WriteLine($"Unhealthy: {unhealthyCount}");
```

### Example 2: Targeted lookup and filtered enumeration

```csharp
IReadOnlyList<CachedService> services = await cache.GetAllServicesAsync();

CachedService? target = await services.GetServiceByNameAsync("auth-service");
if (target is not null)
{
    Console.WriteLine($"Found service: {target.Id}");
}

IReadOnlyList<CachedService> unhealthy = await services.GetUnhealthyServicesAsync();
foreach (var svc in unhealthy)
{
    Console.WriteLine($"Alert: {svc.Name} is unhealthy");
}
```

## Notes

- All methods accept `this IReadOnlyList<CachedService>`, making them callable as extension methods on any read-only list of cached services.
- The source list is not modified by any of these methods; results are derived purely through filtering, counting, or lookup operations.
- Percentage methods return `0.0` for an empty source list to avoid division-by-zero scenarios.
- Lookup methods (`GetServiceByIdAsync`, `GetServiceByNameAsync`) return `null` for missing entries rather than throwing, allowing callers to handle absence gracefully.
- These methods are not thread-safe by themselves. If the underlying `IReadOnlyList<CachedService>` is mutated concurrently (e.g., replaced or modified by another thread while an async operation is in flight), results may be inconsistent. Callers should ensure the source list remains stable for the duration of the query.
- All return types are wrapped in `ValueTask`, enabling efficient asynchronous consumption without heap allocation for synchronously available results.
