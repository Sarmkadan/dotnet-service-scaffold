#!/usr/bin/env python3

# Read the file
with open('/home/redrocket/task-factory/workdir/dotnet-service-scaffold/src/Domain/Models/ServiceDiscoveryRecord.cs', 'r') as f:
    content = f.read()

# Update the IsAlive method
old_method = '''        public bool IsAlive(TimeSpan? staleThreshold = null)
        {
            var threshold = staleThreshold ?? TimeSpan.FromMinutes(5);
            return HealthStatus is not DiscoveryHealthStatus.Critical
                && (DateTime.UtcNow - LastSeenAt) < threshold;
        }'''

new_method = '''        public bool IsAlive(TimeSpan? staleThreshold = null)
        {
            var threshold = staleThreshold ?? TimeSpan.FromMinutes(5);
            var lastActive = LastHeartbeatUtc ?? LastSeenAt;
            return HealthStatus is not DiscoveryHealthStatus.Critical
                && !IsStale
                && !IsEvicted
                && (DateTime.UtcNow - lastActive) < threshold;
        }'''

content = content.replace(old_method, new_method)

# Write the updated content
with open('/home/redrocket/task-factory/workdir/dotnet-service-scaffold/src/Domain/Models/ServiceDiscoveryRecord.cs', 'w') as f:
    f.write(content)

print("Updated IsAlive method successfully")