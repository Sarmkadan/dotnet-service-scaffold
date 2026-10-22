#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Tests;

public class HealthCheckOptionsValidatorTests
{
    private static readonly HealthCheckOptionsValidator Validator = new();

    [Fact]
    public void Validate_ShouldSucceed_WhenDefaultsAreUsed()
    {
        // Arrange
        var options = new HealthCheckOptions();

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("00:00:00")]
    [InlineData("-00:00:05")]
    public void Validate_ShouldFail_WhenIntervalIsNotPositive(string interval)
    {
        // Arrange
        var options = new HealthCheckOptions
        {
            Interval = TimeSpan.Parse(interval),
            Timeout = TimeSpan.Zero
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert: the timeout rule also fails here because Timeout >= Interval, so check for the interval failure only
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("HealthCheck:Interval must be greater than zero"));
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenIntervalIsSmallestPositiveValue()
    {
        // Arrange
        var options = new HealthCheckOptions
        {
            Interval = TimeSpan.FromTicks(1),
            Timeout = TimeSpan.Zero
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenTimeoutEqualsInterval()
    {
        // Arrange
        var options = new HealthCheckOptions
        {
            Interval = TimeSpan.FromSeconds(30),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("HealthCheck:Timeout");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTimeoutExceedsInterval()
    {
        // Arrange
        var options = new HealthCheckOptions
        {
            Interval = TimeSpan.FromSeconds(10),
            Timeout = TimeSpan.FromSeconds(11)
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("must be less than HealthCheck:Interval");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenTimeoutIsJustBelowInterval()
    {
        // Arrange
        var options = new HealthCheckOptions
        {
            Interval = TimeSpan.FromSeconds(10),
            Timeout = TimeSpan.FromSeconds(10) - TimeSpan.FromTicks(1)
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/")]
    [InlineData("/api/v1/health")]
    public void Validate_ShouldSucceed_WhenEndpointStartsWithSlash(string endpoint)
    {
        // Arrange
        var options = new HealthCheckOptions { Endpoint = endpoint };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("health")]
    [InlineData("http://localhost/health")]
    [InlineData(" /health")]
    public void Validate_ShouldFail_WhenEndpointDoesNotStartWithSlash(string endpoint)
    {
        // Arrange
        var options = new HealthCheckOptions { Endpoint = endpoint };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("HealthCheck:Endpoint must start with '/'");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenEndpointIsEmptyOrWhitespace(string endpoint)
    {
        // Arrange
        var options = new HealthCheckOptions { Endpoint = endpoint };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("HealthCheck:Endpoint must not be empty");
    }

    [Fact]
    public void Validate_ShouldReportAllFailures_WhenSeveralRulesAreBroken()
    {
        // Arrange
        var options = new HealthCheckOptions
        {
            Interval = TimeSpan.Zero,
            Timeout = TimeSpan.FromSeconds(5),
            Endpoint = "health"
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(3);
        result.Failures.Should().Contain(f => f.Contains("HealthCheck:Interval must be greater than zero"));
        result.Failures.Should().Contain(f => f.Contains("HealthCheck:Timeout"));
        result.Failures.Should().Contain(f => f.Contains("HealthCheck:Endpoint must start with '/'"));
    }

    [Fact]
    public void Validate_ShouldThrow_WhenOptionsAreNull()
    {
        // Arrange
        HealthCheckOptions? options = null;

        // Act
        var act = () => Validator.Validate(null, options!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddHealthCheckOptions_ShouldBindValues_WhenConfigurationIsValid()
    {
        // Arrange
        var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["HealthCheck:Interval"] = "00:00:30",
            ["HealthCheck:Timeout"] = "00:00:05",
            ["HealthCheck:Endpoint"] = "/ready"
        });

        // Act
        var options = provider.GetRequiredService<IOptions<HealthCheckOptions>>().Value;

        // Assert
        options.Interval.Should().Be(TimeSpan.FromSeconds(30));
        options.Timeout.Should().Be(TimeSpan.FromSeconds(5));
        options.Endpoint.Should().Be("/ready");
    }

    [Fact]
    public void AddHealthCheckOptions_ShouldThrowWithAllErrors_WhenConfigurationIsInvalid()
    {
        // Arrange
        var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["HealthCheck:Interval"] = "00:00:10",
            ["HealthCheck:Timeout"] = "00:00:10",
            ["HealthCheck:Endpoint"] = "ready"
        });

        // Act
        var act = () => provider.GetRequiredService<IOptions<HealthCheckOptions>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().HaveCount(2);
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        var services = new ServiceCollection();
        services.AddHealthCheckOptions(configuration);
        return services.BuildServiceProvider();
    }
}
