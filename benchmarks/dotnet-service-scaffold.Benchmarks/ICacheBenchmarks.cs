#nullable enable

using System.Threading.Tasks;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Interface for cache benchmarks.
/// </summary>
public interface ICacheBenchmarks
{
    Task Setup();
    void Cleanup();
    ValueTask<CachedServiceList?> CacheHit();
    ValueTask<CachedServiceList?> CacheMiss();
    ValueTask CacheSet();
    ValueTask<bool> Exists();
    ValueTask<CachedServiceList?> GetOrSetHit();
    ValueTask<CachedServiceList?> GetOrSetMiss();
}