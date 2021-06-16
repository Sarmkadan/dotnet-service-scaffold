#nullable enable
using FluentAssertions;
using DotnetServiceScaffold.Shared.Utilities;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public sealed class ValidationUtilityEdgeCaseTests
{
    [Fact]
    public void ValidateNotNullOrEmpty_Null_ThrowsArgumentException()
    {
        var act = () => ValidationUtility.ValidateNotNullOrEmpty(null, "test");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateNotNullOrEmpty_Empty_ThrowsArgumentException()
    {
        var act = () => ValidationUtility.ValidateNotNullOrEmpty("", "test");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateNotNullOrEmpty_Whitespace_ThrowsArgumentException()
    {
        var act = () => ValidationUtility.ValidateNotNullOrEmpty("   ", "test");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateRange_BelowMin_ThrowsArgumentOutOfRange()
    {
        var act = () => ValidationUtility.ValidateRange(5, 10, 100, "value");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ValidateRange_AboveMax_ThrowsArgumentOutOfRange()
    {
        var act = () => ValidationUtility.ValidateRange(200, 10, 100, "value");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ValidateRange_AtMinBoundary_DoesNotThrow()
    {
        var act = () => ValidationUtility.ValidateRange(10, 10, 100, "value");
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateRange_AtMaxBoundary_DoesNotThrow()
    {
        var act = () => ValidationUtility.ValidateRange(100, 10, 100, "value");
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateLength_TooShort_ThrowsArgumentException()
    {
        var act = () => ValidationUtility.ValidateLength("ab", 3, 10, "name");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateLength_TooLong_ThrowsArgumentException()
    {
        var act = () => ValidationUtility.ValidateLength("abcdefghijk", 3, 10, "name");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateLength_NullValue_ThrowsArgumentException()
    {
        var act = () => ValidationUtility.ValidateLength(null, 3, 10, "name");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("short", false)]
    [InlineData("nouppercase1!", false)]
    [InlineData("NOLOWERCASE1!", false)]
    [InlineData("NoDigitsHere!", false)]
    [InlineData("NoSpecial1abc", false)]
    [InlineData("ValidPass1!", true)]
    [InlineData("C0mpl3x!Pass", true)]
    public void IsPasswordStrong_VariousInputs(string? password, bool expected) =>
        ValidationUtility.IsPasswordStrong(password).Should().Be(expected);

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("not-a-url", false)]
    [InlineData("https://example.com", true)]
    [InlineData("http://localhost:5000", true)]
    public void IsValidUrl_VariousInputs(string? url, bool expected) =>
        ValidationUtility.IsValidUrl(url).Should().Be(expected);
}
