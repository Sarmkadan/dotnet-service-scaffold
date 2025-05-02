// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

public class StringUtilityTests
{
    [Fact]
    public void Truncate_StringLongerThanMaxLength_TruncatesAndAppendsSuffix()
    {
        var result = StringUtility.Truncate("Hello, World!", 8);

        result.Should().Be("Hello...");
        result.Length.Should().Be(8);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Truncate_NullOrEmptyInput_ReturnsEmptyString(string? input)
    {
        StringUtility.Truncate(input, 10).Should().BeEmpty();
    }

    [Theory]
    [InlineData("helloWorld", "hello_world")]
    [InlineData("MyServiceName", "my_service_name")]
    [InlineData("simple", "simple")]
    public void ToSnakeCase_CamelCaseOrPascalInput_InsertsUnderscoresBeforeUpperCaseLetters(
        string input, string expected)
    {
        StringUtility.ToSnakeCase(input).Should().Be(expected);
    }

    [Fact]
    public void MaskSensitive_LongApiKey_KeepsEdgeCharactersAndMasksMiddle()
    {
        const string key = "ABCDE12345FGHIJ"; // 15 chars

        var masked = StringUtility.MaskSensitive(key, visibleChars: 2);

        masked.Should().StartWith("AB");
        masked.Should().EndWith("IJ");
        masked.Should().Contain("*");
        masked.Length.Should().Be(key.Length);
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("name+tag@sub.domain.org", true)]
    [InlineData("not-an-email", false)]
    [InlineData("@missinguser.com", false)]
    [InlineData("", false)]
    public void IsValidEmail_VariousInputs_ReturnsExpectedValidationOutcome(
        string email, bool expected)
    {
        StringUtility.IsValidEmail(email).Should().Be(expected);
    }
}
