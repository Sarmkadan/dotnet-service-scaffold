# DeploymentConfigurationTests

`DeploymentConfigurationTests` is a test class that verifies the correctness of deployment‑related configuration generation logic within the `dotnet-service-scaffold` project. Each test method asserts that a specific configuration artifact (systemd service unit, Caddy file, environment file, or deployment guide) contains the expected values and adheres to required security or operational constraints.

## API

### `GenerateSystemdServiceUnit_ShouldContainAllExpectedValues`
- **Purpose**: Confirms that the generated systemd service unit includes all required sections and values (e.g., `Description`, `ExecStart`, `WorkingDirectory`).
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: May throw an exception from the unit‑testing framework (e.g., `Xunit.AssertException`) if any expected value is missing or incorrect.

### `GenerateCaddyConfiguration_ShouldContainAllExpectedValues`
- **Purpose**: Validates that the produced Caddy configuration contains the expected directives such as the site address, reverse proxy settings, and log format.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: May throw a testing‑framework assertion exception when the generated Caddy file deviates from the expected content.

### `GenerateEnvironmentFile_ShouldContainAllExpectedValues`
- **Purpose**: Ensures that the environment file generated for the service includes all necessary key‑value pairs (e.g., `ASPNETCORE_ENVIRONMENT`, connection strings, feature flags).
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: May throw an assertion exception if any required environment variable is absent or has an unexpected value.

### `GenerateDeploymentGuide_ShouldContainAllExpectedValues`
- **Purpose**: Checks that the deployment guide markdown contains all mandated sections (prerequisites, installation steps, service activation, verification).
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: May throw a testing‑framework exception when the guide is missing a section or contains incorrect instructions.

### `GenerateSystemdServiceUnit_ShouldHaveCorrectSecuritySettings`
- **Purpose**: Verifies that the systemd service unit includes security‑hardening directives such as `PrivateTmp=yes`, `ProtectSystem=full`, and `CapabilityBoundingSet`.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: May throw an assertion exception if any expected security setting is missing or incorrectly formatted.

### `GenerateCaddyConfiguration_ShouldIncludeHealthCheckSettings`
- **Purpose**: Asserts that the Caddy configuration includes a health check block (e.g., `import health_check`) with the expected path and interval.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: May throw an assertion exception when the health check is omitted or malformed.

### `GenerateEnvironmentFile_ShouldContainProductionEnvironment`
- **Purpose**: Confirms that the environment file sets `ASPNETCORE_ENVIRONMENT=Production` (or the appropriate production identifier) when generating for a production target.
- **Parameters**: None.
- **Return value**: `void`.
- **Exceptions**: May throw an assertion exception if the production environment variable is not present or set to an incorrect value.

## Usage

### Example 1: Running the tests with the .NET test runner
```bash
# From the repository root
dotnet test --filter FullyQualifiedName~DeploymentConfigurationTests
```
This command discovers and executes all test methods in `DeploymentConfigurationTests`, reporting success or failure for each assertion.

### Example 2: Invoking a test method manually in a custom test harness
```csharp
using Xunit;
using DotnetServiceScaffold.Tests; // namespace containing DeploymentConfigurationTests

public class CustomTestRunner
{
    [Fact]
    public void RunSystemdServiceUnitTest()
    {
        var testInstance = new DeploymentConfigurationTests();
        // The method is public and parameter‑less; invoke directly.
        testInstance.GenerateSystemdServiceUnit_ShouldContainAllExpectedValues();
        // If no exception is thrown, the test passed.
    }
}
```
In this scenario, the test class is instantiated and the desired test method is called; any assertion failure will propagate as an exception, which the test framework interprets as a failed test.

## Notes
- The test methods depend on the internal state of the `DeploymentConfiguration` generator (e.g., default values, configuration providers). If those generators rely on external files or environment variables, the tests may fail when run in an environment that deviates from the expected defaults.
- Each method is stateless and does not modify shared static data; therefore, the class is thread‑safe with respect to concurrent execution of its test methods. However, if the underlying generator uses mutable static state, concurrent test runs could produce unreliable results—ensure that any shared state is either immutable or properly synchronized when executing tests in parallel.
