#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.DockerCompose;

/// <summary>
/// Configuration options for Docker Compose file generation.
/// </summary>
public sealed class DockerComposeOptions
{
    /// <summary>Name of the primary application service.</summary>
    public string ServiceName { get; set; } = "app";

    /// <summary>Docker image name (e.g. "myapp:latest").</summary>
    public string ImageName { get; set; } = "dotnet-service-scaffold:latest";

    /// <summary>Host port mapped to the application container.</summary>
    public int HostPort { get; set; } = 5000;

    /// <summary>Container port the application listens on.</summary>
    public int ContainerPort { get; set; } = 5000;

    /// <summary>ASP.NET Core environment (Development / Production).</summary>
    public string Environment { get; set; } = "Production";

    /// <summary>SQLite connection string or full database connection string.</summary>
    public string ConnectionString { get; set; } = "Data Source=/app/data/scaffold.db";

    /// <summary>Additional environment variables to inject.</summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

    /// <summary>Named volumes to create (volume name → container path).</summary>
    public Dictionary<string, string> Volumes { get; set; } = new()
    {
        ["app-data"] = "/app/data",
        ["app-logs"] = "/app/logs"
    };

    /// <summary>Whether to include a Caddy reverse-proxy service.</summary>
    public bool IncludeCaddy { get; set; }

    /// <summary>Domain name for the Caddy service (required when IncludeCaddy is true).</summary>
    public string? CaddyDomain { get; set; }

    /// <summary>Whether to include a Prometheus scrape target sidecar comment block.</summary>
    public bool IncludePrometheus { get; set; }

    /// <summary>Whether to include a Redis service for distributed caching.</summary>
    public bool IncludeRedis { get; set; }

    /// <summary>CPU limit for the application container (e.g. "1").</summary>
    public string CpuLimit { get; set; } = "1";

    /// <summary>Memory limit for the application container (e.g. "512M").</summary>
    public string MemoryLimit { get; set; } = "512M";
}
