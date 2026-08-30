#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Constants for ServiceConfiguration class.
/// </summary>
internal static class ServiceConfigurationConstants
{
    /// <summary>
    /// Maximum length for configuration key and type.
    /// </summary>
    public const int MaxKeyLength = 255;
    public const int MaxConfigTypeLength = 255;

    /// <summary>
    /// Maximum length for configuration value.
    /// </summary>
    public const int MaxValueLength = 4000;

    /// <summary>
    /// Maximum length for configuration description.
    /// </summary>
    public const int MaxDescriptionLength = 1000;

    /// <summary>
    /// String representation of true for boolean configuration values.
    /// </summary>
    public const string BooleanTrueString = "1";

    /// <summary>
    /// The redacted value for sensitive configuration keys.
    /// </summary>
    public const string RedactedValue = "***REDACTED***";
}