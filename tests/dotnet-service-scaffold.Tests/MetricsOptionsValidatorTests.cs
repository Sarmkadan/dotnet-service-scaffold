#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Tests;

public class MetricsOptionsValidatorTests
{
    private static readonly MetricsOptionsValidator Validator = new();

    [Fact]
    public void Validate_ShouldSucceed_WhenDefaultsAreUsed()
    {
        // Arrange
        var options = new MetricsOptions();

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(1024)]
    [InlineData(8080)]
    [InlineData(65535)]
    public void Validate_ShouldSucceed_WhenPortIsWithinRange(int port)
    {
        // Arrange
        var options = new MetricsOptions { Port = port };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(80)]
    [InlineData(1023)]
    [InlineData(-1)]
    public void Validate_ShouldFail_WhenPortIsBelowRange(int port)
    {
        // Arrange
        var options = new MetricsOptions { Port = port };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("Metrics:Port must be between 1024 and 65535");
    }

    [Theory]
    [InlineData(65536)]
    [InlineData(100000)]
    public void Validate_ShouldFail_WhenPortIsAboveRange(int port)
    {
        // Arrange
        var options = new MetricsOptions { Port = port };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("Metrics:Port must be between 1024 and 65535");
    }

    [Theory]
    [InlineData("scaffold")]
    [InlineData("_private")]
    [InlineData("app_metrics_2")]
    [InlineData("a")]
    public void Validate_ShouldSucceed_WhenPrefixMatchesPattern(string prefix)
    {
        // Arrange
        var options = new MetricsOptions { Prefix = prefix };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("Scaffold")]
    [InlineData("2fast")]
    [InlineData("my-prefix")]
    [InlineData("my.prefix")]
    [InlineData("my prefix")]
    [InlineData("scaffold\n")]
    public void Validate_ShouldFail_WhenPrefixDoesNotMatchPattern(string prefix)
    {
        // Arrange
        var options = new MetricsOptions { Prefix = prefix };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("Metrics:Prefix must match");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenPrefixIsEmpty(string? prefix)
    {
        // Arrange
        var options = new MetricsOptions { Prefix = prefix! };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("Metrics:Prefix must match");
    }

    [Fact]
    public void Validate_ShouldStillCheckPortAndPrefix_WhenMetricsAreDisabled()
    {
        // Arrange
        var options = new MetricsOptions
        {
            Enabled = false,
            Port = 80,
            Prefix = "Bad-Prefix"
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(2);
    }

    [Fact]
    public void Validate_ShouldReportAllFailures_WhenPortAndPrefixAreBroken()
    {
        // Arrange
        var options = new MetricsOptions { Port = 22, Prefix = "Scaffold" };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(2);
        result.Failures.Should().Contain(f => f.Contains("Metrics:Port"));
        result.Failures.Should().Contain(f => f.Contains("Metrics:Prefix"));
    }

    [Fact]
    public void Validate_ShouldThrow_WhenOptionsAreNull()
    {
        // Arrange
        MetricsOptions? options = null;

        // Act
        var act = () => Validator.Validate(null, options!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddMetricsOptions_ShouldBindValues_WhenConfigurationIsValid()
    {
        // Arrange
        var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Metrics:Enabled"] = "false",
            ["Metrics:Port"] = "9100",
            ["Metrics:Prefix"] = "orders_service"
        });

        // Act
        var options = provider.GetRequiredService<IOptions<MetricsOptions>>().Value;

        // Assert
        options.Enabled.Should().BeFalse();
        options.Port.Should().Be(9100);
        options.Prefix.Should().Be("orders_service");
    }

    [Fact]
    public void AddMetricsOptions_ShouldThrowWithAllErrors_WhenConfigurationIsInvalid()
    {
        // Arrange
        var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Metrics:Port"] = "443",
            ["Metrics:Prefix"] = "Scaffold"
        });

        // Act
        var act = () => provider.GetRequiredService<IOptions<MetricsOptions>>().Value;

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
        services.AddMetricsOptions(configuration);
        return services.BuildServiceProvider();
    }
}
