#!/usr/bin/env python3

# Read the file
with open('/home/redrocket/task-factory/workdir/dotnet-service-scaffold/src/Domain/Models/ServiceDiscoveryRecord.cs', 'r') as f:
    content = f.read()

# Add LastHeartbeatUtc, IsStale, and IsEvicted properties after LastSeenAt
old_section = '''        /// <summary>Gets or sets the UTC timestamp of the last successful health confirmation.</summary>
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the DNS time-to-live in seconds if this record originated from a DNS lookup.
        /// <see langword="null"/> for registry-sourced records.
        /// </summary>
        public int? DnsTtlSeconds { get; set; }'''

new_section = '''        /// <summary>Gets or sets the UTC timestamp of the last successful health confirmation.</summary>
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the UTC timestamp of the last heartbeat received from this instance.
        /// Used for staleness detection and eviction.
        /// </summary>
        public DateTime? LastHeartbeatUtc { get; set; }

        /// <summary>
        /// Gets or sets whether this instance has been marked as stale due to missed heartbeats.
        /// </summary>
        public bool IsStale { get; set; }

        /// <summary>
        /// Gets or sets whether this instance has been evicted from the registry.
        /// </summary>
        public bool IsEvicted { get; set; }

        /// <summary>
        /// Gets or sets the DNS time-to-live in seconds if this record originated from a DNS lookup.
        /// <see langword="null"/> for registry-sourced records.
        /// </summary>
        public int? DnsTtlSeconds { get; set; }'''

content = content.replace(old_section, new_section)

# Write the updated content
with open('/home/redrocket/task-factory/workdir/dotnet-service-scaffold/src/Domain/Models/ServiceDiscoveryRecord.cs', 'w') as f:
    f.write(content)

print("Updated ServiceDiscoveryRecord.cs successfully")