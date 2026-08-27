#nullable enable

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Fluent builder for <see cref="ServiceDiscoveryRecord"/> instances.
/// </summary>
public sealed class ServiceDiscoveryRecordBuilder
{
    private Guid _instanceId = Guid.NewGuid();
    private string _serviceName = default!;
    private string? _version;
    private string _host = default!;
    private int _port;
    private string _scheme = "https";
    private int _weight = 10;
    private int _priority;
    private DiscoveryHealthStatus _healthStatus = DiscoveryHealthStatus.Unknown;
    private DiscoverySource _source = DiscoverySource.Unknown;

    /// <summary>
    /// Sets the unique identifier for this service instance.
    /// </summary>
    /// <param name="instanceId">The instance identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceDiscoveryRecordBuilder WithInstanceId(Guid instanceId)
    {
        _instanceId = instanceId;
        return this;
    }

    /// <summary>
    /// Sets the logical service name used for discovery lookups.
    /// </summary>
    /// <param name="serviceName">The service name. Must not be null or empty.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serviceName"/> is null or empty.</exception>
    public ServiceDiscoveryRecordBuilder WithServiceName(string serviceName)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        _serviceName = serviceName;
        return this;
    }

    /// <summary>
    /// Sets the semantic version advertised by this instance.
    /// </summary>
    /// <param name="version">The version string.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceDiscoveryRecordBuilder WithVersion(string? version)
    {
        _version = version;
        return this;
    }

    /// <summary>
    /// Sets the host name or IP address of this instance.
    /// </summary>
    /// <param name="host">The host. Must not be null or empty.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="host"/> is null or empty.</exception>
    public ServiceDiscoveryRecordBuilder WithHost(string host)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);
        _host = host;
        return this;
    }

    /// <summary>
    /// Sets the TCP port this instance is listening on.
    /// </summary>
    /// <param name="port">The port number (1-65535).</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceDiscoveryRecordBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    /// <summary>
    /// Sets the URI scheme (http, https, grpc, tcp).
    /// </summary>
    /// <param name="scheme">The scheme string.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceDiscoveryRecordBuilder WithScheme(string scheme)
    {
        _scheme = scheme;
        return this;
    }

    /// <summary>
    /// Sets the relative weight used for weighted load balancing.
    /// </summary>
    /// <param name="weight">The weight value (1-100).</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceDiscoveryRecordBuilder WithWeight(int weight)
    {
        _weight = weight;
        return this;
    }

    /// <summary>
    /// Sets the failover priority. Lower values are preferred when the load-balancing strategy is priority-based.
    /// </summary>
    /// <param name="priority">The priority value.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceDiscoveryRecordBuilder WithPriority(int priority)
    {
        _priority = priority;
        return this;
    }

    /// <summary>
    /// Sets the current health evaluation for this instance.
    /// </summary>
    /// <param name="healthStatus">The health status.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceDiscoveryRecordBuilder WithHealthStatus(DiscoveryHealthStatus healthStatus)
    {
        _healthStatus = healthStatus;
        return this;
    }

    /// <summary>
    /// Sets the resolution backend that populated this record.
    /// </summary>
    /// <param name="source">The discovery source.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceDiscoveryRecordBuilder WithSource(DiscoverySource source)
    {
        _source = source;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="ServiceDiscoveryRecord"/>.
    /// </summary>
    /// <param name="template">The record to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static ServiceDiscoveryRecordBuilder From(ServiceDiscoveryRecord template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new ServiceDiscoveryRecordBuilder()
            .WithInstanceId(template.InstanceId)
            .WithServiceName(template.ServiceName)
            .WithVersion(template.Version)
            .WithHost(template.Host)
            .WithPort(template.Port)
            .WithScheme(template.Scheme)
            .WithWeight(template.Weight)
            .WithPriority(template.Priority)
            .WithHealthStatus(template.HealthStatus)
            .WithSource(template.Source);
    }

    /// <summary>
    /// Builds the <see cref="ServiceDiscoveryRecord"/> instance with the current values.
    /// </summary>
    /// <returns>A fully configured <see cref="ServiceDiscoveryRecord"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public ServiceDiscoveryRecord Build()
    {
        // Validate required properties
        if (string.IsNullOrWhiteSpace(_serviceName))
            throw new ArgumentException("Service name is required.", nameof(_serviceName));
        if (string.IsNullOrWhiteSpace(_host))
            throw new ArgumentException("Host is required.", nameof(_host));

        return new ServiceDiscoveryRecord
        {
            InstanceId = _instanceId,
            ServiceName = _serviceName,
            Version = _version,
            Host = _host,
            Port = _port,
            Scheme = _scheme,
            Weight = _weight,
            Priority = _priority,
            HealthStatus = _healthStatus,
            Source = _source
        };
    }
}