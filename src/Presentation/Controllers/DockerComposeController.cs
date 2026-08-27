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
    [HttpPost(DockerComposeControllerConstants.GenerateRoute)]
    [ProducesResponseType(typeof(string), DockerComposeControllerConstants.SuccessStatusCode)]
    [ProducesResponseType(DockerComposeControllerConstants.BadRequestStatusCode)]
    public IActionResult Generate([FromBody] DockerComposeOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            return BadRequest(new { error = DockerComposeControllerConstants.ServiceNameRequired });
        }

        if (string.IsNullOrWhiteSpace(options.ImageName))
        {
            return BadRequest(new { error = DockerComposeControllerConstants.ImageNameRequired });
        }

        try
        {
            var yaml = _generator.Generate(options);
            _logger.LogInformation(DockerComposeControllerConstants.LogGeneratedMessage, options.ServiceName);
            return Content(yaml, DockerComposeControllerConstants.YamlContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, DockerComposeControllerConstants.LogErrorGeneratingMessage, options.ServiceName);
            return StatusCode(DockerComposeControllerConstants.InternalServerErrorStatusCode, new { error = DockerComposeControllerConstants.GenerationFailed });
        }
    }

    /// <summary>
    /// Returns a Docker Compose YAML file as a downloadable attachment.
    /// </summary>
    /// <param name="options">Docker Compose generation options.</param>
    /// <response code="200">File download of the generated YAML.</response>
    /// <response code="400">If options are invalid.</response>
    [HttpPost(DockerComposeControllerConstants.DownloadRoute)]
    [ProducesResponseType(DockerComposeControllerConstants.SuccessStatusCode)]
    [ProducesResponseType(DockerComposeControllerConstants.BadRequestStatusCode)]
    public IActionResult Download([FromBody] DockerComposeOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            return BadRequest(new { error = DockerComposeControllerConstants.ServiceNameRequired });
        }

        try
        {
            var yaml = _generator.Generate(options);
            var bytes = System.Text.Encoding.UTF8.GetBytes(yaml);
            return File(bytes, DockerComposeControllerConstants.YamlContentType, DockerComposeControllerConstants.DefaultFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, DockerComposeControllerConstants.LogErrorDownloadingMessage, options.ServiceName);
            return StatusCode(DockerComposeControllerConstants.InternalServerErrorStatusCode, new { error = DockerComposeControllerConstants.GenerationFailed });
        }
    }
}
