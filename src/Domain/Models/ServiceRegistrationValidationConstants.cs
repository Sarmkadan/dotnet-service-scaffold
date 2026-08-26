#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Holds the magic values used by <see cref="ServiceRegistrationValidation"/>.
/// </summary>
internal static class ServiceRegistrationValidationConstants
{
    public const int InitialErrorCapacity = 32;

    public const int MaxServiceNameLength = 255;
    public const int MaxVersionLength = 50;
    public const int MaxEndpointLength = 255;
    public const int MaxSystemdServiceNameLength = 500;

    public const int FutureDateTimeToleranceMinutes = 5;

    public const int MaxHealthCheckIntervalSeconds = 86400;
    public const int MaxTimeoutSeconds = 300;

    public const string ServiceNameRequiredError = "ServiceName is required and cannot be empty or whitespace.";
    public const string ServiceNameTooLongError = "ServiceName must be 255 characters or less.";
    public const string HealthCheckUrlRequiredError = "HealthCheckUrl is required and cannot be empty or whitespace.";
    public const string HealthCheckUrlInvalidError = "HealthCheckUrl must be a valid URI.";
    public const string VersionRequiredError = "Version is required and cannot be empty or whitespace.";
    public const string VersionTooLongError = "Version must be 50 characters or less.";
    public const string EndpointRequiredError = "Endpoint is required and cannot be empty or whitespace.";
    public const string EndpointTooLongError = "Endpoint must be 255 characters or less.";
    public const string OwnerIdRequiredError = "OwnerId is required and cannot be empty.";
    public const string CreatedAtRequiredError = "CreatedAt must be set to a valid DateTime.";
    public const string CreatedAtFutureError = "CreatedAt cannot be in the future.";
    public const string UpdatedAtRequiredError = "UpdatedAt must be set to a valid DateTime.";
    public const string UpdatedAtFutureError = "UpdatedAt cannot be in the future.";
    public const string UpdatedAtEarlierThanCreatedAtError = "UpdatedAt cannot be earlier than CreatedAt.";
    public const string LastHealthCheckAtFutureError = "LastHealthCheckAt cannot be in the future.";
    public const string LastHealthCheckAtEarlierThanCreatedAtError = "LastHealthCheckAt cannot be earlier than CreatedAt.";
    public const string HealthCheckIntervalSecondsPositiveError = "HealthCheckIntervalSeconds must be greater than zero.";
    public const string HealthCheckIntervalSecondsTooLargeError = "HealthCheckIntervalSeconds cannot exceed 86400 seconds (24 hours).";
    public const string TimeoutSecondsPositiveError = "TimeoutSeconds must be greater than zero.";
    public const string TimeoutSecondsTooLargeError = "TimeoutSeconds cannot exceed 300 seconds (5 minutes).";
    public const string ConsecutiveFailuresNegativeError = "ConsecutiveFailures cannot be negative.";
    public const string TotalRequestsNegativeError = "TotalRequests cannot be negative.";
    public const string SuccessfulRequestsNegativeError = "SuccessfulRequests cannot be negative.";
    public const string SuccessfulRequestsExceedTotalError = "SuccessfulRequests cannot exceed TotalRequests.";
    public const string SystemdServiceNameTooLongError = "SystemdServiceName must be 500 characters or less.";
    public const string EnabledWithDisabledStatusError = "A service cannot be enabled while having Disabled status.";
    public const string DisabledWithoutIsEnabledError = "A disabled service must have IsEnabled set to false.";
    public const string InvalidRegistrationFormat = "ServiceRegistration is invalid. Validation errors: {0}";
}