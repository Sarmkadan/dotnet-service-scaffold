using System;
using System.Linq;
using DotnetServiceScaffold.Domain.Enums;
using DotnetServiceScaffold.Domain.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ServiceRegistrationValidationTests
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
    public void Validate_ReturnsEmpty_WhenRegistrationIsValid()
    {
        var reg = CreateValidRegistration();

        var errors = reg.Validate();

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ThrowsArgumentNull_WhenValueIsNull()
    {
        ServiceRegistration? reg = null;

        Assert.Throws<ArgumentNullException>(() => reg.Validate());
    }

    [Fact]
    public void Validate_ReportsMissingRequiredFields()
    {
        var reg = CreateValidRegistration();
        reg.ServiceName = string.Empty;
        reg.HealthCheckUrl = "   ";
        reg.Version = null!;
        reg.Endpoint = string.Empty;
        reg.OwnerId = Guid.Empty;

        var errors = reg.Validate();

        Assert.Contains(errors, e => e.Contains("ServiceName"));
        Assert.Contains(errors, e => e.Contains("HealthCheckUrl"));
        Assert.Contains(errors, e => e.Contains("Version"));
        Assert.Contains(errors, e => e.Contains("Endpoint"));
        Assert.Contains(errors, e => e.Contains("OwnerId"));
    }

    [Fact]
    public void Validate_ReportsLengthBoundaries()
    {
        var reg = CreateValidRegistration();
        reg.ServiceName = new string('a', 256);
        reg.Version = new string('b', 51);
        reg.Endpoint = new string('c', 256);
        reg.SystemdServiceName = new string('d', 501);

        var errors = reg.Validate();

        Assert.Contains(errors, e => e.Contains("ServiceName must be 255"));
        Assert.Contains(errors, e => e.Contains("Version must be 50"));
        Assert.Contains(errors, e => e.Contains("Endpoint must be 255"));
        Assert.Contains(errors, e => e.Contains("SystemdServiceName must be 500"));
    }

    [Fact]
    public void Validate_ReportsNumericBoundaryViolations()
    {
        var reg = CreateValidRegistration();
        reg.HealthCheckIntervalSeconds = 0;
        reg.TimeoutSeconds = 301;
        reg.SuccessfulRequests = 10;
        reg.TotalRequests = 5;

        var errors = reg.Validate();

        Assert.Contains(errors, e => e.Contains("HealthCheckIntervalSeconds"));
        Assert.Contains(errors, e => e.Contains("TimeoutSeconds"));
        Assert.Contains(errors, e => e.Contains("SuccessfulRequests cannot exceed TotalRequests"));
    }

    [Fact]
    public void Validate_ReportsCrossFieldStatusConflict()
    {
        var reg = CreateValidRegistration();
        reg.IsEnabled = true;
        reg.Status = ServiceStatus.Disabled;

        var errors = reg.Validate();

        Assert.Contains(errors, e => e.Contains("cannot be enabled while having Disabled status"));
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidRegistration()
    {
        var reg = CreateValidRegistration();

        Assert.True(reg.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForInvalidRegistration()
    {
        var reg = CreateValidRegistration();
        reg.ServiceName = string.Empty;

        Assert.False(reg.IsValid());
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForValidRegistration()
    {
        var reg = CreateValidRegistration();

        reg.EnsureValid();
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_ForInvalidRegistration()
    {
        var reg = CreateValidRegistration();
        reg.ServiceName = string.Empty;

        var ex = Assert.Throws<ArgumentException>(() => reg.EnsureValid());

        Assert.Contains("ServiceName", ex.Message);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNull_WhenValueIsNull()
    {
        ServiceRegistration? reg = null;

        Assert.Throws<ArgumentNullException>(() => reg.EnsureValid());
    }
}