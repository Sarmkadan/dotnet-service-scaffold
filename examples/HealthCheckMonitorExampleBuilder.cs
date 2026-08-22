using System;

/// <summary>
/// Builder for <see cref="HealthCheckEntry"/>.
/// </summary>
public class HealthCheckMonitorExampleBuilder
{
    private string? _id;
    private string? _status;
    private int? _responseTime;
    private int? _statusCode;
    private string? _message;
    private DateTime? _checkedAt;

    /// <summary>
    /// Sets the Id.
    /// </summary>
    public HealthCheckMonitorExampleBuilder WithId(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the Status.
    /// </summary>
    public HealthCheckMonitorExampleBuilder WithStatus(string status)
    {
        ArgumentException.ThrowIfNullOrEmpty(status);
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets the ResponseTime.
    /// </summary>
    public HealthCheckMonitorExampleBuilder WithResponseTime(int responseTime)
    {
        _responseTime = responseTime;
        return this;
    }

    /// <summary>
    /// Sets the StatusCode.
    /// </summary>
    public HealthCheckMonitorExampleBuilder WithStatusCode(int statusCode)
    {
        _statusCode = statusCode;
        return this;
    }

    /// <summary>
    /// Sets the Message.
    /// </summary>
    public HealthCheckMonitorExampleBuilder WithMessage(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        _message = message;
        return this;
    }

    /// <summary>
    /// Sets the CheckedAt date.
    /// </summary>
    public HealthCheckMonitorExampleBuilder WithCheckedAt(DateTime checkedAt)
    {
        _checkedAt = checkedAt;
        return this;
    }

    /// <summary>
    /// Builds a configured <see cref="HealthCheckEntry"/> instance.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public HealthCheckEntry Build()
    {
        if (string.IsNullOrEmpty(_id)) throw new ArgumentException("Id is required.", nameof(_id));
        if (string.IsNullOrEmpty(_status)) throw new ArgumentException("Status is required.", nameof(_status));
        if (_responseTime == null) throw new ArgumentException("ResponseTime is required.", nameof(_responseTime));
        if (_statusCode == null) throw new ArgumentException("StatusCode is required.", nameof(_statusCode));
        if (string.IsNullOrEmpty(_message)) throw new ArgumentException("Message is required.", nameof(_message));
        if (_checkedAt == null) throw new ArgumentException("CheckedAt is required.", nameof(_checkedAt));

        return new HealthCheckEntry
        {
            Id = _id,
            Status = _status,
            ResponseTime = _responseTime.Value,
            StatusCode = _statusCode.Value,
            Message = _message,
            CheckedAt = _checkedAt.Value
        };
    }

    /// <summary>
    /// Creates a builder instance pre-filled from an existing <see cref="HealthCheckEntry"/>.
    /// </summary>
    public static HealthCheckMonitorExampleBuilder From(HealthCheckEntry template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new HealthCheckMonitorExampleBuilder()
            .WithId(template.Id)
            .WithStatus(template.Status)
            .WithResponseTime(template.ResponseTime)
            .WithStatusCode(template.StatusCode)
            .WithMessage(template.Message)
            .WithCheckedAt(template.CheckedAt);
    }
}
