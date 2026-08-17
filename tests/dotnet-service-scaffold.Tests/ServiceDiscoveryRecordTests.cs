using System;
using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ServiceDiscoveryRecordTests
{
    private ServiceDiscoveryRecord CreateDefaultRecord()
    {
        return new ServiceDiscoveryRecord
        {
            ServiceName = "TestService",
            Host = "localhost",
            Port = 8080,
            Scheme = "http",
            Weight = 10,
            Priority = 0,
            HealthStatus = DiscoveryHealthStatus.Passing,
            Source = DiscoverySource.LocalRegistry
        };
    }

    [Fact]
    public void ToEndpointUri_ReturnsCorrectUri()
    {
        var record = CreateDefaultRecord();
        record.Scheme = "https";
        record.Host = "api.example.com";
        record.Port = 443;

        var uri = record.ToEndpointUri();

        Assert.Equal("https://api.example.com:443", uri);
    }

    [Fact]
    public void IsAlive_ReturnsTrue_WhenHealthyAndRecent()
    {
        var record = CreateDefaultRecord();
        record.IsStale = false;
        record.IsEvicted = false;
        record.HealthStatus = DiscoveryHealthStatus.Passing;
        record.LastSeenAt = DateTime.UtcNow;

        var result = record.IsAlive();

        Assert.True(result);
    }

    [Fact]
    public void IsAlive_ReturnsFalse_WhenStaleFlagSet()
    {
        var record = CreateDefaultRecord();
        record.IsStale = true;

        var result = record.IsAlive();

        Assert.False(result);
    }

    [Fact]
    public void IsAlive_ReturnsFalse_WhenEvictedFlagSet()
    {
        var record = CreateDefaultRecord();
        record.IsEvicted = true;

        var result = record.IsAlive();

        Assert.False(result);
    }

    [Fact]
    public void IsAlive_ReturnsFalse_WhenHealthCritical()
    {
        var record = CreateDefaultRecord();
        record.HealthStatus = DiscoveryHealthStatus.Critical;

        var result = record.IsAlive();

        Assert.False(result);
    }

    [Fact]
    public void IsAlive_ReturnsFalse_WhenLastSeenBeyondThreshold()
    {
        var record = CreateDefaultRecord();
        record.LastSeenAt = DateTime.UtcNow - TimeSpan.FromMinutes(10); // beyond default 5‑minute threshold

        var result = record.IsAlive();

        Assert.False(result);
    }

    [Fact]
    public void RecordHealthy_ResetsFailuresAndUpdatesStatusAndTimestamp()
    {
        var record = CreateDefaultRecord();
        record.ConsecutiveFailures = 5;
        record.HealthStatus = DiscoveryHealthStatus.Warning;
        var before = DateTime.UtcNow;

        record.RecordHealthy();

        Assert.Equal(DiscoveryHealthStatus.Passing, record.HealthStatus);
        Assert.Equal(0, record.ConsecutiveFailures);
        Assert.True(record.LastSeenAt >= before);
    }

    [Fact]
    public void RecordUnhealthy_IncrementsFailures_AndSetsWarning()
    {
        var record = CreateDefaultRecord();
        record.ConsecutiveFailures = 0;
        record.HealthStatus = DiscoveryHealthStatus.Passing;

        record.RecordUnhealthy(); // default criticalThreshold = 3

        Assert.Equal(1, record.ConsecutiveFailures);
        Assert.Equal(DiscoveryHealthStatus.Warning, record.HealthStatus);
    }

    [Fact]
    public void RecordUnhealthy_TriggersCritical_WhenThresholdReached()
    {
        var record = CreateDefaultRecord();
        record.ConsecutiveFailures = 2; // one below default threshold
        record.HealthStatus = DiscoveryHealthStatus.Warning;

        record.RecordUnhealthy(); // 3rd failure -> critical

        Assert.Equal(3, record.ConsecutiveFailures);
        Assert.Equal(DiscoveryHealthStatus.Critical, record.HealthStatus);
    }

    [Fact]
    public void RecordUnhealthy_UsesCustomCriticalThreshold()
    {
        var record = CreateDefaultRecord();
        record.ConsecutiveFailures = 0;

        // Use a custom threshold of 2
        record.RecordUnhealthy(criticalThreshold: 2);

        Assert.Equal(1, record.ConsecutiveFailures);
        Assert.Equal(DiscoveryHealthStatus.Warning, record.HealthStatus);

        record.RecordUnhealthy(criticalThreshold: 2);

        Assert.Equal(2, record.ConsecutiveFailures);
        Assert.Equal(DiscoveryHealthStatus.Critical, record.HealthStatus);
    }
}
