// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using DotnetServiceScaffold.Infrastructure.Configuration;
using System.Text.RegularExpressions;

namespace DotnetServiceScaffold.Tests.Infrastructure.Configuration;

public class DeploymentConfigurationTests
{
    private DeploymentOptions _defaultOptions;

    public DeploymentConfigurationTests()
    {
        _defaultOptions = new DeploymentOptions
        {
            ServiceName = "test-service",
            ServiceDescription = "A test .NET service",
            ServiceUser = "testuser",
            ApplicationPath = "/app/test-service",
            DataPath = "/var/lib/test-service",
            LogPath = "/var/log/test-service",
            ServerDomain = "test.com",
            ApplicationPort = 5000,
            DotnetPath = "/usr/bin/dotnet",
            ServiceVersion = "1.0.0"
        };
    }

    [Fact]
    public void GenerateSystemdServiceUnit_ShouldContainAllExpectedValues()
    {
        // Act
        var unitFileContent = DeploymentConfiguration.GenerateSystemdServiceUnit(_defaultOptions);

        // Assert
        unitFileContent.Should().Contain($"Description={_defaultOptions.ServiceDescription}");
        unitFileContent.Should().Contain($"User={_defaultOptions.ServiceUser}");
        unitFileContent.Should().Contain($"WorkingDirectory={_defaultOptions.ApplicationPath}");
        unitFileContent.Should().Contain($"ExecStart={_defaultOptions.DotnetPath} DotnetServiceScaffold.dll");
        unitFileContent.Should().Contain($"SyslogIdentifier={_defaultOptions.ServiceName}");
        unitFileContent.Should().Contain($"ReadWritePaths={_defaultOptions.DataPath}");
    }

    [Fact]
    public void GenerateCaddyConfiguration_ShouldContainAllExpectedValues()
    {
        // Act
        var caddyConfigContent = DeploymentConfiguration.GenerateCaddyConfiguration(_defaultOptions);

        // Assert
        caddyConfigContent.Should().Contain($"# Caddy reverse proxy configuration for {_defaultOptions.ServiceName}");
        caddyConfigContent.Should().Contain($"{_defaultOptions.ServerDomain} {{");
        caddyConfigContent.Should().Contain($"reverse_proxy localhost:{_defaultOptions.ApplicationPort} {{");
        caddyConfigContent.Should().Contain($"output file {_defaultOptions.LogPath}/caddy.log {{");
    }

    [Fact]
    public void GenerateEnvironmentFile_ShouldContainAllExpectedValues()
    {
        // Act
        var envFileContent = DeploymentConfiguration.GenerateEnvironmentFile(_defaultOptions);

        // Assert
        envFileContent.Should().Contain($"# Environment variables for {_defaultOptions.ServiceName}");
        envFileContent.Should().Contain($"ASPNETCORE_URLS=http://localhost:{_defaultOptions.ApplicationPort}");
        envFileContent.Should().Contain($"ConnectionStrings__DefaultConnection=Data Source={_defaultOptions.DataPath}/scaffold.db");
        envFileContent.Should().Contain($"SERVICE_NAME={_defaultOptions.ServiceName}");
        envFileContent.Should().Contain($"SERVICE_VERSION={_defaultOptions.ServiceVersion}");
    }

    [Fact]
    public void GenerateDeploymentGuide_ShouldContainAllExpectedValues()
    {
        // Act
        var guideContent = DeploymentConfiguration.GenerateDeploymentGuide(_defaultOptions);

        // Assert
        guideContent.Should().Contain($"# Deployment Guide for {_defaultOptions.ServiceName}");
        guideContent.Should().Contain($"sudo useradd -r -s /bin/false {_defaultOptions.ServiceUser}");
        guideContent.Should().Contain($"sudo mkdir -p {_defaultOptions.ApplicationPath} {_defaultOptions.DataPath} {_defaultOptions.LogPath}");
        guideContent.Should().Contain($"curl https://{_defaultOptions.ServerDomain}/health");
        guideContent.Should().Contain($"- Database files are stored in {_defaultOptions.DataPath}");
    }

    [Fact]
    public void GenerateSystemdServiceUnit_ShouldHaveCorrectSecuritySettings()
    {
        // Act
        var unitFileContent = DeploymentConfiguration.GenerateSystemdServiceUnit(_defaultOptions);

        // Assert
        unitFileContent.Should().Contain("NoNewPrivileges=true");
        unitFileContent.Should().Contain("PrivateTmp=true");
        unitFileContent.Should().Contain("ProtectSystem=strict");
        unitFileContent.Should().Contain("ProtectHome=true");
    }

    [Fact]
    public void GenerateCaddyConfiguration_ShouldIncludeHealthCheckSettings()
    {
        // Act
        var caddyConfigContent = DeploymentConfiguration.GenerateCaddyConfiguration(_defaultOptions);

        // Assert
        caddyConfigContent.Should().Contain("uri /health");
        caddyConfigContent.Should().Contain("interval 30s");
        caddyConfigContent.Should().Contain("timeout 5s");
        caddyConfigContent.Should().Contain("unhealthy_status 500 502 503");
    }
    
    [Fact]
    public void GenerateEnvironmentFile_ShouldContainProductionEnvironment()
    {
        // Act
        var envFileContent = DeploymentConfiguration.GenerateEnvironmentFile(_defaultOptions);

        // Assert
        envFileContent.Should().Contain("ASPNETCORE_ENVIRONMENT=Production");
    }
}
