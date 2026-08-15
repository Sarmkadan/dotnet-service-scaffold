using System;
using System.Linq;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ServiceRegistrationTests
{
    private ServiceRegistration CreateValidRegistration()
    {
        return new ServiceRegistration
        {
            ServiceName = "Test Service",
            HealthCheckUrl = "https://example.com/health",
            Version = "1.0.0",
            Endpoint = "https://example.com/api",
            OwnerId = Guid.NewGuid()
        };
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenAllRequiredPropertiesAreSet()
    {
        var reg = CreateValidRegistration();

        Assert.True(reg.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenRequiredPropertyIsMissing()
    {
        var reg = CreateValidRegistration();
        reg.ServiceName = string.Empty; // break a required field

        Assert.False(reg.IsValid());
    }

    [Fact]
    public void GetSuccessRate_Returns100_WhenNoRequestsMade()
    {
        var reg = CreateValidRegistration();

        var rate = reg.GetSuccessRate();

        Assert.Equal(100m, rate);
    }

    [Fact]
    public void GetSuccessRate_ReturnsCorrectPercentage()
    {
        var reg = CreateValidRegistration();
        // simulate some requests
        reg.TotalRequests = 20;
        reg.SuccessfulRequests = 15;

        var rate = reg.GetSuccessRate();

        Assert.Equal(75m, rate);
    }

    [Fact]
    public void RecordSuccessfulHealthCheck_UpdatesPropertiesCorrectly()
    {
        var reg = CreateValidRegistration();

        var before = DateTime.UtcNow;
        reg.RecordSuccessfulHealthCheck();

        Assert.NotNull(reg.LastHealthCheckAt);
        Assert.True(reg.LastHealthCheckAt >= before);
        Assert.Equal(1, reg.TotalRequests);
        Assert.Equal(1, reg.SuccessfulRequests);
        Assert.Equal(0, reg.ConsecutiveFailures);
        Assert.Equal(ServiceStatus.Healthy, reg.Status);
        Assert.True(reg.UpdatedAt >= before);
    }

    [Fact]
    public void RecordFailedHealthCheck_UpdatesPropertiesAndStatus_DegradedThenUnhealthy()
    {
        var reg = CreateValidRegistration();

        // First failure – should be Degraded
        reg.RecordFailedHealthCheck();
        Assert.Equal(ServiceStatus.Degraded, reg.Status);
        Assert.Equal(1, reg.ConsecutiveFailures);
        Assert.Equal(1, reg.TotalRequests);
        Assert.NotNull(reg.LastHealthCheckAt);

        // Second failure – still Degraded
        reg.RecordFailedHealthCheck();
        Assert.Equal(ServiceStatus.Degraded, reg.Status);
        Assert.Equal(2, reg.ConsecutiveFailures);
        Assert.Equal(2, reg.TotalRequests);

        // Third failure – becomes Unhealthy
        reg.RecordFailedHealthCheck();
        Assert.Equal(ServiceStatus.Unhealthy, reg.Status);
        Assert.Equal(3, reg.ConsecutiveFailures);
        Assert.Equal(3, reg.TotalRequests);
    }

    [Fact]
    public void Disable_SetsIsEnabledFalse_AddsDisabledEvent()
    {
        var reg = CreateValidRegistration();

        reg.Disable("maintenance");

        Assert.False(reg.IsEnabled);
        Assert.Equal(ServiceStatus.Disabled, reg.Status);
        Assert.NotEmpty(reg.Events);
        var ev = reg.Events.Last();
        Assert.Equal(ServiceEventType.ServiceDisabled, ev.EventType);
        Assert.Equal("maintenance", ev.Message);
        Assert.Equal(reg.Id, ev.ServiceId);
    }

    [Fact]
    public void Enable_SetsIsEnabledTrue_ResetsFailures_AddsEnabledEvent()
    {
        var reg = CreateValidRegistration();
        // Simulate a disabled state first
        reg.Disable("maintenance");
        reg.ConsecutiveFailures = 5;
        reg.Status = ServiceStatus.Unhealthy;

        reg.Enable();

        Assert.True(reg.IsEnabled);
        Assert.Equal(ServiceStatus.Unknown, reg.Status);
        Assert.Equal(0, reg.ConsecutiveFailures);
        Assert.NotEmpty(reg.Events);
        var ev = reg.Events.Last();
        Assert.Equal(ServiceEventType.ServiceEnabled, ev.EventType);
        Assert.Equal("Service re-enabled", ev.Message);
        Assert.Equal(reg.Id, ev.ServiceId);
    }
}
