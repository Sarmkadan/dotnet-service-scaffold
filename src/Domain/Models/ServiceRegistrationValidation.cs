#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;
using DotnetServiceScaffold.Domain.Enums;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ServiceRegistration"/> instances.
/// </summary>
public static class ServiceRegistrationValidation
{
    /// <summary>
    /// Validates a <see cref="ServiceRegistration"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The service registration to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceRegistration? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>(ServiceRegistrationValidationConstants.InitialErrorCapacity);

        // Required string properties
        if (string.IsNullOrWhiteSpace(value.ServiceName))
        {
            errors.Add(ServiceRegistrationValidationConstants.ServiceNameRequiredError);
        }
        else if (value.ServiceName.Length > ServiceRegistrationValidationConstants.MaxServiceNameLength)
        {
            errors.Add(ServiceRegistrationValidationConstants.ServiceNameTooLongError);
        }

        if (string.IsNullOrWhiteSpace(value.HealthCheckUrl))
        {
            errors.Add(ServiceRegistrationValidationConstants.HealthCheckUrlRequiredError);
        }
        else if (!Uri.TryCreate(value.HealthCheckUrl, UriKind.Absolute, out _) &&
                 !Uri.TryCreate(value.HealthCheckUrl, UriKind.Relative, out _))
        {
            errors.Add(ServiceRegistrationValidationConstants.HealthCheckUrlInvalidError);
        }

        if (string.IsNullOrWhiteSpace(value.Version))
        {
            errors.Add(ServiceRegistrationValidationConstants.VersionRequiredError);
        }
        else if (value.Version.Length > ServiceRegistrationValidationConstants.MaxVersionLength)
        {
            errors.Add(ServiceRegistrationValidationConstants.VersionTooLongError);
        }

        if (string.IsNullOrWhiteSpace(value.Endpoint))
        {
            errors.Add(ServiceRegistrationValidationConstants.EndpointRequiredError);
        }
        else if (value.Endpoint.Length > ServiceRegistrationValidationConstants.MaxEndpointLength)
        {
            errors.Add(ServiceRegistrationValidationConstants.EndpointTooLongError);
        }

        // Required Guid properties
        if (value.OwnerId == Guid.Empty)
        {
            errors.Add(ServiceRegistrationValidationConstants.OwnerIdRequiredError);
        }

        // DateTime properties
        if (value.CreatedAt == default)
        {
            errors.Add(ServiceRegistrationValidationConstants.CreatedAtRequiredError);
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(ServiceRegistrationValidationConstants.FutureDateTimeToleranceMinutes))
        {
            errors.Add(ServiceRegistrationValidationConstants.CreatedAtFutureError);
        }

        if (value.UpdatedAt == default)
        {
            errors.Add(ServiceRegistrationValidationConstants.UpdatedAtRequiredError);
        }
        else if (value.UpdatedAt > DateTime.UtcNow.AddMinutes(ServiceRegistrationValidationConstants.FutureDateTimeToleranceMinutes))
        {
            errors.Add(ServiceRegistrationValidationConstants.UpdatedAtFutureError);
        }
        else if (value.UpdatedAt < value.CreatedAt)
        {
            errors.Add(ServiceRegistrationValidationConstants.UpdatedAtEarlierThanCreatedAtError);
        }

        if (value.LastHealthCheckAt.HasValue)
        {
            if (value.LastHealthCheckAt.Value > DateTime.UtcNow.AddMinutes(ServiceRegistrationValidationConstants.FutureDateTimeToleranceMinutes))
            {
                errors.Add(ServiceRegistrationValidationConstants.LastHealthCheckAtFutureError);
            }
            else if (value.LastHealthCheckAt.Value < value.CreatedAt)
            {
                errors.Add(ServiceRegistrationValidationConstants.LastHealthCheckAtEarlierThanCreatedAtError);
            }
        }

        // Numeric properties with business rules
        if (value.HealthCheckIntervalSeconds <= 0)
        {
            errors.Add(ServiceRegistrationValidationConstants.HealthCheckIntervalSecondsPositiveError);
        }
        else if (value.HealthCheckIntervalSeconds > ServiceRegistrationValidationConstants.MaxHealthCheckIntervalSeconds)
        {
            errors.Add(ServiceRegistrationValidationConstants.HealthCheckIntervalSecondsTooLargeError);
        }

        if (value.TimeoutSeconds <= 0)
        {
            errors.Add(ServiceRegistrationValidationConstants.TimeoutSecondsPositiveError);
        }
        else if (value.TimeoutSeconds > ServiceRegistrationValidationConstants.MaxTimeoutSeconds)
        {
            errors.Add(ServiceRegistrationValidationConstants.TimeoutSecondsTooLargeError);
        }

        // Business logic validations
        if (value.ConsecutiveFailures < 0)
        {
            errors.Add(ServiceRegistrationValidationConstants.ConsecutiveFailuresNegativeError);
        }

        if (value.TotalRequests < 0)
        {
            errors.Add(ServiceRegistrationValidationConstants.TotalRequestsNegativeError);
        }

        if (value.SuccessfulRequests < 0)
        {
            errors.Add(ServiceRegistrationValidationConstants.SuccessfulRequestsNegativeError);
        }
        else if (value.SuccessfulRequests > value.TotalRequests)
        {
            errors.Add(ServiceRegistrationValidationConstants.SuccessfulRequestsExceedTotalError);
        }

        // SystemdServiceName length validation
        if (!string.IsNullOrEmpty(value.SystemdServiceName) && value.SystemdServiceName.Length > ServiceRegistrationValidationConstants.MaxSystemdServiceNameLength)
        {
            errors.Add(ServiceRegistrationValidationConstants.SystemdServiceNameTooLongError);
        }

        // Cross-field validation
        if (value.IsEnabled && value.Status == ServiceStatus.Disabled)
        {
            errors.Add(ServiceRegistrationValidationConstants.EnabledWithDisabledStatusError);
        }

        if (value.Status == ServiceStatus.Disabled && !value.IsEnabled)
        {
            errors.Add(ServiceRegistrationValidationConstants.DisabledWithoutIsEnabledError);
        }

        // Navigation properties are validated separately if needed
        // (HealthCheckResults, Metrics, Events collections are initialized by default)

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ServiceRegistration"/> instance is valid.
    /// </summary>
    /// <param name="value">The service registration to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ServiceRegistration? value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ServiceRegistration"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The service registration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this ServiceRegistration? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            string.Format(ServiceRegistrationValidationConstants.InvalidRegistrationFormat, string.Join(" ", errors)));
    }
}