#nullable enable

namespace DotnetServiceScaffold.Infrastructure.ServiceMesh;

/// <summary>
/// Constants for service mesh options validation.
/// </summary>
internal static class ServiceMeshOptionsValidationConstants
{
    // AdminEndpoint validation messages
    public const string AdminEndpointNonEmptyError = "ServiceMesh.AdminEndpoint must be a non-empty URL string.";
    public const string AdminEndpointAbsoluteUrlError = "ServiceMesh.AdminEndpoint must be a valid absolute URL (e.g., http://localhost:15000).";
    public const string AdminEndpointSchemeError = "ServiceMesh.AdminEndpoint must use http:// or https:// scheme.";
    public const string AdminEndpointNoTrailingSlashError = "ServiceMesh.AdminEndpoint must not end with a trailing slash.";

    // ReadinessTimeoutSeconds validation messages and limits
    public const string ReadinessTimeoutPositiveError = "ServiceMesh.ReadinessTimeoutSeconds must be a positive integer greater than zero.";
    public const string ReadinessTimeoutMaxError = "ServiceMesh.ReadinessTimeoutSeconds should not exceed 60 seconds to avoid blocking application startup.";
    public const int ReadinessTimeoutSecondsMinValue = 0;
    public const int ReadinessTimeoutSecondsMaxValue = 60;

    // MeshName validation messages and limits
    public const string MeshNameNonEmptyError = "ServiceMesh.MeshName must be a non-empty string identifying the mesh environment.";
    public const string MeshNameMaxLengthError = "ServiceMesh.MeshName must be 50 characters or less.";
    public const string MeshNameNoWhitespaceExceptHyphenError = "ServiceMesh.MeshName must not contain whitespace characters other than hyphens.";
    public const int MeshNameMaxLength = 50;

    // Validation header
    public const string ValidationFailedHeader = "ServiceMeshOptions validation failed:";
}