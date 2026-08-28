#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.DockerCompose;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Interface for Docker Compose controller operations.
/// </summary>
public interface IDockerComposeController
{
    /// <summary>
    /// Generates a Docker Compose YAML string from the provided options.
    /// </summary>
    /// <param name="options">Docker Compose generation options.</param>
    /// <returns>The generated YAML content as plain text.</returns>
    IActionResult Generate(DockerComposeOptions options);

    /// <summary>
    /// Returns a Docker Compose YAML file as a downloadable attachment.
    /// </summary>
    /// <param name="options">Docker Compose generation options.</param>
    /// <returns>File download of the generated YAML.</returns>
    IActionResult Download(DockerComposeOptions options);
}