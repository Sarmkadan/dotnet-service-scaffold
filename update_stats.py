#!/usr/bin/env python3

import re

# Read the file
with open('/home/redrocket/task-factory/workdir/dotnet-service-scaffold/src/Infrastructure/ServiceDiscovery/IServiceDiscovery.cs', 'r') as f:
    content = f.read()

# Update ServiceDiscoveryStats record definition
old_record = '''public sealed record ServiceDiscoveryStats(
    string ServiceName,
    int TotalInstances,
    int HealthyInstances,
    int DegradedInstances,
    int CriticalInstances,
    DateTime? LastResolvedAt,
    DateTime? CacheExpiresAt,
    DiscoverySource ActiveSource)'''

new_record = '''public sealed record ServiceDiscoveryStats(
    string ServiceName,
    int TotalInstances,
    int HealthyInstances,
    int DegradedInstances,
    int CriticalInstances,
    int StaleInstances,
    int EvictedInstances,
    DateTime? LastResolvedAt,
    DateTime? CacheExpiresAt,
    DiscoverySource ActiveSource)'''

content = content.replace(old_record, new_record)

# Update the XML documentation for ServiceDiscoveryStats
old_params = '''/// <param name="CriticalInstances">Instances in <see cref="DiscoveryHealthStatus.Critical"/> state.</param>
/// <param name="LastResolvedAt">UTC timestamp of the most recent successful resolution.</param>
/// <param name="CacheExpiresAt">UTC timestamp when the current cache entry expires.</param>
/// <param name="ActiveSource">The backend that produced the current cache entry.</param>'''

new_params = '''/// <param name="CriticalInstances">Instances in <see cref="DiscoveryHealthStatus.Critical"/> state.</param>
/// <param name="StaleInstances">Instances that haven't sent heartbeats within the stale threshold.</param>
/// <param name="EvictedInstances">Instances that have been evicted due to prolonged inactivity.</param>
/// <param name="LastResolvedAt">UTC timestamp of the most recent successful resolution.</param>
/// <param name="CacheExpiresAt">UTC timestamp when the current cache entry expires.</param>
/// <param name="ActiveSource">The backend that produced the current cache entry.</param>'''

content = content.replace(old_params, new_params)

# Write the updated content
with open('/home/redrocket/task-factory/workdir/dotnet-service-scaffold/src/Infrastructure/ServiceDiscovery/IServiceDiscovery.cs', 'w') as f:
    f.write(content)

print("Updated IServiceDiscovery.cs successfully")