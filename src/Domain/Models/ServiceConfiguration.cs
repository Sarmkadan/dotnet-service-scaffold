#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Stores configuration parameters for services and the platform.
/// </summary>
public class ServiceConfiguration
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(255)]
    public required string Key { get; set; }

    [Required]
    [StringLength(4000)]
    public required string Value { get; set; }

    [StringLength(255)]
    public string? ConfigType { get; set; }

    public Guid? ServiceId { get; set; }

    [ForeignKey(nameof(Service))]
    public ServiceRegistration? Service { get; set; }

    public bool IsEncrypted { get; set; }

    public bool IsSystemConfig { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UpdatedByUserId { get; set; }

    /// <summary>
    /// Parses the configuration value as an integer.
    /// </summary>
    public int GetIntValue(int defaultValue = 0)
    {
        if (int.TryParse(Value, out var result))
            return result;

        return defaultValue;
    }

    /// <summary>
    /// Parses the configuration value as a boolean.
    /// </summary>
    public bool GetBoolValue(bool defaultValue = false)
    {
        if (bool.TryParse(Value, out var result))
            return result;

        if (Value.Equals("1") || Value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            Value.Equals("yes", StringComparison.OrdinalIgnoreCase))
            return true;

        return defaultValue;
    }

    /// <summary>
    /// Parses the configuration value as a TimeSpan.
    /// </summary>
    public TimeSpan GetTimeSpanValue(TimeSpan? defaultValue = null)
    {
        if (TimeSpan.TryParse(Value, CultureInfo.InvariantCulture, out var result))
            return result;

        return defaultValue ?? TimeSpan.Zero;
    }

    /// <summary>
    /// Validates the configuration value matches the expected type.
    /// </summary>
    public bool ValidateValue()
    {
        if (string.IsNullOrWhiteSpace(Value))
            return false;

        return ConfigType switch
        {
            "integer" => int.TryParse(Value, out _),
            "boolean" => bool.TryParse(Value, out _) || Value == "1" || Value == "0",
            "timespan" => TimeSpan.TryParse(Value, out _),
            "url" => Uri.TryCreate(Value, UriKind.Absolute, out _),
            "string" => true,
            _ => true
        };
    }

    /// <summary>
    /// Masks sensitive values like API keys or passwords for logging.
    /// </summary>
    public string GetMaskedValue()
    {
        var lowerKey = Key.ToLower();

        if (lowerKey.Contains("password") || lowerKey.Contains("secret") ||
            lowerKey.Contains("key") || lowerKey.Contains("token"))
        {
            return "***REDACTED***";
        }

        return Value;
    }

    /// <summary>
    /// Updates the configuration value and tracks when it was changed.
    /// </summary>
    public void UpdateValue(string newValue, Guid? userId = null)
    {
        Value = newValue;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}
