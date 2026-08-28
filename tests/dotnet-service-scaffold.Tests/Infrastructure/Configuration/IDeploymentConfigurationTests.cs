#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Tasks;

/// <summary>
/// Contract for tests of the DeploymentConfiguration class.
/// </summary>
public interface IDeploymentConfigurationTests
{
    void GenerateSystemdServiceUnit_ShouldContainAllExpectedValues();

    void GenerateCaddyConfiguration_ShouldContainAllExpectedValues();

    void GenerateEnvironmentFile_ShouldContainAllExpectedValues();

    void GenerateDeploymentGuide_ShouldContainAllExpectedValues();

    void GenerateSystemdServiceUnit_ShouldHaveCorrectSecuritySettings();

    void GenerateCaddyConfiguration_ShouldIncludeHealthCheckSettings();

    void GenerateEnvironmentFile_ShouldContainProductionEnvironment();
}