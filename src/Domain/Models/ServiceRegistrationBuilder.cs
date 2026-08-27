#nullable enable

using DotnetServiceScaffold.Domain.Enums;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Fluent builder for <see cref="ServiceRegistration"/> instances.
/// </summary>
public class ServiceRegistrationBuilder
{
    private Guid _id;
    private string? _serviceName;
    private string? _description;
    private string? _healthCheckUrl;
    private string? _version;
    private string? _endpoint;
    private ServiceStatus _status = ServiceStatus.Unknown;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;
    private DateTime? _lastHealthCheckAt;

    /// <summary>
    /// Sets the unique identifier for the service registration.
    /// </summary>
    /// <param name="id">The service identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceRegistrationBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the name of the service. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="serviceName">The service name.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serviceName"/> is null, empty, or whitespace.</exception>
    public ServiceRegistrationBuilder WithServiceName(string serviceName)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be whitespace.", nameof(serviceName));
        _serviceName = serviceName;
        return this;
    }

    /// <summary>
    /// Sets the description of the service.
    /// </summary>
    /// <param name="description">The service description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceRegistrationBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the health check URL for the service. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="healthCheckUrl">The health check URL.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="healthCheckUrl"/> is null, empty, or whitespace.</exception>
    public ServiceRegistrationBuilder WithHealthCheckUrl(string healthCheckUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(healthCheckUrl);
        if (string.IsNullOrWhiteSpace(healthCheckUrl))
            throw new ArgumentException("Health check URL cannot be whitespace.", nameof(healthCheckUrl));
        _healthCheckUrl = healthCheckUrl;
        return this;
    }

    /// <summary>
    /// Sets the version of the service. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="version">The service version.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="version"/> is null, empty, or whitespace.</exception>
    public ServiceRegistrationBuilder WithVersion(string version)
    {
        ArgumentException.ThrowIfNullOrEmpty(version);
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version cannot be whitespace.", nameof(version));
        _version = version;
        return this;
    }

    /// <summary>
    /// Sets the endpoint of the service. Must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="endpoint">The service endpoint.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endpoint"/> is null, empty, or whitespace.</exception>
    public ServiceRegistrationBuilder WithEndpoint(string endpoint)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be whitespace.", nameof(endpoint));
        _endpoint = endpoint;
        return this;
    }

    /// <summary>
    /// Sets the current status of the service.
    /// </summary>
    /// <param name="status">The service status.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceRegistrationBuilder WithStatus(ServiceStatus status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets the timestamp when the service registration was created.
    /// </summary>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceRegistrationBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the timestamp when the service registration was last updated.
    /// </summary>
    /// <param name="updatedAt">The update timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceRegistrationBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the last health check performed on the service.
    /// </summary>
    /// <param name="lastHealthCheckAt">The last health check timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceRegistrationBuilder WithLastHealthCheckAt(DateTime? lastHealthCheckAt)
    {
        _lastHealthCheckAt = lastHealthCheckAt;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="ServiceRegistration"/>.
    /// </summary>
    /// <param name="template">The service registration to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static ServiceRegistrationBuilder From(ServiceRegistration template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new ServiceRegistrationBuilder()
            .WithId(template.Id)
            .WithServiceName(template.ServiceName)
            .WithDescription(template.Description)
            .WithHealthCheckUrl(template.HealthCheckUrl)
            .WithVersion(template.Version)
            .WithEndpoint(template.Endpoint)
            .WithStatus(template.Status)
            .WithCreatedAt(template.CreatedAt)
            .WithUpdatedAt(template.UpdatedAt)
            .WithLastHealthCheckAt(template.LastHealthCheckAt);
    }

    /// <summary>
    /// Builds the <see cref="ServiceRegistration"/> instance with the current values.
    /// </summary>
    /// <returns>A fully configured <see cref="ServiceRegistration"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public ServiceRegistration Build()
    {
        // Validate required properties
        if (string.IsNullOrWhiteSpace(_serviceName))
            throw new ArgumentException("Service name is required.", nameof(_serviceName));
        if (string.IsNullOrWhiteSpace(_healthCheckUrl))
            throw new ArgumentException("Health check URL is required.", nameof(_healthCheckUrl));
        if (string.IsNullOrWhiteSpace(_version))
            throw new ArgumentException("Version is required.", nameof(_version));
        if (string.IsNullOrWhiteSpace(_endpoint))
            throw new ArgumentException("Endpoint is required.", nameof(_endpoint));

        return new ServiceRegistration
        {
            Id = _id,
            ServiceName = _serviceName!,
            Description = _description,
            HealthCheckUrl = _healthCheckUrl!,
            Version = _version!,
            Endpoint = _endpoint!,
            Status = _status,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            LastHealthCheckAt = _lastHealthCheckAt
        };
    }
}