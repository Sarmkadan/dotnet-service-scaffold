#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service interface for configuration management and application settings.
/// </summary>
public interface IConfigurationService
{
    Task<ServiceConfiguration?> GetConfigurationAsync(string key, Guid? serviceId = null);

    Task<IEnumerable<ServiceConfiguration>> GetAllConfigurationsAsync();

    Task<IEnumerable<ServiceConfiguration>> GetServiceConfigurationsAsync(Guid serviceId);

    Task<ServiceConfiguration> SetConfigurationAsync(
        string key,
        string value,
        string? configType = null,
        Guid? serviceId = null,
        string? description = null);

    Task DeleteConfigurationAsync(string key, Guid? serviceId = null);

    Task<int> GetConfigIntAsync(string key, int defaultValue = 0);

    Task<bool> GetConfigBoolAsync(string key, bool defaultValue = false);

    Task<string> GetConfigStringAsync(string key, string defaultValue = "");

    Task<TimeSpan> GetConfigTimeSpanAsync(string key, TimeSpan? defaultValue = null);
}
