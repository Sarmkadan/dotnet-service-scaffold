#nullable enable

using DotnetServiceScaffold.Domain.Enums;

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Fluent builder for <see cref="ServiceEvent"/> instances.
/// </summary>
public class ServiceEventBuilder
{
    private Guid _id;
    private Guid _serviceId;
    private ServiceRegistration? _service;
    private ServiceEventType _eventType;
    private string? _message;
    private DateTime _createdAt;
    private string? _severity;
    private string? _sourceHost;
    private string? _stackTrace;
    private bool _acknowledgedAt;

    /// <summary>
    /// Sets the unique identifier for the service event.
    /// </summary>
    /// <param name="id">The service event identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the service identifier.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithServiceId(Guid serviceId)
    {
        _serviceId = serviceId;
        return this;
    }

    /// <summary>
    /// Sets the service.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithService(ServiceRegistration? service)
    {
        _service = service;
        return this;
    }

    /// <summary>
    /// Sets the event type.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithEventType(ServiceEventType eventType)
    {
        _eventType = eventType;
        return this;
    }

    /// <summary>
    /// Sets the message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithMessage(string? message)
    {
        _message = message;
        return this;
    }

    /// <summary>
    /// Sets the timestamp when the event was created.
    /// </summary>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the severity level.
    /// </summary>
    /// <param name="severity">The severity level.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithSeverity(string? severity)
    {
        _severity = severity;
        return this;
    }

    /// <summary>
    /// Sets the source host.
    /// </summary>
    /// <param name="sourceHost">The source host.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithSourceHost(string? sourceHost)
    {
        _sourceHost = sourceHost;
        return this;
    }

    /// <summary>
    /// Sets the stack trace.
    /// </summary>
    /// <param name="stackTrace">The stack trace.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithStackTrace(string? stackTrace)
    {
        _stackTrace = stackTrace;
        return this;
    }

    /// <summary>
    /// Sets whether the event has been acknowledged.
    /// </summary>
    /// <param name="acknowledgedAt">Whether the event has been acknowledged.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ServiceEventBuilder WithAcknowledgedAt(bool acknowledgedAt)
    {
        _acknowledgedAt = acknowledgedAt;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="ServiceEvent"/>.
    /// </summary>
    /// <param name="template">The service event to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static ServiceEventBuilder From(ServiceEvent template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new ServiceEventBuilder()
            .WithId(template.Id)
            .WithServiceId(template.ServiceId)
            .WithService(template.Service)
            .WithEventType(template.EventType)
            .WithMessage(template.Message)
            .WithCreatedAt(template.CreatedAt)
            .WithSeverity(template.Severity)
            .WithSourceHost(template.SourceHost)
            .WithStackTrace(template.StackTrace)
            .WithAcknowledgedAt(template.AcknowledgedAt);
    }

    /// <summary>
    /// Builds the <see cref="ServiceEvent"/> instance with the current values.
    /// </summary>
    /// <returns>A fully configured <see cref="ServiceEvent"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public ServiceEvent Build()
    {
        // Validate required properties
        if (_id == Guid.Empty)
            throw new ArgumentException("Id is required.", nameof(_id));
        if (_serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId is required.", nameof(_serviceId));

        return new ServiceEvent
        {
            Id = _id,
            ServiceId = _serviceId,
            Service = _service,
            EventType = _eventType,
            Message = _message,
            CreatedAt = _createdAt,
            Severity = _severity,
            SourceHost = _sourceHost,
            StackTrace = _stackTrace,
            AcknowledgedAt = _acknowledgedAt
        };
    }
}