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
public class FeatureFlagServiceTests : IFeatureFlagServiceTests
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
        _featureFlagService.EnableFeature(FeatureFlagServiceTestsConstants.AuditLoggingFeatureName); // Default is true, but good to explicitly set

        // Act
        var result = _featureFlagService.IsEnabled(FeatureFlagServiceTestsConstants.AuditLoggingFeatureName);

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
        _featureFlagService.DisableFeature(FeatureFlagServiceTestsConstants.AuditLoggingFeatureName);

        // Act
        var result = _featureFlagService.IsEnabled(FeatureFlagServiceTestsConstants.AuditLoggingFeatureName);

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
        var result = _featureFlagService.IsEnabled(FeatureFlagServiceTestsConstants.NonExistentFeatureName);

        // Assert
        result.Should().BeFalse();
        _logger.Received(FeatureFlagServiceTestsConstants.ExpectedLogInvocationCount).LogWarning(FeatureFlagServiceTestsConstants.FeatureNotFoundLogFormat, FeatureFlagServiceTestsConstants.NonExistentFeatureName);
    }

    /// <summary>
    /// Verifies that EnableFeature sets the feature to enabled.
    /// </summary>
    [Fact]
    public void EnableFeature_ShouldSetFeatureToEnabled()
    {
        // Arrange
        _featureFlagService.DisableFeature(FeatureFlagServiceTestsConstants.RateLimitingFeatureName); // Ensure it's disabled first

        // Act
        _featureFlagService.EnableFeature(FeatureFlagServiceTestsConstants.RateLimitingFeatureName);

        // Assert
        _featureFlagService.IsEnabled(FeatureFlagServiceTestsConstants.RateLimitingFeatureName).Should().BeTrue();
        _logger.Received(FeatureFlagServiceTestsConstants.ExpectedLogInvocationCount).LogInformation(FeatureFlagServiceTestsConstants.FeatureEnabledLogFormat, FeatureFlagServiceTestsConstants.RateLimitingFeatureName);
    }

    /// <summary>
    /// Verifies that DisableFeature sets the feature to disabled.
    /// </summary>
    [Fact]
    public void DisableFeature_ShouldSetFeatureToDisabled()
    {
        // Arrange
        _featureFlagService.EnableFeature(FeatureFlagServiceTestsConstants.RateLimitingFeatureName); // Ensure it's enabled first

        // Act
        _featureFlagService.DisableFeature(FeatureFlagServiceTestsConstants.RateLimitingFeatureName);

        // Assert
        _featureFlagService.IsEnabled(FeatureFlagServiceTestsConstants.RateLimitingFeatureName).Should().BeFalse();
        _logger.Received(FeatureFlagServiceTestsConstants.ExpectedLogInvocationCount).LogInformation(FeatureFlagServiceTestsConstants.FeatureDisabledLogFormat, FeatureFlagServiceTestsConstants.RateLimitingFeatureName);
    }

    /// <summary>
    /// Verifies that SetRolloutPercentage updates the percentage.
    /// </summary>
    [Fact]
    public void SetRolloutPercentage_ShouldUpdatePercentage()
    {
        // Arrange
        var featureName = FeatureFlagServiceTestsConstants.AdvancedAnalyticsFeatureName; // This is initially false, but has a rollout.
        _featureFlagService.SetRolloutPercentage(featureName, FeatureFlagServiceTestsConstants.ValidRolloutPercentage);

        // Act
        var flag = _featureFlagService.GetFlag(featureName);

        // Assert
        flag.Should().NotBeNull();
        flag.RolloutPercentage.Should().Be(FeatureFlagServiceTestsConstants.ValidRolloutPercentage);
        _logger.Received(FeatureFlagServiceTestsConstants.ExpectedLogInvocationCount).LogInformation(FeatureFlagServiceTestsConstants.RolloutPercentageSetLogFormat, featureName, FeatureFlagServiceTestsConstants.ValidRolloutPercentage);
    }

    /// <summary>
    /// Verifies that SetRolloutPercentage throws an ArgumentException for invalid percentages.
    /// </summary>
    /// <param name="invalidPercentage">The invalid percentage to test.</param>
    [Theory]
    [InlineData(FeatureFlagServiceTestsConstants.InvalidLowRolloutPercentage)]
    [InlineData(FeatureFlagServiceTestsConstants.InvalidHighRolloutPercentage)]
    public void SetRolloutPercentage_ShouldThrowArgumentException_ForInvalidPercentage(int invalidPercentage)
    {
        // Arrange
        var featureName = FeatureFlagServiceTestsConstants.HealthChecksFeatureName;

        // Act
        Action act = () => _featureFlagService.SetRolloutPercentage(featureName, invalidPercentage);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage(FeatureFlagServiceTestsConstants.InvalidRolloutPercentageMessagePattern);
    }

    /// <summary>
    /// Verifies that RegisterFeature adds a new feature.
    /// </summary>
    [Fact]
    public void RegisterFeature_ShouldAddNewFeature()
    {
        // Arrange
        var newFeatureName = FeatureFlagServiceTestsConstants.NewCoolFeatureName;
        var description = FeatureFlagServiceTestsConstants.NewCoolFeatureDescription;

        // Act
        _featureFlagService.RegisterFeature(newFeatureName, description, true);

        // Assert
        _featureFlagService.IsEnabled(newFeatureName).Should().BeTrue();
        _featureFlagService.GetFlag(newFeatureName).Should().NotBeNull()
            .And.Match<FeatureFlagInfo>(f => f.Name == newFeatureName && f.Description == description && f.IsEnabled);
        _logger.Received(FeatureFlagServiceTestsConstants.ExpectedLogInvocationCount).LogInformation(FeatureFlagServiceTestsConstants.FeatureRegisteredLogFormat, newFeatureName, true);
    }

    /// <summary>
    /// Verifies that GetAllFlags returns all registered flags.
    /// </summary>
    [Fact]
    public void GetAllFlags_ShouldReturnAllRegisteredFlags()
    {
        // Arrange (initial flags are already registered in constructor)
        _featureFlagService.RegisterFeature(FeatureFlagServiceTestsConstants.AnotherFeatureName, FeatureFlagServiceTestsConstants.AnotherFeatureDescription);

        // Act
        var allFlags = _featureFlagService.GetAllFlags().ToList();

        // Assert
        allFlags.Should().HaveCountGreaterOrEqualTo(FeatureFlagServiceTestsConstants.MinimumExpectedFlagCount); // Initial 6 + 1 new one
        allFlags.Should().Contain(f => f.Name == FeatureFlagServiceTestsConstants.AuditLoggingFeatureName && f.IsEnabled == true);
        allFlags.Should().Contain(f => f.Name == FeatureFlagServiceTestsConstants.AnotherFeatureName && f.IsEnabled == false);
    }
}
