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

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="configRepository">The configuration repository.</param>
    /// <param name="logger">The logger.</param>
    public ConfigurationService(IConfigurationRepository configRepository, ILogger<ConfigurationService> logger)
    {
        ArgumentNullException.ThrowIfNull(configRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _configRepository = configRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a configuration by its key and optional service identifier.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="serviceId">The optional service identifier.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the key is null or empty.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while retrieving the configuration.</exception>
    public async Task<ServiceConfiguration?> GetConfigurationAsync(string key, Guid? serviceId = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key), "Configuration key cannot be empty");

        try
        {
            return await _configRepository.GetByKeyAsync(key, serviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration: {Key}", key);
            throw new DataAccessException($"Error retrieving configuration: {key}", ex);
        }
    }

    /// <summary>
    /// Retrieves all configurations.
    /// </summary>
    /// <returns>An enumerable collection of all configurations.</returns>
    /// <exception cref="DataAccessException">Thrown when an error occurs while retrieving the configurations.</exception>
    public async Task<IEnumerable<ServiceConfiguration>> GetAllConfigurationsAsync()
    {
        try
        {
            return await _configRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all configurations");
            throw new DataAccessException("Error retrieving all configurations", ex);
        }
    }

    /// <summary>
    /// Retrieves all configurations for a specific service.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns>An enumerable collection of configurations for the specified service.</returns>
    /// <exception cref="DataAccessException">Thrown when an error occurs while retrieving the configurations.</exception>
    public async Task<IEnumerable<ServiceConfiguration>> GetServiceConfigurationsAsync(Guid serviceId)
    {
        try
        {
            return await _configRepository.GetByServiceIdAsync(serviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configurations for service: {ServiceId}", serviceId);
            throw new DataAccessException($"Error retrieving configurations for service: {serviceId}", ex);
        }
    }

    /// <summary>
    /// Sets a configuration value. If a configuration with the same key and serviceId exists, it updates it; otherwise, it creates a new one.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value.</param>
    /// <param name="configType">The configuration type.</param>
    /// <param name="serviceId">The optional service identifier.</param>
    /// <param name="description">The optional description.</param>
    /// <returns>The configuration that was set.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the key or value is null or empty.</exception>
    /// <exception cref="ConfigurationException">Thrown when the configuration value is invalid for the given type.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while accessing the data.</exception>
    public async Task<ServiceConfiguration> SetConfigurationAsync(
        string key,
        string value,
        string? configType = null,
        Guid? serviceId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key), "Configuration key cannot be empty");
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value), "Configuration value cannot be empty");

        try
        {
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
                throw new ConfigurationException($"Invalid value for configuration type {configType}");
            }

            var created = await _configRepository.AddAsync(config);
            _logger.LogInformation("Configuration created: {Key} for service {ServiceId}", key, serviceId);
            return created;
        }
        catch (ConfigurationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting configuration: {Key}", key);
            throw new DataAccessException($"Error setting configuration: {key}", ex);
        }
    }

    /// <summary>
    /// Deletes a configuration by its key and optional service identifier.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="serviceId">The optional service identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when the key is null or empty.</exception>
    /// <exception cref="DataAccessException">Thrown when an error occurs while deleting the configuration.</exception>
    public async Task DeleteConfigurationAsync(string key, Guid? serviceId = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key), "Configuration key cannot be empty");

        try
        {
            await _configRepository.DeleteByKeyAsync(key, serviceId);
            _logger.LogInformation("Configuration deleted: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration: {Key}", key);
            throw new DataAccessException($"Error deleting configuration: {key}", ex);
        }
    }

    /// <summary>
    /// Retrieves a configuration value as an integer.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value to return if the configuration is not found or cannot be parsed.</param>
    /// <returns>The configuration value as an integer, or the default value if not found.</returns>
    public async Task<int> GetConfigIntAsync(string key, int defaultValue = 0)
    {
        var config = await GetConfigurationAsync(key);
        return config?.GetIntValue(defaultValue) ?? defaultValue;
    }

    /// <summary>
    /// Retrieves a configuration value as a boolean.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value to return if the configuration is not found or cannot be parsed.</param>
    /// <returns>The configuration value as a boolean, or the default value if not found.</returns>
    public async Task<bool> GetConfigBoolAsync(string key, bool defaultValue = false)
    {
        var config = await GetConfigurationAsync(key);
        return config?.GetBoolValue(defaultValue) ?? defaultValue;
    }

    /// <summary>
    /// Retrieves a configuration value as a string.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value to return if the configuration is not found.</param>
    /// <returns>The configuration value as a string, or the default value if not found.</returns>
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
