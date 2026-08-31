#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Tests.Infrastructure.DockerCompose;

using DotnetServiceScaffold.Infrastructure.DockerCompose;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests for the DockerComposeGenerator class.
/// </summary>
public class DockerComposeGeneratorTests : IDockerComposeGeneratorTests
{
    private readonly IDockerComposeGenerator _generator;
    private readonly ILogger<DockerComposeGeneratorTests> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockerComposeGeneratorTests"/> class.
    /// </summary>
    public DockerComposeGeneratorTests()
    {
        var generatorLogger = Substitute.For<ILogger<DockerComposeGenerator>>();
        _logger = Substitute.For<ILogger<DockerComposeGeneratorTests>>();
        _generator = new DockerComposeGenerator(generatorLogger);
    }

    /// <summary>
    /// Verifies that the generated YAML contains the service name when options are provided.
    /// </summary>
    [Fact]
    public void Generate_ShouldContainServiceName_WhenOptionsProvided()
    {
        _logger.LogInformation("Generate_ShouldContainServiceName_WhenOptionsProvided called");
        var options = new DockerComposeOptions
        {
            ServiceName = DockerComposeGeneratorTestsConstants.MyApiServiceName,
            ImageName = DockerComposeGeneratorTestsConstants.MyApiImageName,
            HostPort = DockerComposeGeneratorTestsConstants.MyApiPort,
            ContainerPort = DockerComposeGeneratorTestsConstants.MyApiPort
        };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain($"{DockerComposeGeneratorTestsConstants.MyApiServiceName}:");
        yaml.Should().Contain($"image: {DockerComposeGeneratorTestsConstants.MyApiImageName}");
        yaml.Should().Contain(DockerComposeGeneratorTestsConstants.MyApiPortMapping);
    }

    public async Task Generate_ShouldContainServiceName_WhenOptionsProvidedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate_ShouldContainServiceName_WhenOptionsProvidedAsync called");
        await Task.Yield();
        Generate_ShouldContainServiceName_WhenOptionsProvided();
    }

    /// <summary>
    /// Verifies that the generated YAML includes a health check.
    /// </summary>
    [Fact]
    public void Generate_ShouldIncludeHealthCheck()
    {
        _logger.LogInformation("Generate_ShouldIncludeHealthCheck called");
        var options = new DockerComposeOptions { ServiceName = DockerComposeGeneratorTestsConstants.DefaultServiceName, ContainerPort = DockerComposeGeneratorTestsConstants.DefaultContainerPort };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain(DockerComposeGeneratorTestsConstants.HealthCheckSection);
        yaml.Should().Contain(DockerComposeGeneratorTestsConstants.DefaultHealthCheckUrl);
    }

    public async Task Generate_ShouldIncludeHealthCheckAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate_ShouldIncludeHealthCheckAsync called");
        await Task.Yield();
        Generate_ShouldIncludeHealthCheck();
    }

    /// <summary>
    /// Verifies that the generated YAML includes Caddy when requested.
    /// </summary>
    [Fact]
    public void Generate_ShouldIncludeCaddy_WhenRequested()
    {
        _logger.LogInformation("Generate_ShouldIncludeCaddy_WhenRequested called");
        var options = new DockerComposeOptions
        {
            ServiceName = DockerComposeGeneratorTestsConstants.DefaultServiceName,
            IncludeCaddy = true,
            CaddyDomain = DockerComposeGeneratorTestsConstants.CaddyDomain
        };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain($"{DockerComposeGeneratorTestsConstants.CaddyServiceName}:");
        yaml.Should().Contain(DockerComposeGeneratorTestsConstants.CaddyDomain);
        yaml.Should().Contain($"{DockerComposeGeneratorTestsConstants.CaddyDataServiceName}:");
    }

    public async Task Generate_ShouldIncludeCaddy_WhenRequestedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate_ShouldIncludeCaddy_WhenRequestedAsync called");
        await Task.Yield();
        Generate_ShouldIncludeCaddy_WhenRequested();
    }

    /// <summary>
    /// Verifies that the generated YAML does not include Caddy when not requested.
    /// </summary>
    [Fact]
    public void Generate_ShouldNotIncludeCaddy_WhenNotRequested()
    {
        _logger.LogInformation("Generate_ShouldNotIncludeCaddy_WhenNotRequested called");
        var options = new DockerComposeOptions { ServiceName = DockerComposeGeneratorTestsConstants.DefaultServiceName, IncludeCaddy = false };

        var yaml = _generator.Generate(options);

        yaml.Should().NotContain($"{DockerComposeGeneratorTestsConstants.CaddyServiceName}:");
    }

    public async Task Generate_ShouldNotIncludeCaddy_WhenNotRequestedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate_ShouldNotIncludeCaddy_WhenNotRequestedAsync called");
        await Task.Yield();
        Generate_ShouldNotIncludeCaddy_WhenNotRequested();
    }

    /// <summary>
    /// Verifies that the generated YAML includes Redis when requested.
    /// </summary>
    [Fact]
    public void Generate_ShouldIncludeRedis_WhenRequested()
    {
        _logger.LogInformation("Generate_ShouldIncludeRedis_WhenRequested called");
        var options = new DockerComposeOptions { ServiceName = DockerComposeGeneratorTestsConstants.DefaultServiceName, IncludeRedis = true };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain($"{DockerComposeGeneratorTestsConstants.RedisServiceName}:");
        yaml.Should().Contain(DockerComposeGeneratorTestsConstants.RedisImageName);
    }

    public async Task Generate_ShouldIncludeRedis_WhenRequestedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate_ShouldIncludeRedis_WhenRequestedAsync called");
        await Task.Yield();
        Generate_ShouldIncludeRedis_WhenRequested();
    }

    /// <summary>
    /// Verifies that the generated YAML includes extra environment variables when provided.
    /// </summary>
    [Fact]
    public void Generate_ShouldIncludeExtraEnvVars_WhenProvided()
    {
        _logger.LogInformation("Generate_ShouldIncludeExtraEnvVars_WhenProvided called");
        var options = new DockerComposeOptions
        {
            ServiceName = DockerComposeGeneratorTestsConstants.DefaultServiceName,
            EnvironmentVariables = new Dictionary<string, string> { [DockerComposeGeneratorTestsConstants.TestEnvVarName] = DockerComposeGeneratorTestsConstants.TestEnvVarValue }
        };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain(DockerComposeGeneratorTestsConstants.TestEnvVarEntry);
    }

    public async Task Generate_ShouldIncludeExtraEnvVars_WhenProvidedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate_ShouldIncludeExtraEnvVars_WhenProvidedAsync called");
        await Task.Yield();
        Generate_ShouldIncludeExtraEnvVars_WhenProvided();
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown when options are null.
    /// </summary>
    [Fact]
    public void Generate_ShouldThrow_WhenOptionsIsNull()
    {
        _logger.LogInformation("Generate_ShouldThrow_WhenOptionsIsNull called");
        var act = () => _generator.Generate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    public async Task Generate_ShouldThrow_WhenOptionsIsNullAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate_ShouldThrow_WhenOptionsIsNullAsync called");
        await Task.Yield();
        Generate_ShouldThrow_WhenOptionsIsNull();
    }

    /// <summary>
    /// Verifies that the generated YAML includes a Prometheus comment when requested.
    /// </summary>
    [Fact]
    public void Generate_ShouldIncludePrometheusComment_WhenRequested()
    {
        _logger.LogInformation("Generate_ShouldIncludePrometheusComment_WhenRequested called");
        var options = new DockerComposeOptions { ServiceName = DockerComposeGeneratorTestsConstants.DefaultServiceName, IncludePrometheus = true };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain(DockerComposeGeneratorTestsConstants.PrometheusMetricsPath);
    }

    public async Task Generate_ShouldIncludePrometheusComment_WhenRequestedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate_ShouldIncludePrometheusComment_WhenRequestedAsync called");
        await Task.Yield();
        Generate_ShouldIncludePrometheusComment_WhenRequested();
    }

    /// <summary>
    /// Verifies that the generated YAML includes resource limits.
    /// </summary>
    [Fact]
    public void Generate_ShouldIncludeResourceLimits()
    {
        _logger.LogInformation("Generate_ShouldIncludeResourceLimits called");
        var options = new DockerComposeOptions
        {
            ServiceName = DockerComposeGeneratorTestsConstants.DefaultServiceName,
            CpuLimit = DockerComposeGeneratorTestsConstants.CpuLimitValue,
            MemoryLimit = DockerComposeGeneratorTestsConstants.MemoryLimitValue
        };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain(DockerComposeGeneratorTestsConstants.CpusEntry);
        yaml.Should().Contain(DockerComposeGeneratorTestsConstants.MemoryEntry);
    }

    public async Task Generate_ShouldIncludeResourceLimitsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate_ShouldIncludeResourceLimitsAsync called");
        await Task.Yield();
        Generate_ShouldIncludeResourceLimits();
    }

    /// <summary>
    /// Verifies that the <see cref="WriteToFileAsync"/> method writes the YAML file correctly.
    /// </summary>
    [Fact]
    public async Task WriteToFileAsync_ShouldWriteYamlFile()
    {
        _logger.LogInformation("WriteToFileAsync_ShouldWriteYamlFile called");
        var options = new DockerComposeOptions { ServiceName = DockerComposeGeneratorTestsConstants.DefaultServiceName };
        var path = Path.Combine(AppContext.BaseDirectory, $"{DockerComposeGeneratorTestsConstants.TestComposeFilePrefix}{Guid.NewGuid()}{DockerComposeGeneratorTestsConstants.YamlFileExtension}");

        try
        {
            await _generator.WriteToFileAsync(options, path);

            File.Exists(path).Should().BeTrue();
            var content = await File.ReadAllTextAsync(path);
            content.Should().Contain($"{DockerComposeGeneratorTestsConstants.DefaultServiceName}:");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WriteToFileAsync_ShouldWriteYamlFile");
            throw;
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
