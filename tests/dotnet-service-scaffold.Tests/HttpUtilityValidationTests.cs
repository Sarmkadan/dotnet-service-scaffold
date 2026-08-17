#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

public class HttpUtilityValidationTests
{
    [Fact]
    public void ValidateBasicAuth_ValidCredentials_ReturnsEmptyList()
    {
        var problems = HttpUtilityValidation.ValidateBasicAuth("user", "password123");
        problems.Should().BeEmpty();
    }

    [Fact]
    public void ValidateBasicAuth_InvalidUsername_ReturnsProblems()
    {
        var longUsername = new string('a', 257);
        var problems = HttpUtilityValidation.ValidateBasicAuth(longUsername, "password");
        problems.Should().Contain("Username exceeds maximum length of 256 characters.");
    }

    [Fact]
    public void ValidateBearerToken_ValidToken_ReturnsEmptyList()
    {
        var problems = HttpUtilityValidation.ValidateBearerToken("valid-token");
        problems.Should().BeEmpty();
    }

    [Fact]
    public void ValidateBearerToken_NullToken_ThrowsArgumentNullException()
    {
        var act = () => HttpUtilityValidation.ValidateBearerToken(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateStatusCode_ValidCode_ReturnsEmptyList()
    {
        var problems = HttpUtilityValidation.ValidateStatusCode(200);
        problems.Should().BeEmpty();
    }

    [Fact]
    public void ValidateStatusCode_InvalidCode_ReturnsProblems()
    {
        var problems = HttpUtilityValidation.ValidateStatusCode(99);
        problems.Should().Contain("Status code must be between 100 and 599 inclusive.");
    }

    [Fact]
    public void ValidateBaseUrl_ValidUrl_ReturnsEmptyList()
    {
        var problems = HttpUtilityValidation.ValidateBaseUrl("https://example.com");
        problems.Should().BeEmpty();
    }

    [Fact]
    public void ValidateBaseUrl_InvalidUrl_ReturnsProblems()
    {
        var problems = HttpUtilityValidation.ValidateBaseUrl("not-a-url");
        problems.Should().Contain("Base URL is not a valid URI format.");
    }
}
