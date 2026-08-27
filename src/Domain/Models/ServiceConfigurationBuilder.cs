#nullable enable

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Fluent builder for <see cref="ServiceConfiguration"/> instances.
/// </summary>
public class ServiceConfigurationBuilder
{
    private Guid _id;
    private string? _key;
    private string? _value;
    private string? _configType;
    private Guid? _serviceId;
    private ServiceRegistration? _service;
    private bool _isEncrypted;
    private bool _isSystemConfig;
    private string? _description;
    private DateTime _createdAt = DateTime.UtcNow;

    /// <summary>
    /// Sets the unique identifier for the service configuration.
    /// </summary>
    /// <param name="id">The configuration identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceConfigurationBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the configuration key. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null, empty, or whitespace.</exception>
    public ServiceConfigurationBuilder WithKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key cannot be whitespace.", nameof(key));
        _key = key;
        return this;
    }

    /// <summary>
    /// Sets the configuration value. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The configuration value.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null, empty, or whitespace.</exception>
    public ServiceConfigurationBuilder WithValue(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Configuration value cannot be whitespace.", nameof(value));
        _value = value;
        return this;
    }

    /// <summary>
    /// Sets the configuration type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceConfigurationBuilder WithConfigType(string? configType)
    {
        _configType = configType;
        return this;
    }

    /// <summary>
    /// Sets the service identifier.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceConfigurationBuilder WithServiceId(Guid? serviceId)
    {
        _serviceId = serviceId;
        return this;
    }

    /// <summary>
    /// Sets the service registration.
    /// </summary>
    /// <param name="service">The service registration.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceConfigurationBuilder WithService(ServiceRegistration? service)
    {
        _service = service;
        return this;
    }

    /// <summary>
    /// Sets whether the configuration value is encrypted.
    /// </summary>
    /// <param name="isEncrypted">True if the configuration value is encrypted.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceConfigurationBuilder WithIsEncrypted(bool isEncrypted)
    {
        _isEncrypted = isEncrypted;
        return this;
    }

    /// <summary>
    /// Sets whether the configuration is a system configuration.
    /// </summary>
    /// <param name="isSystemConfig">True if the configuration is a system configuration.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceConfigurationBuilder WithIsSystemConfig(bool isSystemConfig)
    {
        _isSystemConfig = isSystemConfig;
        return this;
    }

    /// <summary>
    /// Sets the configuration description.
    /// </summary>
    /// <param name="description">The configuration description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceConfigurationBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the timestamp when the service configuration was created.
    /// </summary>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceConfigurationBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="ServiceConfiguration"/>.
    /// </summary>
    /// <param name="template">The service configuration to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static ServiceConfigurationBuilder From(ServiceConfiguration template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new ServiceConfigurationBuilder()
            .WithId(template.Id)
            .WithKey(template.Key)
            .WithValue(template.Value)
            .WithConfigType(template.ConfigType)
            .WithServiceId(template.ServiceId)
            .WithService(template.Service)
            .WithIsEncrypted(template.IsEncrypted)
            .WithIsSystemConfig(template.IsSystemConfig)
            .WithDescription(template.Description)
            .WithCreatedAt(template.CreatedAt);
    }

    /// <summary>
    /// Builds the <see cref="ServiceConfiguration"/> instance with the current values.
    /// </summary>
    /// <returns>A fully configured <see cref="ServiceConfiguration"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public ServiceConfiguration Build()
    {
        // Validate required properties
        if (string.IsNullOrWhiteSpace(_key))
            throw new ArgumentException("Configuration key is required.", nameof(_key));
        if (string.IsNullOrWhiteSpace(_value))
            throw new ArgumentException("Configuration value is required.", nameof(_value));

        return new ServiceConfiguration
        {
            Id = _id,
            Key = _key!,
            Value = _value!,
            ConfigType = _configType,
            ServiceId = _serviceId,
            Service = _service,
            IsEncrypted = _isEncrypted,
            IsSystemConfig = _isSystemConfig,
            Description = _description,
            CreatedAt = _createdAt
        };
    }
}