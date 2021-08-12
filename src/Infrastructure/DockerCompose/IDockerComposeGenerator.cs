#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.DockerCompose;

/// <summary>
/// Generates Docker Compose YAML content from strongly-typed options.
/// </summary>
public interface IDockerComposeGenerator
{
    /// <summary>
    /// Generates a Docker Compose YAML string from the provided options.
    /// </summary>
    /// <param name="options">Configuration for the compose file.</param>
    /// <returns>Valid Docker Compose YAML as a string.</returns>
    string Generate(DockerComposeOptions options);

    /// <summary>
    /// Writes a Docker Compose YAML file to the specified path.
    /// </summary>
    /// <param name="options">Configuration for the compose file.</param>
    /// <param name="outputPath">File system path to write the YAML to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WriteToFileAsync(DockerComposeOptions options, string outputPath, CancellationToken cancellationToken = default);
}
