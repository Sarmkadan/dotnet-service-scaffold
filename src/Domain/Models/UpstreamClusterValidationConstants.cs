#nullable enable

namespace DotnetServiceScaffold.Domain.Models;

/// <summary>
/// Contains constant values used in <see cref="UpstreamClusterValidation"/>.
/// </summary>
internal static class UpstreamClusterValidationConstants
{
    public const string NameCannotBeNullOrWhitespace = "Name cannot be null or whitespace.";
    public const string EndpointCannotBeNullOrWhitespace = "Endpoint cannot be null or whitespace.";
    public const string HealthyHostsCannotBeNegative = "HealthyHosts cannot be negative.";
    public const string TotalHostsCannotBeNegative = "TotalHosts cannot be negative.";
    public const string TotalHostsCannotBeLessThanHealthyHosts = "TotalHosts cannot be less than HealthyHosts.";
    public const string CircuitBreakerOpenShouldBeTrueWhenHostsPresentButNoneHealthy = "CircuitBreakerOpen should be true when there are hosts but none are healthy.";
    public const string UpstreamClusterValidationFailedHeader = "UpstreamCluster validation failed:";
}