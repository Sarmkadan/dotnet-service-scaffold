#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Interface for service configuration.
/// </summary>
public interface IServiceConfiguration
{
    Guid Id { get; set; }
    string Key { get; set; }
    string Value { get; set; }
    string? ConfigType { get; set; }
    Guid? ServiceId { get; set; }
    ServiceRegistration? Service { get; set; }
    bool IsEncrypted { get; set; }
    bool IsSystemConfig { get; set; }
    string? Description { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    Guid? UpdatedByUserId { get; set; }
    int GetIntValue(int defaultValue = 0);
    bool GetBoolValue(bool defaultValue = false);
    TimeSpan GetTimeSpanValue(TimeSpan? defaultValue = null);
    bool ValidateValue();
    string GetMaskedValue();
    void UpdateValue(string newValue, Guid? userId = null);
}