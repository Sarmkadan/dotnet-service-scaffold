# DockerComposeGeneratorTests

A test class that validates the behavior of the Docker Compose generator within the `dotnet-service-scaffold` project. It contains unit tests covering service name generation, optional component inclusion (Caddy, Redis, Prometheus), health check configuration, environment variable injection, resource limit settings, file output, and error handling for invalid inputs.

## API

### `public DockerComposeGeneratorTests`

Default constructor. Initializes a new instance of the test class. Takes no parameters and performs no custom setup beyond what the test framework provides.

### `public void Generate_ShouldContainServiceName_WhenOptionsProvided`

Verifies that the generated Docker Compose output includes the expected service name when valid options are supplied. Fails the test if the service name is absent or incorrect.

### `public void Generate_ShouldIncludeHealthCheck`

Ensures that a health check configuration is present in the generated output when the provided options request one. The test asserts the presence of health check YAML directives.

### `public void Generate_ShouldIncludeCaddy_WhenRequested`

Confirms that Caddy reverse-proxy configuration appears in the generated Docker Compose output when the options explicitly request Caddy inclusion. Fails if Caddy-related services or volumes are missing.

### `public void Generate_ShouldNotIncludeCaddy_WhenNotRequested`

Validates that Caddy configuration is absent from the generated output when the options do not request Caddy. The test asserts that no Caddy service definitions or references appear.

### `public void Generate_ShouldIncludeRedis_WhenRequested`

Checks that Redis service configuration is present in the generated output when Redis inclusion is requested via options. Fails if Redis-related YAML blocks are missing.

### `public void Generate_ShouldIncludeExtraEnvVars_WhenProvided`

Verifies that additional environment variables supplied through options are correctly rendered in the generated Docker Compose output. Asserts the presence of the specified key-value pairs in the environment section.

### `public void Generate_ShouldThrow_WhenOptionsIsNull`

Ensures that the generator throws an exception when null options are passed. The test expects a specific exception type (typically `ArgumentNullException`) to be raised, preventing silent failures.

### `public void Generate_ShouldIncludePrometheusComment_WhenRequested`

Confirms that a Prometheus metrics endpoint comment or annotation appears in the generated output when Prometheus support is requested. The test searches for the expected comment string in the YAML.

### `public void Generate_ShouldIncludeResourceLimits`

Validates that resource limit directives (CPU, memory reservations and caps) are included in the generated service definitions. Fails if the expected resource constraint YAML keys are absent.

### `public async Task WriteToFileAsync_ShouldWriteYamlFile`

An asynchronous test that verifies the file-writing capability of the generator. It calls the async write method, then asserts that a YAML file was created at the expected path with non-empty content. Returns a `Task` representing the ongoing test operation.

## Usage

```csharp
// Example 1: Running a single test to verify Caddy inclusion
var tests = new DockerComposeGeneratorTests();

// This test will pass if the generator produces Caddy configuration
// when the options flag for Caddy is set to true.
tests.Generate_ShouldIncludeCaddy_WhenRequested();
```

```csharp
// Example 2: Verifying file output asynchronously in a test suite
var tests = new DockerComposeGeneratorTests();

// Arrange: ensure the generator is configured with valid options.
// Act & Assert: the test awaits the async file write and checks the result.
await tests.WriteToFileAsync_ShouldWriteYamlFile();
```

## Notes

- All `Generate_*` methods are synchronous and designed to be executed by a test runner such as xUnit or NUnit. They do not return values; they communicate results through assertion failures.
- `WriteToFileAsync_ShouldWriteYamlFile` is the only asynchronous member. Callers must await it to ensure the file system operations complete before assertions are evaluated.
- The `Generate_ShouldThrow_WhenOptionsIsNull` test expects a specific exception type. Production code must throw predictably (e.g., `ArgumentNullException`) for this test to pass.
- Tests that verify optional components (Caddy, Redis, Prometheus) assume the generator respects boolean flags in the options object. Absence of a flag should result in absence of the corresponding YAML block.
- Resource limit tests assume the generator outputs standard Docker Compose `deploy.resources` syntax. Changes to the Compose specification may require test updates.
- These tests are not thread-safe in the sense that they may interact with shared file system paths during `WriteToFileAsync_ShouldWriteYamlFile`. Parallel test execution should use isolated temporary directories to avoid cross-test contamination.
- No setup or teardown methods are exposed publicly; any test initialization or cleanup is handled internally by the test class or framework.
