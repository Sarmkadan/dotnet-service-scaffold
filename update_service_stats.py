#!/usr/bin/env python3

# Read the file
with open('/home/redrocket/task-factory/workdir/dotnet-service-scaffold/src/Infrastructure/ServiceDiscovery/ServiceDiscoveryService.cs', 'r') as f:
    content = f.read()

# Update GetServiceStatsAsync method to include stale and evicted counts
old_method = '''        var stats = new ServiceDiscoveryStats(
            ServiceName: serviceName,
            TotalInstances: records.Count,
            HealthyInstances: records.Count(r => r.HealthStatus == DiscoveryHealthStatus.Passing),
            DegradedInstances: records.Count(r => r.HealthStatus == DiscoveryHealthStatus.Warning),
            CriticalInstances: records.Count(r => r.HealthStatus == DiscoveryHealthStatus.Critical),
            LastResolvedAt: meta?.LastResolvedAt,
            CacheExpiresAt: meta?.CacheExpiresAt,
            ActiveSource: meta?.Source ?? DiscoverySource.Unknown);'''

new_method = '''        var stats = new ServiceDiscoveryStats(
            ServiceName: serviceName,
            TotalInstances: records.Count,
            HealthyInstances: records.Count(r => r.HealthStatus == DiscoveryHealthStatus.Passing && !r.IsStale && !r.IsEvicted),
            DegradedInstances: records.Count(r => r.HealthStatus == DiscoveryHealthStatus.Warning && !r.IsStale && !r.IsEvicted),
            CriticalInstances: records.Count(r => r.HealthStatus == DiscoveryHealthStatus.Critical && !r.IsStale && !r.IsEvicted),
            StaleInstances: records.Count(r => r.IsStale && !r.IsEvicted),
            EvictedInstances: records.Count(r => r.IsEvicted),
            LastResolvedAt: meta?.LastResolvedAt,
            CacheExpiresAt: meta?.CacheExpiresAt,
            ActiveSource: meta?.Source ?? DiscoverySource.Unknown);'''

content = content.replace(old_method, new_method)

# Write the updated content
with open('/home/redrocket/task-factory/workdir/dotnet-service-scaffold/src/Infrastructure/ServiceDiscovery/ServiceDiscoveryService.cs', 'w') as f:
    f.write(content)

print("Updated GetServiceStatsAsync method successfully")