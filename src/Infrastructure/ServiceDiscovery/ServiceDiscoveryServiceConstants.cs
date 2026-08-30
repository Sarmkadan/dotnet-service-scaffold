#nullable enable

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Constants for ServiceDiscoveryService.
/// </summary>
internal static class ServiceDiscoveryServiceConstants
{
    public const string CacheKeyPrefix = "discovery:";
    public const string CacheHitLogMessage = "Cache hit for {ServiceName} ({Count} instance(s))";
    public const string DefaultServiceName = "dotnet-service";
    public const string NoHealthyInstancesMessage = "No healthy instances found for service '{serviceName}'.";
    public const string NoHealthyInstancesErrorCode = "NO_HEALTHY_INSTANCES";
    public const string SelfRegisteredLogMessage = "Self-registered as {ServiceName} ({InstanceId}) via {Provider}";
    public const string SelfRegistrationFailedLogMessage = "Self-registration failed via {Provider}: {Error}";
    public const string SelfDeregisteredLogMessage = "Self-deregistered instance {InstanceId} via {Provider}";
    public const string ServiceCatalogEnumRequiresRegistryMessage = "Service catalog enumeration requires a registry-based provider.";
    public const string ProviderUnsupportedErrorCode = "PROVIDER_UNSUPPORTED";
    public const string CacheInvalidatedForServiceLogMessage = "Cache invalidated for service {ServiceName}";
    public const string DiscoveryCacheFullyInvalidatedLogMessage = "Discovery cache fully invalidated";
    public const string SelfRegistrationDisabledLogMessage = "Self-registration is disabled, skipping heartbeat update";
    public const string HeartbeatUpdatedLogMessage = "Heartbeat updated for instance {InstanceId} via {Provider}";
    public const string FailedToUpdateHeartbeatLogMessage = "Failed to update heartbeat for instance {InstanceId} via {Provider}: {Error}";
}