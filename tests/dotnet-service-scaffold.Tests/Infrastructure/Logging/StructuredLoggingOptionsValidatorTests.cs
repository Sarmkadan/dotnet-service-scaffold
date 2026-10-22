using DotnetServiceScaffold.Infrastructure.Logging;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.Logging;

public class StructuredLoggingOptionsValidatorTests
{
    [Theory]
    [InlineData("Verbose")]
    [InlineData("Information")]
    [InlineData("warning")]
    public void Validate_ShouldSucceed_WhenMinimumLevelIsSerilogLevel(string level)
    {
        // Arrange
        var options = new StructuredLoggingOptions { MinimumLevel = level };
        var validator = new StructuredLoggingOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenMinimumLevelIsUnknown()
    {
        // Arrange
        var options = new StructuredLoggingOptions { MinimumLevel = "Loud" };
        var validator = new StructuredLoggingOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("MinimumLevel").And.Contain("'Loud'");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenMinimumLevelIsEmpty_BecauseRequiredAttributeReportsIt()
    {
        // Arrange
        var options = new StructuredLoggingOptions { MinimumLevel = string.Empty };
        var validator = new StructuredLoggingOptionsValidator();

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }
}
