#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;
using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Extension methods for <see cref="ServiceConfiguration"/> that provide additional functionality
/// for working with configuration values in common scenarios.
/// </summary>
public static class ServiceConfigurationExtensions
{
    /// <summary>
    /// Gets the configuration value as a double. Returns 0 if parsing fails.
    /// </summary>
    /// <param name="configuration">The service configuration</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    /// <returns>The parsed double value or 0 if parsing fails</returns>
    public static double GetDoubleValue(this ServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (double.TryParse(configuration.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return 0;
    }

    /// <summary>
    /// Gets the configuration value as a decimal. Returns 0 if parsing fails.
    /// </summary>
    /// <param name="configuration">The service configuration</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    /// <returns>The parsed decimal value or 0 if parsing fails</returns>
    public static decimal GetDecimalValue(this ServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (decimal.TryParse(configuration.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return 0;
    }

    /// <summary>
    /// Gets the configuration value as a DateTime. Returns DateTime.MinValue if parsing fails.
    /// </summary>
    /// <param name="configuration">The service configuration</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    /// <returns>The parsed DateTime value or DateTime.MinValue if parsing fails</returns>
    public static DateTime GetDateTimeValue(this ServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (DateTime.TryParse(configuration.Value, out var result))
        {
            return result;
        }

        return DateTime.MinValue;
    }

    /// <summary>
    /// Gets the configuration value as a Guid. Returns Guid.Empty if parsing fails.
    /// </summary>
    /// <param name="configuration">The service configuration</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    /// <returns>The parsed Guid value or Guid.Empty if parsing fails</returns>
    public static Guid GetGuidValue(this ServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (Guid.TryParse(configuration.Value, out var result))
        {
            return result;
        }

        return Guid.Empty;
    }

    /// <summary>
    /// Updates the configuration value only if the new value is different from the current value.
    /// This prevents unnecessary updates and preserves the UpdatedAt timestamp.
    /// </summary>
    /// <param name="configuration">The service configuration</param>
    /// <param name="newValue">The new value to set</param>
    /// <param name="userId">Optional user ID tracking who made the change</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    /// <returns>True if the value was updated, false if the value was the same</returns>
    public static bool UpdateValueIfChanged(this ServiceConfiguration configuration, string newValue, Guid? userId = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.Equals(configuration.Value, newValue, StringComparison.Ordinal))
        {
            return false;
        }

        configuration.UpdateValue(newValue, userId);
        return true;
    }

    /// <summary>
    /// Safely gets a configuration value with a fallback to a default value.
    /// Returns the default value if the configuration is null or the value is empty.
    /// </summary>
    /// <param name="configuration">The service configuration</param>
    /// <param name="defaultValue">The default value to return if configuration is null or empty</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    /// <returns>The configuration value or the default value</returns>
    public static string GetValueOrDefault(this ServiceConfiguration? configuration, string defaultValue = "")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return configuration.Value ?? defaultValue;
    }

    /// <summary>
    /// Determines if the configuration is a system configuration that should not be modified by users.
    /// </summary>
    /// <param name="configuration">The service configuration</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    /// <returns>True if this is a system configuration; otherwise false</returns>
    public static bool IsSystemConfiguration(this ServiceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return configuration.IsSystemConfig || configuration.Key.StartsWith("System.", StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the configuration value as an enum value of type T.
    /// </summary>
    /// <typeparam name="T">The enum type to parse</typeparam>
    /// <param name="configuration">The service configuration</param>
    /// <param name="defaultValue">The default value to return if parsing fails</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    /// <returns>The parsed enum value or the default value if parsing fails</returns>
    public static T GetEnumValue<T>(this ServiceConfiguration configuration, T defaultValue = default) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (Enum.TryParse<T>(configuration.Value, true, out var result))
        {
            return result;
        }

        return defaultValue;
    }
}