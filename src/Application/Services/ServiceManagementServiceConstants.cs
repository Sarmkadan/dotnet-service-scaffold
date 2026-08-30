namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// Constants for ServiceManagementService.
/// </summary>
internal static class ServiceManagementServiceConstants
{
    public const string ServiceNameRequired = "Service name is required";
    public const string ServiceEndpointRequired = "Service endpoint is required";
    public const string HealthCheckUrlRequired = "Health check URL is required";
    public const string InvalidServiceEndpointUrl = "Invalid service endpoint URL";
    public const string InvalidHealthCheckUrl = "Invalid health check URL";
    public const string ServiceOwnerNotFound = "Service owner not found";
    public const string OwnerNotFoundErrorCode = "OWNER_NOT_FOUND";
    public const string ServiceNameAlreadyRegistered = "Service name already registered";
    public const string DefaultVersion = "1.0.0";
    public const string AuditActionCreate = "Create";
    public const string AuditEntityTypeServiceRegistration = "ServiceRegistration";
    public const string AuditActionDelete = "Delete";
    public const string AuditActionUpdate = "Update";
    public const string ServiceConfigurationInvalid = "Service configuration is invalid";
    public const string ReEnabledServiceAuditMessage = "Re-enabled service";
    public const int DefaultMinutesBack = 60;
}