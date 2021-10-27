#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using NSubstitute;
using Xunit;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using DotnetServiceScaffold.Application.Services;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Tests for the FeatureFlagService class.
/// </summary>
public class FeatureFlagServiceTests
{
    private readonly ILogger<FeatureFlagService> _logger;
    private readonly FeatureFlagService _featureFlagService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureFlagServiceTests"/> class.
    /// </summary>
    public FeatureFlagServiceTests()
    {
        _logger = Substitute.For<ILogger<FeatureFlagService>>();
        _featureFlagService = new FeatureFlagService(_logger);
    }

    /// <summary>
    /// Verifies that IsEnabled returns true when the feature is enabled.
    /// </summary>
    [Fact]
    public void IsEnabled_ShouldReturnTrue_WhenFeatureIsEnabled()
    {
        // Arrange
        _featureFlagService.EnableFeature("audit_logging"); // Default is true, but good to explicitly set

        // Act
        var result = _featureFlagService.IsEnabled("audit_logging");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsEnabled returns false when the feature is disabled.
    /// </summary>
    [Fact]
    public void IsEnabled_ShouldReturnFalse_WhenFeatureIsDisabled()
    {
        // Arrange
        _featureFlagService.DisableFeature("audit_logging");

        // Act
        var result = _featureFlagService.IsEnabled("audit_logging");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsEnabled returns false when the feature is not found.
    /// </summary>
    [Fact]
    public void IsEnabled_ShouldReturnFalse_WhenFeatureNotFound()
    {
        // Act
        var result = _featureFlagService.IsEnabled("non_existent_feature");

        // Assert
        result.Should().BeFalse();
        _logger.Received(1).LogWarning("Feature flag '{FeatureName}' not found, defaulting to false", "non_existent_feature");
    }

    /// <summary>
    /// Verifies that EnableFeature sets the feature to enabled.
    /// </summary>
    [Fact]
    public void EnableFeature_ShouldSetFeatureToEnabled()
    {
        // Arrange
        _featureFlagService.DisableFeature("rate_limiting"); // Ensure it's disabled first

        // Act
        _featureFlagService.EnableFeature("rate_limiting");

        // Assert
        _featureFlagService.IsEnabled("rate_limiting").Should().BeTrue();
        _logger.Received(1).LogInformation("Feature '{FeatureName}' enabled", "rate_limiting");
    }

    /// <summary>
    /// Verifies that DisableFeature sets the feature to disabled.
    /// </summary>
    [Fact]
    public void DisableFeature_ShouldSetFeatureToDisabled()
    {
        // Arrange
        _featureFlagService.EnableFeature("rate_limiting"); // Ensure it's enabled first

        // Act
        _featureFlagService.DisableFeature("rate_limiting");

        // Assert
        _featureFlagService.IsEnabled("rate_limiting").Should().BeFalse();
        _logger.Received(1).LogInformation("Feature '{FeatureName}' disabled", "rate_limiting");
    }

    /// <summary>
    /// Verifies that SetRolloutPercentage updates the percentage.
    /// </summary>
    [Fact]
    public void SetRolloutPercentage_ShouldUpdatePercentage()
    {
        // Arrange
        var featureName = "advanced_analytics"; // This is initially false, but has a rollout.
        _featureFlagService.SetRolloutPercentage(featureName, 50);

        // Act
        var flag = _featureFlagService.GetFlag(featureName);

        // Assert
        flag.Should().NotBeNull();
        flag.RolloutPercentage.Should().Be(50);
        _logger.Received(1).LogInformation("Feature '{FeatureName}' rollout percentage set to {Percentage}%", featureName, 50);
    }

    /// <summary>
    /// Verifies that SetRolloutPercentage throws an ArgumentException for invalid percentages.
    /// </summary>
    /// <param name="invalidPercentage">The invalid percentage to test.</param>
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void SetRolloutPercentage_ShouldThrowArgumentException_ForInvalidPercentage(int invalidPercentage)
    {
        // Arrange
        var featureName = "health_checks";

        // Act
        Action act = () => _featureFlagService.SetRolloutPercentage(featureName, invalidPercentage);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Rollout percentage must be between 0 and 100*");
    }

    /// <summary>
    /// Verifies that RegisterFeature adds a new feature.
    /// </summary>
    [Fact]
    public void RegisterFeature_ShouldAddNewFeature()
    {
        // Arrange
        var newFeatureName = "new_cool_feature";
        var description = "A brand new feature";

        // Act
        _featureFlagService.RegisterFeature(newFeatureName, description, true);

        // Assert
        _featureFlagService.IsEnabled(newFeatureName).Should().BeTrue();
        _featureFlagService.GetFlag(newFeatureName).Should().NotBeNull()
            .And.Match<FeatureFlagInfo>(f => f.Name == newFeatureName && f.Description == description && f.IsEnabled);
        _logger.Received(1).LogInformation("Feature '{FeatureName}' registered (enabled: {Enabled})", newFeatureName, true);
    }

    /// <summary>
    /// Verifies that GetAllFlags returns all registered flags.
    /// </summary>
    [Fact]
    public void GetAllFlags_ShouldReturnAllRegisteredFlags()
    {
        // Arrange (initial flags are already registered in constructor)
        _featureFlagService.RegisterFeature("another_feature", "Just another feature");

        // Act
        var allFlags = _featureFlagService.GetAllFlags().ToList();

        // Assert
        allFlags.Should().HaveCountGreaterOrEqualTo(7); // Initial 6 + 1 new one
        allFlags.Should().Contain(f => f.Name == "audit_logging" && f.IsEnabled == true);
        allFlags.Should().Contain(f => f.Name == "another_feature" && f.IsEnabled == false);
    }
}
