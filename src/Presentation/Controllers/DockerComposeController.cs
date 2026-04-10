#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.DockerCompose;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// API controller for generating Docker Compose YAML files from configuration options.
/// Useful for scaffolding deployment configurations programmatically.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class DockerComposeController : ControllerBase
{
    private readonly IDockerComposeGenerator _generator;
    private readonly ILogger<DockerComposeController> _logger;

    public DockerComposeController(
        IDockerComposeGenerator generator,
        ILogger<DockerComposeController> logger)
    {
        _generator = generator;
        _logger = logger;
    }

    /// <summary>
    /// Generates a Docker Compose YAML string from the provided options.
    /// </summary>
    /// <param name="options">Docker Compose generation options.</param>
    /// <returns>The generated YAML content as plain text.</returns>
    /// <response code="200">Returns the generated YAML.</response>
    /// <response code="400">If options are invalid.</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    public IActionResult Generate([FromBody] DockerComposeOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            return BadRequest(new { error = "ServiceName is required." });
        }

        if (string.IsNullOrWhiteSpace(options.ImageName))
        {
            return BadRequest(new { error = "ImageName is required." });
        }

        try
        {
            var yaml = _generator.Generate(options);
            _logger.LogInformation("Generated Docker Compose for service '{ServiceName}'", options.ServiceName);
            return Content(yaml, "text/yaml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Docker Compose for '{ServiceName}'", options.ServiceName);
            return StatusCode(500, new { error = "Failed to generate Docker Compose file." });
        }
    }

    /// <summary>
    /// Returns a Docker Compose YAML file as a downloadable attachment.
    /// </summary>
    /// <param name="options">Docker Compose generation options.</param>
    /// <response code="200">File download of the generated YAML.</response>
    /// <response code="400">If options are invalid.</response>
    [HttpPost("download")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public IActionResult Download([FromBody] DockerComposeOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            return BadRequest(new { error = "ServiceName is required." });
        }

        try
        {
            var yaml = _generator.Generate(options);
            var bytes = System.Text.Encoding.UTF8.GetBytes(yaml);
            return File(bytes, "text/yaml", "docker-compose.yml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Docker Compose download for '{ServiceName}'", options.ServiceName);
            return StatusCode(500, new { error = "Failed to generate Docker Compose file." });
        }
    }
}
