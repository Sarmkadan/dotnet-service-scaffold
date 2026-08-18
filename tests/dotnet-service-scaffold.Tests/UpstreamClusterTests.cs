#nullable enable
using System;
using DotnetServiceScaffold.Domain.Models;
using Xunit;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

public class UpstreamClusterTests
{
    [Fact]
    public void Name_CanBeSetAndRetrieved()
    {
        var cluster = new UpstreamCluster();
        cluster.Name = "test-cluster";
        cluster.Name.Should().Be("test-cluster");
    }

    [Fact]
    public void Endpoint_CanBeSetAndRetrieved()
    {
        var cluster = new UpstreamCluster();
        cluster.Endpoint = "http://localhost:8080";
        cluster.Endpoint.Should().Be("http://localhost:8080");
    }

    [Fact]
    public void HealthyHosts_CanBeSetAndRetrieved()
    {
        var cluster = new UpstreamCluster();
        cluster.HealthyHosts = 5;
        cluster.HealthyHosts.Should().Be(5);
    }

    [Fact]
    public void TotalHosts_CanBeSetAndRetrieved()
    {
        var cluster = new UpstreamCluster();
        cluster.TotalHosts = 10;
        cluster.TotalHosts.Should().Be(10);
    }

    [Fact]
    public void CircuitBreakerOpen_CanBeSetAndRetrieved()
    {
        var cluster = new UpstreamCluster();
        cluster.CircuitBreakerOpen = true;
        cluster.CircuitBreakerOpen.Should().BeTrue();

        cluster.CircuitBreakerOpen = false;
        cluster.CircuitBreakerOpen.Should().BeFalse();
    }

    [Fact]
    public void GetHealthPercent_ReturnsHundred_WhenTotalHostsIsZero()
    {
        var cluster = new UpstreamCluster
        {
            TotalHosts = 0,
            HealthyHosts = 0 // Could be any value, but we set to 0 for clarity
        };

        cluster.GetHealthPercent().Should().Be(100m);
    }

    [Fact]
    public void GetHealthPercent_ReturnsZero_WhenHealthyHostsIsZeroAndTotalHostsPositive()
    {
        var cluster = new UpstreamCluster
        {
            HealthyHosts = 0,
            TotalHosts = 5
        };

        cluster.GetHealthPercent().Should().Be(0m);
    }

    [Fact]
    public void GetHealthPercent_ReturnsCorrectPercentage_WhenHealthyHostsLessThanTotalHosts()
    {
        var cluster = new UpstreamCluster
        {
            HealthyHosts = 3,
            TotalHosts = 4
        };

        cluster.GetHealthPercent().Should().Be(75m);
    }

    [Fact]
    public void GetHealthPercent_ReturnsHundred_WhenHealthyHostsEqualsTotalHosts()
    {
        var cluster = new UpstreamCluster
        {
            HealthyHosts = 7,
            TotalHosts = 7
        };

        cluster.GetHealthPercent().Should().Be(100m);
    }

    [Fact]
    public void GetHealthPercent_HandlesLargeNumbersCorrectly()
    {
        var cluster = new UpstreamCluster
        {
            HealthyHosts = 999,
            TotalHosts = 1000
        };

        cluster.GetHealthPercent().Should().Be(99.9m);
    }
}