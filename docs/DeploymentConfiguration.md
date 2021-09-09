# DeploymentConfiguration
The `DeploymentConfiguration` type is designed to provide a centralized configuration for deploying .NET services, encapsulating essential settings and generation methods for various deployment artifacts. It serves as a key component in streamlining the deployment process, ensuring consistency and accuracy across different environments.

## API
The `DeploymentConfiguration` type exposes the following public members:
* `public static string GenerateSystemdServiceUnit`: Generates a systemd service unit file content based on the current configuration. This method does not take any parameters and returns the generated content as a string. It may throw exceptions if the configuration is invalid or if there are issues generating the content.
* `public static string GenerateCaddyConfiguration`: Generates a Caddy configuration file content based on the current configuration. This method does not take any parameters and returns the generated content as a string. It may throw exceptions if the configuration is invalid or if there are issues generating the content.
* `public static string GenerateEnvironmentFile`: Generates an environment file content based on the current configuration. This method does not take any parameters and returns the generated content as a string. It may throw exceptions if the configuration is invalid or if there are issues generating the content.
* `public static string GenerateDeploymentGuide`: Generates a deployment guide based on the current configuration. This method does not take any parameters and returns the generated guide as a string. It may throw exceptions if the configuration is invalid or if there are issues generating the guide.
* `public string ServiceName`: Gets or sets the name of the service.
* `public string ServiceDescription`: Gets or sets the description of the service.
* `public string ServiceUser`: Gets or sets the user under which the service runs.
* `public string ApplicationPath`: Gets or sets the path to the application.
* `public string DataPath`: Gets or sets the path to the data directory.
* `public string LogPath`: Gets or sets the path to the log directory.
* `public string ServerDomain`: Gets or sets the domain of the server.
* `public int ApplicationPort`: Gets or sets the port on which the application listens.
* `public string DotnetPath`: Gets or sets the path to the .NET runtime.
* `public string ServiceVersion`: Gets or sets the version of the service.

## Usage
Here are two examples of using the `DeploymentConfiguration` type:
```csharp
// Example 1: Generating deployment artifacts
var config = new DeploymentConfiguration
{
    ServiceName = "MyService",
    ServiceDescription = "My service description",
    ServiceUser = "myuser",
    ApplicationPath = "/path/to/app",
    DataPath = "/path/to/data",
    LogPath = "/path/to/logs",
    ServerDomain = "example.com",
    ApplicationPort = 8080,
    DotnetPath = "/path/to/dotnet",
    ServiceVersion = "1.0.0"
};

var systemdUnit = DeploymentConfiguration.GenerateSystemdServiceUnit;
var caddyConfig = DeploymentConfiguration.GenerateCaddyConfiguration;
var envFile = DeploymentConfiguration.GenerateEnvironmentFile;
var deploymentGuide = DeploymentConfiguration.GenerateDeploymentGuide;

// Example 2: Customizing the deployment configuration
var customConfig = new DeploymentConfiguration
{
    ServiceName = "CustomService",
    ServiceDescription = "Custom service description",
    ServiceUser = "customuser",
    ApplicationPath = "/custom/path/to/app",
    DataPath = "/custom/path/to/data",
    LogPath = "/custom/path/to/logs",
    ServerDomain = "custom.example.com",
    ApplicationPort = 8081,
    DotnetPath = "/custom/path/to/dotnet",
    ServiceVersion = "2.0.0"
};

var customSystemdUnit = DeploymentConfiguration.GenerateSystemdServiceUnit;
var customCaddyConfig = DeploymentConfiguration.GenerateCaddyConfiguration;
var customEnvFile = DeploymentConfiguration.GenerateEnvironmentFile;
var customDeploymentGuide = DeploymentConfiguration.GenerateDeploymentGuide;
```

## Notes
When using the `DeploymentConfiguration` type, consider the following:
* The `GenerateSystemdServiceUnit`, `GenerateCaddyConfiguration`, `GenerateEnvironmentFile`, and `GenerateDeploymentGuide` methods are static and do not depend on the instance configuration. Therefore, they can be called without creating an instance of the `DeploymentConfiguration` type.
* The `ServiceName`, `ServiceDescription`, `ServiceUser`, `ApplicationPath`, `DataPath`, `LogPath`, `ServerDomain`, `ApplicationPort`, `DotnetPath`, and `ServiceVersion` properties are instance-specific and must be set on an instance of the `DeploymentConfiguration` type.
* The `DeploymentConfiguration` type is not thread-safe, as it relies on instance state. If multiple threads need to access or modify the configuration, synchronization mechanisms should be employed to ensure data integrity.
* The `GenerateSystemdServiceUnit`, `GenerateCaddyConfiguration`, `GenerateEnvironmentFile`, and `GenerateDeploymentGuide` methods may throw exceptions if the configuration is invalid or if there are issues generating the content. It is essential to handle these exceptions properly to ensure robust deployment processes.
