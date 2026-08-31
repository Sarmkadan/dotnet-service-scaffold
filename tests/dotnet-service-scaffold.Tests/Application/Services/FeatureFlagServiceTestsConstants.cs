#nullable enable

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Constants for FeatureFlagServiceTests.
/// </summary>
internal static class FeatureFlagServiceTestsConstants
{
    /// <summary>
    /// The audit logging feature name used in tests.
    /// </summary>
    public const string AuditLoggingFeatureName = "audit_logging";

    /// <summary>
    /// The rate limiting feature name used in tests.
    /// </summary>
    public const string RateLimitingFeatureName = "rate_limiting";

    /// <summary>
    /// The advanced analytics feature name used in tests.
    /// </summary>
    public const string AdvancedAnalyticsFeatureName = "advanced_analytics";

    /// <summary>
    /// The health checks feature name used in tests.
    /// </summary>
    public const string HealthChecksFeatureName = "health_checks";

    /// <summary>
    /// The new cool feature name used in tests.
    /// </summary>
    public const string NewCoolFeatureName = "new_cool_feature";

    /// <summary>
    /// The another feature name used in tests.
    /// </summary>
    public const string AnotherFeatureName = "another_feature";

    /// <summary>
    /// The non-existent feature name used in tests.
    /// </summary>
    public const string NonExistentFeatureName = "non_existent_feature";

    /// <summary>
    /// The description for the new cool feature.
    /// </summary>
    public const string NewCoolFeatureDescription = "A brand new feature";

    /// <summary>
    /// The description for another feature.
    /// </summary>
    public const string AnotherFeatureDescription = "Just another feature";

    /// <summary>
    /// The valid rollout percentage used in tests.
    /// </summary>
    public const int ValidRolloutPercentage = 50;

    /// <summary>
    /// The invalid low rollout percentage used in tests.
    /// </summary>
    public const int InvalidLowRolloutPercentage = -1;

    /// <summary>
    /// The invalid high rollout percentage used in tests.
    /// </summary>
    public const int InvalidHighRolloutPercentage = 101;

    /// <summary>
    /// The minimum expected flag count for GetAllFlags test.
    /// </summary>
    public const int MinimumExpectedFlagCount = 7;

    /// <summary>
    /// The expected number of matching log invocations.
    /// </summary>
    public const int ExpectedLogInvocationCount = 1;

    /// <summary>
    /// The expected exception message pattern for an invalid rollout percentage.
    /// </summary>
    public const string InvalidRolloutPercentageMessagePattern = "Rollout percentage must be between 0 and 100*";

    /// <summary>
    /// The log message format for feature not found warning.
    /// </summary>
    public const string FeatureNotFoundLogFormat = "Feature flag '{FeatureName}' not found, defaulting to false";

    /// <summary>
    /// The log message format for feature enabled information.
    /// </summary>
    public const string FeatureEnabledLogFormat = "Feature '{FeatureName}' enabled";

    /// <summary>
    /// The log message format for feature disabled information.
    /// </summary>
    public const string FeatureDisabledLogFormat = "Feature '{FeatureName}' disabled";

    /// <summary>
    /// The log message format for rollout percentage set information.
    /// </summary>
    public const string RolloutPercentageSetLogFormat = "Feature '{FeatureName}' rollout percentage set to {Percentage}%";

    /// <summary>
    /// The log message format for feature registered information.
    /// </summary>
    public const string FeatureRegisteredLogFormat = "Feature '{FeatureName}' registered (enabled: {Enabled})";
}
