#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
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

        var errors = new List<string>();

        // Required string properties
        if (string.IsNullOrWhiteSpace(value.ServiceName))
        {
            errors.Add("ServiceName is required and cannot be empty or whitespace.");
        }
        else if (value.ServiceName.Length > 255)
        {
            errors.Add("ServiceName must be 255 characters or less.");
        }

        if (string.IsNullOrWhiteSpace(value.HealthCheckUrl))
        {
            errors.Add("HealthCheckUrl is required and cannot be empty or whitespace.");
        }
        else if (!Uri.TryCreate(value.HealthCheckUrl, UriKind.Absolute, out _) &&
                 !Uri.TryCreate(value.HealthCheckUrl, UriKind.Relative, out _))
        {
            errors.Add("HealthCheckUrl must be a valid URI.");
        }

        if (string.IsNullOrWhiteSpace(value.Version))
        {
            errors.Add("Version is required and cannot be empty or whitespace.");
        }
        else if (value.Version.Length > 50)
        {
            errors.Add("Version must be 50 characters or less.");
        }

        if (string.IsNullOrWhiteSpace(value.Endpoint))
        {
            errors.Add("Endpoint is required and cannot be empty or whitespace.");
        }
        else if (value.Endpoint.Length > 255)
        {
            errors.Add("Endpoint must be 255 characters or less.");
        }

        // Required Guid properties
        if (value.OwnerId == Guid.Empty)
        {
            errors.Add("OwnerId is required and cannot be empty.");
        }

        // DateTime properties
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must be set to a valid DateTime.");
        }
        else if (value.UpdatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("UpdatedAt cannot be in the future.");
        }
        else if (value.UpdatedAt < value.CreatedAt)
        {
            errors.Add("UpdatedAt cannot be earlier than CreatedAt.");
        }

        if (value.LastHealthCheckAt.HasValue)
        {
            if (value.LastHealthCheckAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("LastHealthCheckAt cannot be in the future.");
            }
            else if (value.LastHealthCheckAt.Value < value.CreatedAt)
            {
                errors.Add("LastHealthCheckAt cannot be earlier than CreatedAt.");
            }
        }

        // Numeric properties with business rules
        if (value.HealthCheckIntervalSeconds <= 0)
        {
            errors.Add("HealthCheckIntervalSeconds must be greater than zero.");
        }
        else if (value.HealthCheckIntervalSeconds > 86400) // 24 hours
        {
            errors.Add("HealthCheckIntervalSeconds cannot exceed 86400 seconds (24 hours).");
        }

        if (value.TimeoutSeconds <= 0)
        {
            errors.Add("TimeoutSeconds must be greater than zero.");
        }
        else if (value.TimeoutSeconds > 300) // 5 minutes
        {
            errors.Add("TimeoutSeconds cannot exceed 300 seconds (5 minutes).");
        }

        // Business logic validations
        if (value.ConsecutiveFailures < 0)
        {
            errors.Add("ConsecutiveFailures cannot be negative.");
        }

        if (value.TotalRequests < 0)
        {
            errors.Add("TotalRequests cannot be negative.");
        }

        if (value.SuccessfulRequests < 0)
        {
            errors.Add("SuccessfulRequests cannot be negative.");
        }

        if (value.SuccessfulRequests > value.TotalRequests)
        {
            errors.Add("SuccessfulRequests cannot exceed TotalRequests.");
        }

        // SystemdServiceName length validation
        if (!string.IsNullOrEmpty(value.SystemdServiceName) && value.SystemdServiceName.Length > 500)
        {
            errors.Add("SystemdServiceName must be 500 characters or less.");
        }

        // Cross-field validation
        if (value.IsEnabled && value.Status == ServiceStatus.Disabled)
        {
            errors.Add("A service cannot be enabled while having Disabled status.");
        }

        if (value.Status == ServiceStatus.Disabled && !value.IsEnabled)
        {
            errors.Add("A disabled service must have IsEnabled set to false.");
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
    public static bool IsValid(this ServiceRegistration? value)
    {
        return value?.Validate().Count == 0;
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
            $"ServiceRegistration is invalid. Validation errors: {string.Join(" ", errors)}");
    }
}