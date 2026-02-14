#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotnetServiceScaffold.Infrastructure.Caching;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Benchmarks for in-memory cache operations. Simulates the hot paths hit on every
/// API request: reading a cached service list, writing a new entry, and the
/// get-or-set pattern used for lazy population of service metadata.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CacheBenchmarks
{
    private InMemoryCacheService _cache = default!;

    private static readonly CachedServiceList _serviceList = new()
    {
        Services = Enumerable.Range(1, 20)
            .Select(i => new CachedService { Id = $"svc-{i:D3}", Name = $"Service {i}", IsHealthy = i % 5 != 0 })
            .ToList()
    };

    [GlobalSetup]
    public async Task Setup()
    {
        _cache = new InMemoryCacheService(NullLogger<InMemoryCacheService>.Instance);
        await _cache.SetAsync("services:all", _serviceList, TimeSpan.FromMinutes(5));
        await _cache.SetAsync("services:page:1", _serviceList, TimeSpan.FromMinutes(2));
    }

    [GlobalCleanup]
    public void Cleanup() => _cache.Dispose();

    [Benchmark(Description = "GetAsync — cache hit (warm key)")]
    public ValueTask<CachedServiceList?> CacheHit()
        => _cache.GetAsync<CachedServiceList>("services:all");

    [Benchmark(Description = "GetAsync — cache miss (cold key)")]
    public ValueTask<CachedServiceList?> CacheMiss()
        => _cache.GetAsync<CachedServiceList>("services:nonexistent");

    [Benchmark(Description = "SetAsync — write with 5-min TTL")]
    public ValueTask CacheSet()
        => _cache.SetAsync("services:write", _serviceList, TimeSpan.FromMinutes(5));

    [Benchmark(Description = "ExistsAsync — key presence check")]
    public ValueTask<bool> Exists()
        => _cache.ExistsAsync("services:all");

    [Benchmark(Description = "GetOrSetAsync — hot path (cache hit, no factory call)")]
    public ValueTask<CachedServiceList> GetOrSetHit()
        => _cache.GetOrSetAsync("services:all",
            () => Task.FromResult(_serviceList),
            TimeSpan.FromMinutes(5));

    [Benchmark(Description = "GetOrSetAsync — cold path (cache miss, factory invoked)")]
    public ValueTask<CachedServiceList> GetOrSetMiss()
        => _cache.GetOrSetAsync($"services:cold:{Guid.NewGuid()}",
            () => Task.FromResult(_serviceList),
            TimeSpan.FromMinutes(5));
}

public sealed class CachedServiceList
{
    public List<CachedService> Services { get; set; } = [];
}

public sealed class CachedService
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsHealthy { get; set; }
}
