#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Exceptions;

/// <summary>
/// Base exception for the service scaffold platform.
/// </summary>
public class ServiceScaffoldException : Exception, IServiceScaffoldException
{
    public string? ErrorCode { get; set; }

    public ServiceScaffoldException(string message) : base(message)
    {
    }

    public ServiceScaffoldException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ServiceScaffoldException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public ServiceScaffoldException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Thrown when a service is not found in the system.
/// </summary>
public class ServiceNotFoundException : ServiceScaffoldException
{
    public ServiceNotFoundException(Guid serviceId)
        : base(
            string.Format(ServiceScaffoldExceptionConstants.ServiceIdNotFoundMessageFormat, serviceId),
            ServiceScaffoldExceptionConstants.ServiceNotFoundErrorCode)
    {
    }

    public ServiceNotFoundException(string serviceName)
        : base(
            string.Format(ServiceScaffoldExceptionConstants.ServiceNameNotFoundMessageFormat, serviceName),
            ServiceScaffoldExceptionConstants.ServiceNotFoundErrorCode)
    {
    }
}

/// <summary>
/// Thrown when validation of service configuration fails.
/// </summary>
public class ServiceValidationException : ServiceScaffoldException
{
    public List<string> Errors { get; set; } = new();

    public ServiceValidationException(string message)
        : base(message, ServiceScaffoldExceptionConstants.ValidationErrorCode)
    {
        Errors.Add(message);
    }

    public ServiceValidationException(List<string> errors)
        : base(
            string.Format(
                ServiceScaffoldExceptionConstants.ValidationFailedMessageFormat,
                string.Join(ServiceScaffoldExceptionConstants.ErrorSeparator, errors)),
            ServiceScaffoldExceptionConstants.ValidationErrorCode)
    {
        Errors = errors;
    }
}

/// <summary>
/// Thrown when a health check operation fails.
/// </summary>
public class HealthCheckException : ServiceScaffoldException
{
    public HealthCheckException(Guid serviceId, string reason)
        : base(
            string.Format(ServiceScaffoldExceptionConstants.HealthCheckFailedMessageFormat, serviceId, reason),
            ServiceScaffoldExceptionConstants.HealthCheckFailedErrorCode)
    {
    }
}

/// <summary>
/// Thrown when an unauthorized access attempt is made.
/// </summary>
public class UnauthorizedException : ServiceScaffoldException
{
    public UnauthorizedException(string message = ServiceScaffoldExceptionConstants.DefaultUnauthorizedMessage)
        : base(message, ServiceScaffoldExceptionConstants.UnauthorizedErrorCode)
    {
    }
}

/// <summary>
/// Thrown when an API key is invalid or expired.
/// </summary>
public class InvalidApiKeyException : ServiceScaffoldException
{
    public InvalidApiKeyException(string reason = ServiceScaffoldExceptionConstants.DefaultInvalidApiKeyMessage)
        : base(reason, ServiceScaffoldExceptionConstants.InvalidApiKeyErrorCode)
    {
    }
}

/// <summary>
/// Thrown when a database operation fails.
/// </summary>
public class DataAccessException : ServiceScaffoldException
{
    public DataAccessException(string message, Exception innerException)
        : base(message, ServiceScaffoldExceptionConstants.DataAccessErrorCode, innerException)
    {
    }
}

/// <summary>
/// Thrown when configuration is invalid or missing.
/// </summary>
public class ConfigurationException : ServiceScaffoldException
{
    public ConfigurationException(string message)
        : base(message, ServiceScaffoldExceptionConstants.ConfigurationErrorCode)
    {
    }
}

/// <summary>
/// Thrown when an operation exceeds a resource limit.
/// </summary>
public class ResourceExhaustedException : ServiceScaffoldException
{
    public ResourceExhaustedException(string resource)
        : base(
            string.Format(ServiceScaffoldExceptionConstants.ResourceExhaustedMessageFormat, resource),
            ServiceScaffoldExceptionConstants.ResourceExhaustedErrorCode)
    {
    }
}
