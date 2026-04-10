#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.DockerCompose;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.DockerCompose;

public class DockerComposeGeneratorTests
{
    private readonly IDockerComposeGenerator _generator;

    public DockerComposeGeneratorTests()
    {
        var logger = Substitute.For<ILogger<DockerComposeGenerator>>();
        _generator = new DockerComposeGenerator(logger);
    }

    [Fact]
    public void Generate_ShouldContainServiceName_WhenOptionsProvided()
    {
        var options = new DockerComposeOptions
        {
            ServiceName = "my-api",
            ImageName = "my-api:1.0",
            HostPort = 8080,
            ContainerPort = 8080
        };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain("my-api:");
        yaml.Should().Contain("image: my-api:1.0");
        yaml.Should().Contain("8080:8080");
    }

    [Fact]
    public void Generate_ShouldIncludeHealthCheck()
    {
        var options = new DockerComposeOptions { ServiceName = "svc", ContainerPort = 5001 };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain("healthcheck:");
        yaml.Should().Contain("http://localhost:5001/health");
    }

    [Fact]
    public void Generate_ShouldIncludeCaddy_WhenRequested()
    {
        var options = new DockerComposeOptions
        {
            ServiceName = "svc",
            IncludeCaddy = true,
            CaddyDomain = "example.com"
        };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain("caddy:");
        yaml.Should().Contain("example.com");
        yaml.Should().Contain("caddy-data:");
    }

    [Fact]
    public void Generate_ShouldNotIncludeCaddy_WhenNotRequested()
    {
        var options = new DockerComposeOptions { ServiceName = "svc", IncludeCaddy = false };

        var yaml = _generator.Generate(options);

        yaml.Should().NotContain("caddy:");
    }

    [Fact]
    public void Generate_ShouldIncludeRedis_WhenRequested()
    {
        var options = new DockerComposeOptions { ServiceName = "svc", IncludeRedis = true };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain("redis:");
        yaml.Should().Contain("redis:7-alpine");
    }

    [Fact]
    public void Generate_ShouldIncludeExtraEnvVars_WhenProvided()
    {
        var options = new DockerComposeOptions
        {
            ServiceName = "svc",
            EnvironmentVariables = new Dictionary<string, string> { ["MY_VAR"] = "hello" }
        };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain("MY_VAR: hello");
    }

    [Fact]
    public void Generate_ShouldThrow_WhenOptionsIsNull()
    {
        var act = () => _generator.Generate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Generate_ShouldIncludePrometheusComment_WhenRequested()
    {
        var options = new DockerComposeOptions { ServiceName = "svc", IncludePrometheus = true };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain("metrics_path: /metrics");
    }

    [Fact]
    public void Generate_ShouldIncludeResourceLimits()
    {
        var options = new DockerComposeOptions
        {
            ServiceName = "svc",
            CpuLimit = "2",
            MemoryLimit = "1G"
        };

        var yaml = _generator.Generate(options);

        yaml.Should().Contain("cpus: '2'");
        yaml.Should().Contain("memory: 1G");
    }

    [Fact]
    public async Task WriteToFileAsync_ShouldWriteYamlFile()
    {
        var options = new DockerComposeOptions { ServiceName = "svc" };
        var path = Path.Combine(AppContext.BaseDirectory, $"test-compose-{Guid.NewGuid()}.yml");

        try
        {
            await _generator.WriteToFileAsync(options, path);

            File.Exists(path).Should().BeTrue();
            var content = await File.ReadAllTextAsync(path);
            content.Should().Contain("svc:");
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
