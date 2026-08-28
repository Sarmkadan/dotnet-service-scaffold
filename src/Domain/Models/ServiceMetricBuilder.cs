#nullable enable

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Fluent builder for <see cref="ServiceMetric"/> instances.
/// </summary>
public class ServiceMetricBuilder
{
    private Guid _id;
    private Guid _serviceId;
    private ServiceRegistration? _service;
    private decimal _cpuUsagePercent;
    private decimal _memoryUsagePercent;
    private long _memoryUsageBytes;
    private decimal _diskUsagePercent;
    private long _diskUsageBytes;
    private int _activeConnections;
    private long _requestsPerSecond;

    /// <summary>
    /// Sets the unique identifier for the service metric.
    /// </summary>
    /// <param name="id">The service metric identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the service identifier.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithServiceId(Guid serviceId)
    {
        _serviceId = serviceId;
        return this;
    }

    /// <summary>
    /// Sets the service.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithService(ServiceRegistration? service)
    {
        _service = service;
        return this;
    }

    /// <summary>
    /// Sets the CPU usage percentage.
    /// </summary>
    /// <param name="cpuUsagePercent">The CPU usage percentage.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithCpuUsagePercent(decimal cpuUsagePercent)
    {
        _cpuUsagePercent = cpuUsagePercent;
        return this;
    }

    /// <summary>
    /// Sets the memory usage percentage.
    /// </summary>
    /// <param name="memoryUsagePercent">The memory usage percentage.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithMemoryUsagePercent(decimal memoryUsagePercent)
    {
        _memoryUsagePercent = memoryUsagePercent;
        return this;
    }

    /// <summary>
    /// Sets the memory usage in bytes.
    /// </summary>
    /// <param name="memoryUsageBytes">The memory usage in bytes.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithMemoryUsageBytes(long memoryUsageBytes)
    {
        _memoryUsageBytes = memoryUsageBytes;
        return this;
    }

    /// <summary>
    /// Sets the disk usage percentage.
    /// </summary>
    /// <param name="diskUsagePercent">The disk usage percentage.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithDiskUsagePercent(decimal diskUsagePercent)
    {
        _diskUsagePercent = diskUsagePercent;
        return this;
    }

    /// <summary>
    /// Sets the disk usage in bytes.
    /// </summary>
    /// <param name="diskUsageBytes">The disk usage in bytes.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithDiskUsageBytes(long diskUsageBytes)
    {
        _diskUsageBytes = diskUsageBytes;
        return this;
    }

    /// <summary>
    /// Sets the number of active connections.
    /// </summary>
    /// <param name="activeConnections">The number of active connections.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithActiveConnections(int activeConnections)
    {
        _activeConnections = activeConnections;
        return this;
    }

    /// <summary>
    /// Sets the requests per second.
    /// </summary>
    /// <param name="requestsPerSecond">The requests per second.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceMetricBuilder WithRequestsPerSecond(long requestsPerSecond)
    {
        _requestsPerSecond = requestsPerSecond;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="ServiceMetric"/>.
    /// </summary>
    /// <param name="template">The service metric to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static ServiceMetricBuilder From(ServiceMetric template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new ServiceMetricBuilder()
            .WithId(template.Id)
            .WithServiceId(template.ServiceId)
            .WithService(template.Service)
            .WithCpuUsagePercent(template.CpuUsagePercent)
            .WithMemoryUsagePercent(template.MemoryUsagePercent)
            .WithMemoryUsageBytes(template.MemoryUsageBytes)
            .WithDiskUsagePercent(template.DiskUsagePercent)
            .WithDiskUsageBytes(template.DiskUsageBytes)
            .WithActiveConnections(template.ActiveConnections)
            .WithRequestsPerSecond(template.RequestsPerSecond);
    }

    /// <summary>
    /// Builds the <see cref="ServiceMetric"/> instance with the current values.
    /// </returns>A fully configured <see cref="ServiceMetric"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public ServiceMetric Build()
    {
        // Validate required properties
        // Note: Based on the ServiceMetric class, Id and ServiceId are required (they have [Key] and [ForeignKey] attributes)
        // However, looking at the class definition, these are get/set properties without explicit required validation
        // Since the task says to validate required properties are missing, and the class doesn't have explicit
        // validation attributes on these properties, I'll check if they're default values (Guid.Empty)
        if (_id == Guid.Empty)
            throw new ArgumentException("Id is required.", nameof(_id));
        if (_serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId is required.", nameof(_serviceId));

        return new ServiceMetric
        {
            Id = _id,
            ServiceId = _serviceId,
            Service = _service,
            CpuUsagePercent = _cpuUsagePercent,
            MemoryUsagePercent = _memoryUsagePercent,
            MemoryUsageBytes = _memoryUsageBytes,
            DiskUsagePercent = _diskUsagePercent,
            DiskUsageBytes = _diskUsageBytes,
            ActiveConnections = _activeConnections,
            RequestsPerSecond = _requestsPerSecond
        };
    }
}