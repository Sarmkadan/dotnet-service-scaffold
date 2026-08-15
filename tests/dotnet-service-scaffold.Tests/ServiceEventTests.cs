using System;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ServiceEventTests
{
    [Fact]
    public void IsAlertWorthy_ReturnsTrue_WhenSeverityIsCritical()
    {
        var ev = new ServiceEvent
        {
            Severity = "Critical",
            EventType = ServiceEventType.ServiceUp
        };

        Assert.True(ev.IsAlertWorthy());
    }

    [Fact]
    public void IsAlertWorthy_ReturnsTrue_WhenEventTypeIsServiceDown()
    {
        var ev = new ServiceEvent
        {
            Severity = "Info",
            EventType = ServiceEventType.ServiceDown
        };

        Assert.True(ev.IsAlertWorthy());
    }

    [Fact]
    public void IsAlertWorthy_ReturnsTrue_WhenEventTypeIsHealthCheckFailed()
    {
        var ev = new ServiceEvent
        {
            Severity = "Warning",
            EventType = ServiceEventType.HealthCheckFailed
        };

        Assert.True(ev.IsAlertWorthy());
    }

    [Fact]
    public void IsAlertWorthy_ReturnsFalse_ForNonAlertConditions()
    {
        var ev = new ServiceEvent
        {
            Severity = "Info",
            EventType = ServiceEventType.ServiceUp
        };

        Assert.False(ev.IsAlertWorthy());
    }

    [Fact]
    public void Acknowledge_SetsAcknowledgedAt_And_AcknowledgedBy()
    {
        var ev = new ServiceEvent();

        ev.Acknowledge();

        Assert.True(ev.AcknowledgedAt);
        Assert.NotNull(ev.AcknowledgedBy);
        // The acknowledged time should be recent (within the last second)
        Assert.InRange(ev.AcknowledgedBy.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow);
    }

    [Theory]
    [InlineData(ServiceEventType.ServiceUp, "Service Started")]
    [InlineData(ServiceEventType.ServiceDown, "Service Stopped")]
    [InlineData(ServiceEventType.ServiceRestarted, "Service Restarted")]
    [InlineData(ServiceEventType.HealthCheckFailed, "Health Check Failed")]
    [InlineData(ServiceEventType.HealthCheckPassed, "Health Check Passed")]
    [InlineData(ServiceEventType.ConfigurationChanged, "Configuration Updated")]
    [InlineData(ServiceEventType.ServiceDisabled, "Service Disabled")]
    [InlineData(ServiceEventType.ServiceEnabled, "Service Enabled")]
    [InlineData(ServiceEventType.ErrorOccurred, "Error Occurred")]
    [InlineData(ServiceEventType.DeploymentStarted, "Deployment Started")]
    [InlineData(ServiceEventType.DeploymentCompleted, "Deployment Completed")]
    public void GetEventTypeDescription_ReturnsCorrectDescription(ServiceEventType type, string expectedDescription)
    {
        var ev = new ServiceEvent { EventType = type };

        var description = ev.GetEventTypeDescription();

        Assert.Equal(expectedDescription, description);
    }

    [Fact]
    public void GetEventTypeDescription_ReturnsUnknown_ForUndefinedEnum()
    {
        var ev = new ServiceEvent { EventType = (ServiceEventType)999 };

        var description = ev.GetEventTypeDescription();

        Assert.Equal("Unknown Event", description);
    }

    [Fact]
    public void DefaultValues_AreInitializedCorrectly()
    {
        var ev = new ServiceEvent();

        // Id and ServiceId default to Guid.Empty
        Assert.Equal(Guid.Empty, ev.Id);
        Assert.Equal(Guid.Empty, ev.ServiceId);

        // CreatedAt should be set to a recent UTC time
        Assert.InRange(ev.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow);

        // Boolean defaults
        Assert.False(ev.AcknowledgedAt);
        Assert.Null(ev.AcknowledgedBy);
    }
}
