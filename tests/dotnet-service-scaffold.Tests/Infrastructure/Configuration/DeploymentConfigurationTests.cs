#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using DotnetServiceScaffold.Infrastructure.Configuration;
using System.Text.RegularExpressions;

namespace DotnetServiceScaffold.Tests.Infrastructure.Configuration;

/// <summary>
/// Tests for the DeploymentConfiguration class.
/// </summary>
public class DeploymentConfigurationTests : IDeploymentConfigurationTests
{
    private DeploymentOptions _defaultOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentConfigurationTests"/> class.
    /// </summary>
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

    /// <summary>
    /// Verifies that the GenerateSystemdServiceUnit method contains all expected values.
    /// </summary>
    [Fact]
    public void GenerateSystemdServiceUnit_ShouldContainAllExpectedValues()
    {
        // Act
        var unitFileContent = DeploymentConfiguration.GenerateSystemdServiceUnit(_defaultOptions);

        // Assert
        unitFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.Systemd_Description}{_defaultOptions.ServiceDescription}");
        unitFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.Systemd_User}{_defaultOptions.ServiceUser}");
        unitFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.Systemd_WorkingDirectory}{_defaultOptions.ApplicationPath}");
        unitFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.Systemd_ExecStart}{_defaultOptions.DotnetPath}{DeploymentConfigurationTestsConstants.ExecStart_Dll}");
        unitFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.Systemd_SyslogIdentifier}{_defaultOptions.ServiceName}");
        unitFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.Systemd_ReadWritePaths}{_defaultOptions.DataPath}");
    }

    /// <summary>
    /// Verifies that the GenerateCaddyConfiguration method contains all expected values.
    /// </summary>
    [Fact]
    public void GenerateCaddyConfiguration_ShouldContainAllExpectedValues()
    {
        // Act
        var caddyConfigContent = DeploymentConfiguration.GenerateCaddyConfiguration(_defaultOptions);

        // Assert
        caddyConfigContent.Should().Contain($"{DeploymentConfigurationTestsConstants.Caddy_Comment}{_defaultOptions.ServiceName}");
        caddyConfigContent.Should().Contain($"{_defaultOptions.ServerDomain}{DeploymentConfigurationTestsConstants.Caddy_OpenBrace}");
        caddyConfigContent.Should().Contain($"{DeploymentConfigurationTestsConstants.Caddy_ReverseProxy}{_defaultOptions.ApplicationPort}{DeploymentConfigurationTestsConstants.Caddy_OpenBrace}");
        caddyConfigContent.Should().Contain($"{DeploymentConfigurationTestsConstants.Caddy_OutputFile}{_defaultOptions.LogPath}{DeploymentConfigurationTestsConstants.Caddy_LogOpenBrace}");
    }

    /// <summary>
    /// Verifies that the GenerateEnvironmentFile method contains all expected values.
    /// </summary>
    [Fact]
    public void GenerateEnvironmentFile_ShouldContainAllExpectedValues()
    {
        // Act
        var envFileContent = DeploymentConfiguration.GenerateEnvironmentFile(_defaultOptions);

        // Assert
        envFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.EnvFile_Comment}{_defaultOptions.ServiceName}");
        envFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.EnvFile_Urls}{_defaultOptions.ApplicationPort}");
        envFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.EnvFile_ConnectionString}{_defaultOptions.DataPath}{DeploymentConfigurationTestsConstants.EnvFile_DbPath}");
        envFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.EnvFile_ServiceName}{_defaultOptions.ServiceName}");
        envFileContent.Should().Contain($"{DeploymentConfigurationTestsConstants.EnvFile_ServiceVersion}{_defaultOptions.ServiceVersion}");
    }

    /// <summary>
    /// Verifies that the GenerateDeploymentGuide method contains all expected values.
    /// </summary>
    [Fact]
    public void GenerateDeploymentGuide_ShouldContainAllExpectedValues()
    {
        // Act
        var guideContent = DeploymentConfiguration.GenerateDeploymentGuide(_defaultOptions);

        // Assert
        guideContent.Should().Contain($"{DeploymentConfigurationTestsConstants.DeploymentGuide_Comment}{_defaultOptions.ServiceName}");
        guideContent.Should().Contain($"{DeploymentConfigurationTestsConstants.DeploymentGuide_AddUser}{_defaultOptions.ServiceUser}");
        guideContent.Should().Contain($"{DeploymentConfigurationTestsConstants.DeploymentGuide_Mkdir}{_defaultOptions.ApplicationPath} {_defaultOptions.DataPath} {_defaultOptions.LogPath}");
        guideContent.Should().Contain($"{DeploymentConfigurationTestsConstants.DeploymentGuide_Curl}{_defaultOptions.ServerDomain}{DeploymentConfigurationTestsConstants.DeploymentGuide_HealthCheck}");
        guideContent.Should().Contain($"{DeploymentConfigurationTestsConstants.DeploymentGuide_DbPathInfo}{_defaultOptions.DataPath}");
    }

    /// <summary>
    /// Verifies that the GenerateSystemdServiceUnit method has the correct security settings.
    /// </summary>
    [Fact]
    public void GenerateSystemdServiceUnit_ShouldHaveCorrectSecuritySettings()
    {
        // Act
        var unitFileContent = DeploymentConfiguration.GenerateSystemdServiceUnit(_defaultOptions);

        // Assert
        unitFileContent.Should().Contain(DeploymentConfigurationTestsConstants.SystemdSecurity_NoNewPrivileges);
        unitFileContent.Should().Contain(DeploymentConfigurationTestsConstants.SystemdSecurity_PrivateTmp);
        unitFileContent.Should().Contain(DeploymentConfigurationTestsConstants.SystemdSecurity_ProtectSystem);
        unitFileContent.Should().Contain(DeploymentConfigurationTestsConstants.SystemdSecurity_ProtectHome);
    }

    /// <summary>
    /// Verifies that the GenerateCaddyConfiguration method includes health check settings.
    /// </summary>
    [Fact]
    public void GenerateCaddyConfiguration_ShouldIncludeHealthCheckSettings()
    {
        // Act
        var caddyConfigContent = DeploymentConfiguration.GenerateCaddyConfiguration(_defaultOptions);

        // Assert
        caddyConfigContent.Should().Contain(DeploymentConfigurationTestsConstants.CaddyHealthCheck_Uri);
        caddyConfigContent.Should().Contain($"interval {DeploymentConfigurationTestsConstants.HealthCheckIntervalSeconds}s");
        caddyConfigContent.Should().Contain($"timeout {DeploymentConfigurationTestsConstants.HealthCheckTimeoutSeconds}s");
        caddyConfigContent.Should().Contain($"unhealthy_status {string.Join(" ", DeploymentConfigurationTestsConstants.HealthCheckUnhealthyStatusCodes)}");
    }
    
    /// <summary>
    /// Verifies that the GenerateEnvironmentFile method contains the production environment.
    /// </summary>
    [Fact]
    public void GenerateEnvironmentFile_ShouldContainProductionEnvironment()
    {
        // Act
        var envFileContent = DeploymentConfiguration.GenerateEnvironmentFile(_defaultOptions);

        // Assert
        envFileContent.Should().Contain("ASPNETCORE_ENVIRONMENT=Production");
    }
}
