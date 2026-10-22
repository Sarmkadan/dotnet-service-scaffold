using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.ServiceDiscovery;

public class ServiceDiscoveryOptionsValidatorTests
{
    [Fact]
    public void Validate_ShouldSucceed_WhenDefaultOptionsAreUsed()
    {
        // Arrange
        var validator = new ServiceDiscoveryOptionsValidator();

        // Act
        var result = validator.Validate(null, new ServiceDiscoveryOptions());

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenAgentEndpointIsNotAbsoluteUrl()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions();
        options.Registry.AgentEndpoint = "consul.local:8500";
        var validator = new ServiceDiscoveryOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("Registry.AgentEndpoint");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDnsPortsAreOutOfRange()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions();
        options.Dns.DnsServerPort = 0;
        options.Dns.DefaultPort = 65536;
        var validator = new ServiceDiscoveryOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(2);
        result.Failures.Should().Contain(f => f.Contains("Dns.DnsServerPort"));
        result.Failures.Should().Contain(f => f.Contains("Dns.DefaultPort"));
    }

    [Fact]
    public void Validate_ShouldFail_WhenSelfRegistrationEnabledWithInvalidAdvertisePort()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions();
        options.SelfRegistration.Enabled = true;
        options.SelfRegistration.AdvertisePort = 70000;
        var validator = new ServiceDiscoveryOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("SelfRegistration.AdvertisePort");
    }

    [Fact]
    public void Validate_ShouldIgnoreSelfRegistrationPort_WhenSelfRegistrationDisabled()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions();
        options.SelfRegistration.Enabled = false;
        options.SelfRegistration.AdvertisePort = 70000;
        var validator = new ServiceDiscoveryOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenEvictionThresholdIsShorterThanStaleThreshold()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions
        {
            HeartbeatStaleThreshold = TimeSpan.FromMinutes(5),
            EvictionThreshold = TimeSpan.FromMinutes(3)
        };
        var validator = new ServiceDiscoveryOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("EvictionThreshold").And.Contain("HeartbeatStaleThreshold");
    }
}
