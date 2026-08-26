#nullable enable

namespace DotnetServiceScaffold.Infrastructure.ServiceMesh;

/// <summary>
/// Interface for service mesh configuration options.
/// </summary>
public interface IServiceMeshOptions
{
    string AdminEndpoint { get; set; }
    int ReadinessTimeoutSeconds { get; set; }
    string MeshName { get; set; }
    bool Enabled { get; set; }
}