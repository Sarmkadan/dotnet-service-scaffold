#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.DockerCompose;

/// <summary>
/// Constants for DockerComposeOptions.
/// </summary>
internal static class DockerComposeOptionsConstants
{
    /// <summary>Default name of the primary application service.</summary>
    public const string DefaultServiceName = "app";

    /// <summary>Default Docker image name (e.g. "myapp:latest").</summary>
    public const string DefaultImageName = "dotnet-service-scaffold:latest";

    /// <summary>Default host port mapped to the application container.</summary>
    public const int DefaultHostPort = 5000;

    /// <summary>Default container port the application listens on.</summary>
    public const int DefaultContainerPort = 5000;

    /// <summary>Default ASP.NET Core environment (Development / Production).</summary>
    public const string DefaultEnvironment = "Production";

    /// <summary>Default SQLite connection string or full database connection string.</summary>
    public const string DefaultConnectionString = "Data Source=/app/data/scaffold.db";

    /// <summary>Default CPU limit for the application container (e.g. "1").</summary>
    public const string DefaultCpuLimit = "1";

    /// <summary>Default memory limit for the application container (e.g. "512M").</summary>
    public const string DefaultMemoryLimit = "512M";

    /// <summary>Volume name for application data.</summary>
    public const string VolumeAppData = "app-data";

    /// <summary>Container path for application data volume.</summary>
    public const string VolumeAppDataPath = "/app/data";

    /// <summary>Volume name for application logs.</summary>
    public const string VolumeAppLogs = "app-logs";

    /// <summary>Container path for application logs volume.</summary>
    public const string VolumeAppLogsPath = "/app/logs";
}