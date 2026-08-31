#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Contains tests for the StringUtility class.
/// </summary>
public class StringUtilityTests : IStringUtilityTests
{
    [Fact]
    public void Truncate_StringLongerThanMaxLength_TruncatesAndAppendsSuffix()
    {
        /// <summary>
        /// Verifies that the Truncate method truncates a string longer than the specified maxLength and appends an ellipsis.
        /// </summary>
        var result = StringUtility.Truncate("Hello, World!", StringUtilityTestsConstants.HelloWorldTruncateLength);

        result.Should().Be("Hello...");
        result.Length.Should().Be(StringUtilityTestsConstants.HelloWorldTruncateLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Truncate_NullOrEmptyInput_ReturnsEmptyString(string? input)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);

        /// <summary>
        /// Verifies that the Truncate method returns an empty string when the input is null or empty.
        /// </summary>
        /// <param name="input">The input string to truncate.</param>
        StringUtility.Truncate(input, StringUtilityTestsConstants.NullOrEmptyTruncateLength).Should().BeEmpty();
    }

    [Theory]
    [InlineData("helloWorld", "hello_world")]
    [InlineData("MyServiceName", "my_service_name")]
    [InlineData("simple", "simple")]
    public void ToSnakeCase_CamelCaseOrPascalInput_InsertsUnderscoresBeforeUpperCaseLetters(
        string input, string expected)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);
        ArgumentException.ThrowIfNullOrEmpty(expected);

        /// <summary>
        /// Verifies that the ToSnakeCase method converts camel case or pascal case input to snake case by inserting underscores before uppercase letters.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <param name="expected">The expected output string.</param>
        StringUtility.ToSnakeCase(input).Should().Be(expected);
    }

    [Fact]
    public void MaskSensitive_LongApiKey_KeepsEdgeCharactersAndMasksMiddle()
    {
        /// <summary>
        /// Verifies that the MaskSensitive method masks the middle characters of a long API key while keeping the edge characters.
        /// </summary>
        const string key = "ABCDE12345FGHIJ"; // 15 chars

        var masked = StringUtility.MaskSensitive(key, visibleChars: StringUtilityTestsConstants.MaskSensitiveVisibleChars);

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
        ArgumentException.ThrowIfNullOrEmpty(email);

        /// <summary>
        /// Verifies that the IsValidEmail method returns the expected validation outcome for various email inputs.
        /// </summary>
        /// <param name="email">The email to validate.</param>
        /// <param name="expected">The expected validation outcome.</param>
        StringUtility.IsValidEmail(email).Should().Be(expected);
    }
}
