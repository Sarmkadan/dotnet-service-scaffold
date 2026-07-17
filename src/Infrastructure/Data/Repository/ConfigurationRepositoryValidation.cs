#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository;

/// <summary>
/// Provides validation helpers for <see cref="ConfigurationRepository"/> operations.
/// </summary>
public static class ConfigurationRepositoryValidation
{
    /// <summary>
    /// Validates a <see cref="ServiceConfiguration"/> entity for repository operations.
    /// </summary>
    /// <param name="value">The configuration entity to validate.</param>
    /// <returns>A list of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Key
        if (string.IsNullOrWhiteSpace(value.Key))
        {
            errors.Add("Configuration Key cannot be null or whitespace.");
        }
        else if (value.Key.Length > 255)
        {
            errors.Add("Configuration Key cannot exceed 255 characters.");
        }

        // Validate Value
        if (string.IsNullOrWhiteSpace(value.Value))
        {
            errors.Add("Configuration Value cannot be null or whitespace.");
        }
        else if (value.Value.Length > 4000)
        {
            errors.Add("Configuration Value cannot exceed 4000 characters.");
        }

        // Validate ConfigType if present
        if (!string.IsNullOrWhiteSpace(value.ConfigType) && value.ConfigType.Length > 50)
        {
            errors.Add("Configuration Type cannot exceed 50 characters.");
        }

        // Validate Description if present
        if (!string.IsNullOrWhiteSpace(value.Description) && value.Description.Length > 1000)
        {
            errors.Add("Configuration Description cannot exceed 1000 characters.");
        }

        // Validate timestamps
        if (value.CreatedAt == default)
        {
            errors.Add("Configuration CreatedAt must be set to a valid date.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Configuration CreatedAt cannot be in the future.");
        }

        if (value.UpdatedAt == default)
        {
            errors.Add("Configuration UpdatedAt must be set to a valid date.");
        }
        else if (value.UpdatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Configuration UpdatedAt cannot be in the future.");
        }

        // Validate ServiceId if present
        if (value.ServiceId.HasValue && value.ServiceId.Value == Guid.Empty)
        {
            errors.Add("Configuration ServiceId cannot be an empty GUID.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ServiceConfiguration"/> is valid.
    /// </summary>
    /// <param name="value">The configuration entity to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ServiceConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ServiceConfiguration"/> is valid.
    /// </summary>
    /// <param name="value">The configuration entity to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this ServiceConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Configuration validation failed:{Environment.NewLine}- {
                    string.Join(Environment.NewLine + "- ", errors)
                }",
                nameof(value));
        }
    }
}
