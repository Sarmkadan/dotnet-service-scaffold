#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Exceptions;

/// <summary>
/// Centralized constants for service scaffold exceptions (error codes and message formats).
/// </summary>
internal static class ServiceScaffoldExceptionConstants
{
    /// <summary>Error code used when a service cannot be found.</summary>
    public const string ServiceNotFoundErrorCode = "SERVICE_NOT_FOUND";

    /// <summary>Error code used when validation fails.</summary>
    public const string ValidationErrorCode = "VALIDATION_ERROR";

    /// <summary>Error code used when a health check operation fails.</summary>
    public const string HealthCheckFailedErrorCode = "HEALTH_CHECK_FAILED";

    /// <summary>Error code used for unauthorized access attempts.</summary>
    public const string UnauthorizedErrorCode = "UNAUTHORIZED";

    /// <summary>Error code used when an API key is invalid or expired.</summary>
    public const string InvalidApiKeyErrorCode = "INVALID_API_KEY";

    /// <summary>Error code used when a database operation fails.</summary>
    public const string DataAccessErrorCode = "DATA_ACCESS_ERROR";

    /// <summary>Error code used when configuration is invalid or missing.</summary>
    public const string ConfigurationErrorCode = "CONFIGURATION_ERROR";

    /// <summary>Error code used when an operation exceeds a resource limit.</summary>
    public const string ResourceExhaustedErrorCode = "RESOURCE_EXHAUSTED";

    /// <summary>Message format for a missing service referenced by its identifier.</summary>
    public const string ServiceIdNotFoundMessageFormat = "Service with ID {0} not found";

    /// <summary>Message format for a missing service referenced by its name.</summary>
    public const string ServiceNameNotFoundMessageFormat = "Service '{0}' not found";

    /// <summary>Message format for aggregated validation failures.</summary>
    public const string ValidationFailedMessageFormat = "Validation failed: {0}";

    /// <summary>Message format for a failed health check.</summary>
    public const string HealthCheckFailedMessageFormat = "Health check failed for service {0}: {1}";

    /// <summary>Message format for exhausted resources.</summary>
    public const string ResourceExhaustedMessageFormat = "Resource limit exceeded for {0}";

    /// <summary>Default message for unauthorized access.</summary>
    public const string DefaultUnauthorizedMessage = "Unauthorized access";

    /// <summary>Default message for an invalid API key.</summary>
    public const string DefaultInvalidApiKeyMessage = "Invalid API key";

    /// <summary>Separator used when joining validation error messages.</summary>
    public const string ErrorSeparator = "; ";
}
