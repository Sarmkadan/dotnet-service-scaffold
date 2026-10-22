using DotnetServiceScaffold.Shared.Configuration;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests.Shared.Configuration;

public class DotnetServiceScaffoldOptionsValidatorTests
{
    private const string ValidMetricsApiKey = "metrics_test_key";

    [Fact]
    public void Validate_ShouldSucceed_WhenOptionsAreConsistent()
    {
        // Arrange
        var options = CreateValidOptions();
        var validator = new DotnetServiceScaffoldOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenHealthCheckTimeoutEqualsInterval()
    {
        // Arrange
        var options = CreateValidOptions();
        options.HealthCheckInterval = 30;
        options.HealthCheckTimeout = 30;
        var validator = new DotnetServiceScaffoldOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("HealthCheckTimeout (30s)").And.Contain("HealthCheckInterval (30s)");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDefaultPageSizeExceedsMaxPageSize()
    {
        // Arrange
        var options = CreateValidOptions();
        options.DefaultPageSize = 500;
        options.MaxPageSize = 200;
        var validator = new DotnetServiceScaffoldOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("DefaultPageSize").And.Contain("MaxPageSize");
    }

    [Fact]
    public void Validate_ShouldFail_WhenApiKeyModeHasNoMetricsApiKey()
    {
        // Arrange
        var options = CreateValidOptions();
        options.MetricsProtectionMode = "ApiKey";
        options.MetricsApiKey = "   ";
        var validator = new DotnetServiceScaffoldOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("MetricsApiKey");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenLocalhostOnlyModeHasNoMetricsApiKey()
    {
        // Arrange
        var options = CreateValidOptions();
        options.MetricsProtectionMode = "LocalhostOnly";
        options.MetricsApiKey = string.Empty;
        var validator = new DotnetServiceScaffoldOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("Disabled")]
    [InlineData("apikey")]
    [InlineData("LOCALHOSTONLY")]
    public void Validate_ShouldAcceptMode_WhenModeMatchesAllowedValueIgnoringCase(string mode)
    {
        // Arrange
        var options = CreateValidOptions();
        options.MetricsProtectionMode = mode;
        var validator = new DotnetServiceScaffoldOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenMetricsProtectionModeIsUnknown()
    {
        // Arrange
        var options = CreateValidOptions();
        options.MetricsProtectionMode = "Public";
        var validator = new DotnetServiceScaffoldOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("MetricsProtectionMode").And.Contain("'Public'");
    }

    [Fact]
    public void Validate_ShouldReportEveryCrossFieldFailure_WhenSeveralRulesBreak()
    {
        // Arrange
        var options = CreateValidOptions();
        options.HealthCheckInterval = 20;
        options.HealthCheckTimeout = 25;
        options.DefaultPageSize = 300;
        options.MaxPageSize = 100;
        options.MetricsApiKey = string.Empty;
        var validator = new DotnetServiceScaffoldOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(3);
        result.Failures.Should().Contain(f => f.Contains("HealthCheckTimeout"));
        result.Failures.Should().Contain(f => f.Contains("DefaultPageSize"));
        result.Failures.Should().Contain(f => f.Contains("MetricsApiKey"));
    }

    private static DotnetServiceScaffoldOptions CreateValidOptions()
    {
        return new DotnetServiceScaffoldOptions
        {
            HealthCheckInterval = 60,
            HealthCheckTimeout = 10,
            DefaultPageSize = 50,
            MaxPageSize = 200,
            MetricsProtectionMode = "ApiKey",
            MetricsApiKey = ValidMetricsApiKey
        };
    }
}
