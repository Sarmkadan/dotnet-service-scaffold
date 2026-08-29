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
/// Interface for FeatureFlagServiceTests to enable test extraction and mocking.
/// </summary>
public interface IFeatureFlagServiceTests
{
    void IsEnabled_ShouldReturnTrue_WhenFeatureIsEnabled();
    void IsEnabled_ShouldReturnFalse_WhenFeatureIsDisabled();
    void IsEnabled_ShouldReturnFalse_WhenFeatureNotFound();
    void EnableFeature_ShouldSetFeatureToEnabled();
    void DisableFeature_ShouldSetFeatureToDisabled();
    void SetRolloutPercentage_ShouldUpdatePercentage();
    void SetRolloutPercentage_ShouldThrowArgumentException_ForInvalidPercentage(int invalidPercentage);
    void RegisterFeature_ShouldAddNewFeature();
    void GetAllFlags_ShouldReturnAllRegisteredFlags();
}