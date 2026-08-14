using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ServiceDiscoveryOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldMatchExpectedDefaults()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions();

        // Assert
        Assert.True(options.Enabled);
        Assert.Equal(DiscoveryMode.Dns, options.Mode);
        Assert.Equal(LoadBalancingStrategy.RoundRobin, options.LoadBalancing);
        Assert.Equal(TimeSpan.FromSeconds(30), options.CacheTtl);
        Assert.Equal(TimeSpan.FromSeconds(15), options.RefreshInterval);
        Assert.Equal(TimeSpan.FromSeconds(5), options.ResolutionTimeout);
        Assert.NotNull(options.Dns);
        Assert.NotNull(options.Registry);
        Assert.NotNull(options.SelfRegistration);
    }

    [Fact]
    public void CanModifyAllProperties_AndValuesPersist()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions
        {
            Enabled = false,
            Mode = DiscoveryMode.Hybrid,
            LoadBalancing = LoadBalancingStrategy.Random,
            CacheTtl = TimeSpan.FromMinutes(2),
            RefreshInterval = TimeSpan.FromSeconds(45),
            ResolutionTimeout = TimeSpan.FromSeconds(10),
            Dns = new DnsDiscoveryOptions
            {
                SearchDomain = "custom.domain",
                PreferSrvRecords = false,
                DnsServerAddress = "8.8.8.8",
                DnsServerPort = 5353,
                DefaultPort = 8080,
                DefaultScheme = "http",
                MaxRetries = 5,
                SocketTimeout = TimeSpan.FromSeconds(3)
            },
            Registry = new RegistryDiscoveryOptions
            {
                AgentEndpoint = "http://registry.local:8500",
                AclToken = "secret-token",
                OnlyHealthyInstances = false,
                Datacenter = "dc1",
                HeartbeatInterval = TimeSpan.FromSeconds(20)
            },
            SelfRegistration = new SelfRegistrationOptions
            {
                Enabled = true,
                ServiceName = "my-service",
                Version = "1.2.3",
                AdvertiseHost = "10.0.0.5",
                AdvertisePort = 5000,
                AdvertiseScheme = "http",
                HealthCheckPath = "/ping",
                Tags = new List<string> { "tag1", "tag2" }
            }
        };

        // Assert
        Assert.False(options.Enabled);
        Assert.Equal(DiscoveryMode.Hybrid, options.Mode);
        Assert.Equal(LoadBalancingStrategy.Random, options.LoadBalancing);
        Assert.Equal(TimeSpan.FromMinutes(2), options.CacheTtl);
        Assert.Equal(TimeSpan.FromSeconds(45), options.RefreshInterval);
        Assert.Equal(TimeSpan.FromSeconds(10), options.ResolutionTimeout);

        // Dns
        Assert.Equal("custom.domain", options.Dns.SearchDomain);
        Assert.False(options.Dns.PreferSrvRecords);
        Assert.Equal("8.8.8.8", options.Dns.DnsServerAddress);
        Assert.Equal(5353, options.Dns.DnsServerPort);
        Assert.Equal(8080, options.Dns.DefaultPort);
        Assert.Equal("http", options.Dns.DefaultScheme);
        Assert.Equal(5, options.Dns.MaxRetries);
        Assert.Equal(TimeSpan.FromSeconds(3), options.Dns.SocketTimeout);

        // Registry
        Assert.Equal("http://registry.local:8500", options.Registry.AgentEndpoint);
        Assert.Equal("secret-token", options.Registry.AclToken);
        Assert.False(options.Registry.OnlyHealthyInstances);
        Assert.Equal("dc1", options.Registry.Datacenter);
        Assert.Equal(TimeSpan.FromSeconds(20), options.Registry.HeartbeatInterval);

        // SelfRegistration
        Assert.True(options.SelfRegistration.Enabled);
        Assert.Equal("my-service", options.SelfRegistration.ServiceName);
        Assert.Equal("1.2.3", options.SelfRegistration.Version);
        Assert.Equal("10.0.0.5", options.SelfRegistration.AdvertiseHost);
        Assert.Equal(5000, options.SelfRegistration.AdvertisePort);
        Assert.Equal("http", options.SelfRegistration.AdvertiseScheme);
        Assert.Equal("/ping", options.SelfRegistration.HealthCheckPath);
        Assert.Equal(new[] { "tag1", "tag2" }, options.SelfRegistration.Tags);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NegativeOrZeroCacheTtl_ShouldBeSetWithoutException(long seconds)
    {
        // Arrange
        var options = new ServiceDiscoveryOptions
        {
            CacheTtl = TimeSpan.FromSeconds(seconds)
        };

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(seconds), options.CacheTtl);
    }

    [Fact]
    public void DnsSearchDomain_CanBeNullOrEmpty()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions
        {
            Dns = new DnsDiscoveryOptions
            {
                SearchDomain = string.Empty
            }
        };

        // Assert
        Assert.Equal(string.Empty, options.Dns.SearchDomain);
    }

    [Fact]
    public void SelfRegistration_TagsCanBeEmptyList()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions
        {
            SelfRegistration = new SelfRegistrationOptions
            {
                Tags = new List<string>()
            }
        };

        // Assert
        Assert.Empty(options.SelfRegistration.Tags);
    }

    [Fact]
    public void Registry_AclToken_AllowsNull()
    {
        // Arrange
        var options = new ServiceDiscoveryOptions
        {
            Registry = new RegistryDiscoveryOptions
            {
                AclToken = null
            }
        };

        // Assert
        Assert.Null(options.Registry.AclToken);
    }
}
