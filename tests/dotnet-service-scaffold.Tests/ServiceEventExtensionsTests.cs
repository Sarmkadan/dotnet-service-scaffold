using System;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ServiceEventExtensionsTests
{
    [Fact]
    public void IsRecent_ReturnsTrue_WhenWithinTimeSpan()
    {
        var ev = new ServiceEvent { CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
        Assert.True(ev.IsRecent(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public void IsRecent_ReturnsFalse_WhenOutsideTimeSpan()
    {
        var ev = new ServiceEvent { CreatedAt = DateTime.UtcNow.AddMinutes(-15) };
        Assert.False(ev.IsRecent(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public void IsRecent_ThrowsArgumentNullException_WhenEventIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ((ServiceEvent)null!).IsRecent(TimeSpan.FromMinutes(10)));
    }

    [Fact]
    public void IsRecent_ThrowsArgumentOutOfRangeException_WhenTimeSpanIsNegative()
    {
        var ev = new ServiceEvent();
        Assert.Throws<ArgumentOutOfRangeException>(() => ev.IsRecent(TimeSpan.FromMinutes(-1)));
    }

    [Fact]
    public void GetDisplayString_ReturnsFormattedString()
    {
        var ev = new ServiceEvent 
        { 
            CreatedAt = new DateTime(2026, 8, 18, 12, 0, 0), 
            Severity = "Critical",
            EventType = ServiceEventType.ServiceDown,
            Message = "Service stopped unexpectedly"
        };
        var expected = "[2026-08-18 12:00:00] [Critical] Service Stopped: Service stopped unexpectedly";
        Assert.Equal(expected, ev.GetDisplayString());
    }

    [Fact]
    public void GetDisplayString_HandlesNullsCorrectly()
    {
        var ev = new ServiceEvent 
        { 
            CreatedAt = new DateTime(2026, 8, 18, 12, 0, 0),
            EventType = ServiceEventType.ServiceUp
        };
        // Severity is null -> "Info", Message is null -> "No message"
        var expected = "[2026-08-18 12:00:00] [Info] Service Started: No message";
        Assert.Equal(expected, ev.GetDisplayString());
    }

    [Fact]
    public void BelongsToService_ReturnsTrue_WhenIdsMatch()
    {
        var serviceId = Guid.NewGuid();
        var ev = new ServiceEvent { ServiceId = serviceId };
        Assert.True(ev.BelongsToService(serviceId));
    }

    [Fact]
    public void BelongsToService_ReturnsFalse_WhenIdsDoNotMatch()
    {
        var serviceId = Guid.NewGuid();
        var ev = new ServiceEvent { ServiceId = Guid.NewGuid() };
        Assert.False(ev.BelongsToService(serviceId));
    }

    [Fact]
    public void GetPriorityLevel_CalculatesCorrectPriority()
    {
        // Critical + ServiceDown (5 + 3 = 8, capped at 5)
        var ev1 = new ServiceEvent { Severity = "Critical", EventType = ServiceEventType.ServiceDown };
        Assert.Equal(5, ev1.GetPriorityLevel());

        // Info + ServiceUp (1 + 1 = 2)
        var ev2 = new ServiceEvent { Severity = "Info", EventType = ServiceEventType.ServiceUp };
        Assert.Equal(2, ev2.GetPriorityLevel());

        // Warning + HealthCheckFailed (3 + 4 = 7, capped at 5)
        var ev3 = new ServiceEvent { Severity = "Warning", EventType = ServiceEventType.HealthCheckFailed };
        Assert.Equal(5, ev3.GetPriorityLevel());
    }
}
