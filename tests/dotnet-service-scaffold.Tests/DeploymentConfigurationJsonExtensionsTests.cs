#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using DotnetServiceScaffold.Infrastructure.Configuration;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Tests for the <see cref="DeploymentConfigurationJsonExtensions"/> class.
/// </summary>
public class DeploymentConfigurationJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidObject_ReturnsJsonString()
    {
        var config = new DeploymentConfiguration();
        var json = config.ToJson();
        json.Should().Be("{}");
    }

    [Fact]
    public void ToJson_Indented_ReturnsSameJsonAsNonIndentedForEmptyObject()
    {
        var config = new DeploymentConfiguration();
        var jsonIndented = config.ToJson(indented: true);
        var json = config.ToJson(indented: false);
        jsonIndented.Should().Be(json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        DeploymentConfiguration? config = null;
        Action act = () => config!.ToJson();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsObject()
    {
        var json = "{}";
        var config = DeploymentConfigurationJsonExtensions.FromJson(json);
        config.Should().NotBeNull();
    }

    [Fact]
    public void FromJson_EmptyOrNullJson_ReturnsNull()
    {
        DeploymentConfigurationJsonExtensions.FromJson("").Should().BeNull();
        DeploymentConfigurationJsonExtensions.FromJson("   ").Should().BeNull();
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        var json = "{invalid}";
        Action act = () => DeploymentConfigurationJsonExtensions.FromJson(json);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        var json = "{}";
        var success = DeploymentConfigurationJsonExtensions.TryFromJson(json, out var config);
        success.Should().BeTrue();
        config.Should().NotBeNull();
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        var json = "{invalid}";
        var success = DeploymentConfigurationJsonExtensions.TryFromJson(json, out var config);
        success.Should().BeFalse();
        config.Should().BeNull();
    }
}
