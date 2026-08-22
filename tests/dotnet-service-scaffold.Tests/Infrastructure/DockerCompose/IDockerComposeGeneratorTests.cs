namespace DotnetServiceScaffold.Tests.Infrastructure.DockerCompose;

using System.Threading.Tasks;

/// <summary>
/// Interface for DockerComposeGeneratorTests.
/// </summary>
public interface IDockerComposeGeneratorTests
{
    void Generate_ShouldContainServiceName_WhenOptionsProvided();
    void Generate_ShouldIncludeHealthCheck();
    void Generate_ShouldIncludeCaddy_WhenRequested();
    void Generate_ShouldNotIncludeCaddy_WhenNotRequested();
    void Generate_ShouldIncludeRedis_WhenRequested();
    void Generate_ShouldIncludeExtraEnvVars_WhenProvided();
    void Generate_ShouldThrow_WhenOptionsIsNull();
    void Generate_ShouldIncludePrometheusComment_WhenRequested();
    void Generate_ShouldIncludeResourceLimits();
    Task WriteToFileAsync_ShouldWriteYamlFile();
}
