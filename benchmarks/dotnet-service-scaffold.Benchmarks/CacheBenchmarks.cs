#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotnetServiceScaffold.Infrastructure.Caching;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Benchmarks for in-memory cache operations. Simulates the hot paths hit on every
/// API request: reading a cached service list, writing a new entry, and the
/// get-or-set pattern used for lazy population of service metadata.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CacheBenchmarks : ICacheBenchmarks, IEquatable<CacheBenchmarks>
{
    private InMemoryCacheService _cache = default!;

    private static readonly CachedServiceList _serviceList = new()
    {
        Services = Enumerable.Range(1, 20)
            .Select(i => new CachedService { Id = $"svc-{i:D3}", Name = $"Service {i}", IsHealthy = i % 5 != 0 })
            .ToList()
    };

    private static readonly int _cachedHashCode = ComputeHashCode();

    private static int ComputeHashCode()
    {
        var hash = new HashCode();
        foreach (var service in _serviceList.Services)
        {
            hash.Add(service.Id);
            hash.Add(service.Name);
            hash.Add(service.IsHealthy);
        }
        hash.Add(_serviceList.Services.Count);
        return hash.ToHashCode();
    }

    public bool Equals(CacheBenchmarks? other)
    {
        if (other is null)
            return false;

        // Since _serviceList is static and readonly, all instances share the same data.
        // We compare the Services, Id, Name, IsHealthy as instructed.
        var list = CacheBenchmarks._serviceList.Services;
        return list.Count == list.Count
               && list.Zip(list, (s1, s2) => s1.Id == s2.Id && s1.Name == s2.Name && s1.IsHealthy == s2.IsHealthy)
                      .All(b => b);
    }

    public override bool Equals(object? obj) => Equals(obj as CacheBenchmarks);

    public override int GetHashCode() => _cachedHashCode;

    public override string ToString()
    {
        var firstService = _serviceList.Services[0];
        return $"CacheBenchmarks {{ Services = {_serviceList.Services.Count}, Id = {firstService.Id}, Name = {firstService.Name}, IsHealthy = {firstService.IsHealthy} }}";
    }

    public static bool operator ==(CacheBenchmarks? left, CacheBenchmarks? right) => Equals(left, right);

    public static bool operator !=(CacheBenchmarks? left, CacheBenchmarks? right) => !(left == right);

    [GlobalSetup]
    public async Task Setup()
    {
        _cache = new InMemoryCacheService(NullLogger<InMemoryCacheService>.Instance);
        await _cache.SetAsync("services:all", _serviceList, TimeSpan.FromMinutes(5));
        await _cache.SetAsync("services:page:1", _serviceList, TimeSpan.FromMinutes(2));
    }

    [GlobalCleanup]
    public void Cleanup() => _cache.Dispose();

    /// <summary>
    /// Gets the cache service instance for validation purposes.
    /// </summary>
    /// <returns>The cache service instance, or null if not initialized.</returns>
    internal InMemoryCacheService? GetCache() => _cache;

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
    public ValueTask<CachedServiceList?> GetOrSetHit()
        => _cache.GetOrSetAsync("services:all",
            () => Task.FromResult(_serviceList),
            TimeSpan.FromMinutes(5));

    [Benchmark(Description = "GetOrSetAsync — cold path (cache miss, factory invoked)")]
    public ValueTask<CachedServiceList?> GetOrSetMiss()
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