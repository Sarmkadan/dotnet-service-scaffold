#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Constant values used by <see cref="ServiceConfigurationExtensions"/>.
/// </summary>
internal static class ServiceConfigurationExtensionsConstants
{
    /// <summary>
    /// Default numeric value returned when parsing a configuration value fails.
    /// </summary>
    public const double DefaultNumericValue = 0;

    /// <summary>
    /// Default decimal value returned when parsing a configuration value fails.
    /// </summary>
    public const decimal DefaultDecimalValue = 0;

    /// <summary>
    /// Default string value returned when no configuration value is present.
    /// </summary>
    public const string DefaultStringValue = "";

    /// <summary>
    /// Prefix that marks a configuration key as a system configuration.
    /// </summary>
    public const string SystemKeyPrefix = "System.";

    /// <summary>
    /// Ordinal comparison used when comparing configuration keys and values.
    /// </summary>
    public static readonly StringComparison OrdinalComparison = StringComparison.Ordinal;
}