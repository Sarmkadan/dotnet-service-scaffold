using Xunit;
using DotnetServiceScaffold.Infrastructure.DockerCompose;

namespace DotnetServiceScaffold.Tests.Infrastructure.DockerCompose;

public class DockerComposeOptionsTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var options = new DockerComposeOptions();

        // Assert
        Assert.Equal("app", options.ServiceName);
        Assert.Equal("dotnet-service-scaffold:latest", options.ImageName);
        Assert.Equal(5000, options.HostPort);
        Assert.Equal(5000, options.ContainerPort);
        Assert.Equal("Production", options.Environment);
        Assert.Equal("Data Source=/app/data/scaffold.db", options.ConnectionString);
        Assert.False(options.IncludeCaddy);
        Assert.Null(options.CaddyDomain);
        Assert.False(options.IncludePrometheus);
        Assert.False(options.IncludeRedis);
        Assert.Equal("1", options.CpuLimit);
        Assert.Equal("512M", options.MemoryLimit);
    }

    [Fact]
    public void EnvironmentVariables_IsInitializedAndMutable()
    {
        // Arrange
        var options = new DockerComposeOptions();

        // Act
        options.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
        options.EnvironmentVariables["PORT"] = "8080";

        // Assert
        Assert.NotNull(options.EnvironmentVariables);
        Assert.Equal(2, options.EnvironmentVariables.Count);
        Assert.Equal("Development", options.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"]);
        Assert.Equal("8080", options.EnvironmentVariables["PORT"]);
    }

    [Fact]
    public void Volumes_ContainsDefaultMappings()
    {
        // Arrange
        var options = new DockerComposeOptions();

        // Assert
        Assert.NotNull(options.Volumes);
        Assert.Equal(2, options.Volumes.Count);
        Assert.True(options.Volumes.ContainsKey("app-data"));
        Assert.Equal("/app/data", options.Volumes["app-data"]);
        Assert.True(options.Volumes.ContainsKey("app-logs"));
        Assert.Equal("/app/logs", options.Volumes["app-logs"]);
    }

    [Fact]
    public void Properties_CanBeModified()
    {
        // Arrange
        var options = new DockerComposeOptions();

        // Act
        options.ServiceName = "custom-app";
        options.HostPort = 8080;
        options.IncludeCaddy = true;
        options.CaddyDomain = "example.com";
        options.IncludeRedis = true;

        // Assert
        Assert.Equal("custom-app", options.ServiceName);
        Assert.Equal(8080, options.HostPort);
        Assert.True(options.IncludeCaddy);
        Assert.Equal("example.com", options.CaddyDomain);
        Assert.True(options.IncludeRedis);
    }
}
