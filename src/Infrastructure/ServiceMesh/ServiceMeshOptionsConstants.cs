#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.ServiceMesh;

/// <summary>
/// Constants for service mesh options and configuration.
/// </summary>
internal static class ServiceMeshOptionsConstants
{
    /// <summary>
    /// Default base URL for the sidecar proxy admin API.
    /// </summary>
    public const string DefaultAdminEndpoint = "http://localhost:15000";

    /// <summary>
    /// Default timeout in seconds for the sidecar proxy HTTP client.
    /// </summary>
    public const int HttpClientTimeoutSeconds = 10;

    /// <summary>
    /// Name of the User-Agent header for mesh client requests.
    /// </summary>
    public const string UserAgentHeaderName = "User-Agent";

    /// <summary>
    /// Value of the User-Agent header for mesh client requests.
    /// </summary>
    public const string UserAgentHeaderValue = "dotnet-service-scaffold/mesh-client";

    /// <summary>
    /// Prefix used for storing mesh context in HttpContext.Items.
    /// </summary>
    public const string MeshContextPrefix = "mesh:";
}