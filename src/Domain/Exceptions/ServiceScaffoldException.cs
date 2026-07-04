#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Exceptions;

/// <summary>
/// Base exception for the service scaffold platform.
/// </summary>
public class ServiceScaffoldException : Exception
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
        : base($"Service with ID {serviceId} not found", "SERVICE_NOT_FOUND")
    {
    }

    public ServiceNotFoundException(string serviceName)
        : base($"Service '{serviceName}' not found", "SERVICE_NOT_FOUND")
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
        : base(message, "VALIDATION_ERROR")
    {
        Errors.Add(message);
    }

    public ServiceValidationException(List<string> errors)
        : base($"Validation failed: {string.Join("; ", errors)}", "VALIDATION_ERROR")
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
        : base($"Health check failed for service {serviceId}: {reason}", "HEALTH_CHECK_FAILED")
    {
    }
}

/// <summary>
/// Thrown when an unauthorized access attempt is made.
/// </summary>
public class UnauthorizedException : ServiceScaffoldException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, "UNAUTHORIZED")
    {
    }
}

/// <summary>
/// Thrown when an API key is invalid or expired.
/// </summary>
public class InvalidApiKeyException : ServiceScaffoldException
{
    public InvalidApiKeyException(string reason = "Invalid API key")
        : base(reason, "INVALID_API_KEY")
    {
    }
}

/// <summary>
/// Thrown when a database operation fails.
/// </summary>
public class DataAccessException : ServiceScaffoldException
{
    public DataAccessException(string message, Exception innerException)
        : base(message, "DATA_ACCESS_ERROR", innerException)
    {
    }
}

/// <summary>
/// Thrown when configuration is invalid or missing.
/// </summary>
public class ConfigurationException : ServiceScaffoldException
{
    public ConfigurationException(string message)
        : base(message, "CONFIGURATION_ERROR")
    {
    }
}

/// <summary>
/// Thrown when an operation exceeds a resource limit.
/// </summary>
public class ResourceExhaustedException : ServiceScaffoldException
{
    public ResourceExhaustedException(string resource)
        : base($"Resource limit exceeded for {resource}", "RESOURCE_EXHAUSTED")
    {
    }
}
