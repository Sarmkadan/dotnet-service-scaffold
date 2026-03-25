#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Service for managing application and service configurations.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configRepository;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(IConfigurationRepository configRepository, ILogger<ConfigurationService> logger)
    {
        _configRepository = configRepository;
        _logger = logger;
    }

    public async Task<ServiceConfiguration?> GetConfigurationAsync(string key, Guid? serviceId = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ServiceValidationException("Configuration key cannot be empty");

        return await _configRepository.GetByKeyAsync(key, serviceId);
    }

    public async Task<IEnumerable<ServiceConfiguration>> GetAllConfigurationsAsync()
    {
        return await _configRepository.GetAllAsync();
    }

    public async Task<IEnumerable<ServiceConfiguration>> GetServiceConfigurationsAsync(Guid serviceId)
    {
        return await _configRepository.GetByServiceIdAsync(serviceId);
    }

    public async Task<ServiceConfiguration> SetConfigurationAsync(
        string key,
        string value,
        string? configType = null,
        Guid? serviceId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            throw new ServiceValidationException("Configuration key and value are required");

        var existing = await _configRepository.GetByKeyAsync(key, serviceId);

        if (existing is not null)
        {
            existing.UpdateValue(value);
            existing.ConfigType = configType;
            existing.Description = description;
            var updated = await _configRepository.UpdateAsync(existing);

            _logger.LogInformation("Configuration updated: {Key} for service {ServiceId}", key, serviceId);
            return updated;
        }

        var config = new ServiceConfiguration
        {
            Id = Guid.NewGuid(),
            Key = key,
            Value = value,
            ConfigType = configType,
            ServiceId = serviceId,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!config.ValidateValue())
        {
            _logger.LogWarning("Configuration validation failed: {Key} = {Value}", key, value);
            throw new ServiceValidationException($"Invalid value for configuration type {configType}");
        }

        var created = await _configRepository.AddAsync(config);
        _logger.LogInformation("Configuration created: {Key} for service {ServiceId}", key, serviceId);
        return created;
    }

    public async Task DeleteConfigurationAsync(string key, Guid? serviceId = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ServiceValidationException("Configuration key cannot be empty");

        await _configRepository.DeleteByKeyAsync(key, serviceId);
        _logger.LogInformation("Configuration deleted: {Key}", key);
    }

    public async Task<int> GetConfigIntAsync(string key, int defaultValue = 0)
    {
        var config = await GetConfigurationAsync(key);
        return config?.GetIntValue(defaultValue) ?? defaultValue;
    }

    public async Task<bool> GetConfigBoolAsync(string key, bool defaultValue = false)
    {
        var config = await GetConfigurationAsync(key);
        return config?.GetBoolValue(defaultValue) ?? defaultValue;
    }

    public async Task<string> GetConfigStringAsync(string key, string defaultValue = "")
    {
        var config = await GetConfigurationAsync(key);
        return config?.Value ?? defaultValue;
    }

    public async Task<TimeSpan> GetConfigTimeSpanAsync(string key, TimeSpan? defaultValue = null)
    {
        var config = await GetConfigurationAsync(key);
        return config?.GetTimeSpanValue(defaultValue) ?? (defaultValue ?? TimeSpan.Zero);
    }
}
