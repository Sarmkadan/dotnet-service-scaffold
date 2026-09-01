#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.DockerCompose;

/// <summary>
/// Configuration options for Docker Compose file generation.
/// </summary>
public sealed class DockerComposeOptions : IEquatable<DockerComposeOptions>
{
    /// <summary>Name of the primary application service.</summary>
    public string ServiceName { get; set; } = DockerComposeOptionsConstants.DefaultServiceName;

    /// <summary>Docker image name (e.g. "myapp:latest").</summary>
    public string ImageName { get; set; } = DockerComposeOptionsConstants.DefaultImageName;

    /// <summary>Host port mapped to the application container.</summary>
    public int HostPort { get; set; } = DockerComposeOptionsConstants.DefaultHostPort;

    /// <summary>Container port the application listens on.</summary>
    public int ContainerPort { get; set; } = DockerComposeOptionsConstants.DefaultContainerPort;

    /// <summary>ASP.NET Core environment (Development / Production).</summary>
    public string Environment { get; set; } = DockerComposeOptionsConstants.DefaultEnvironment;

    /// <summary>SQLite connection string or full database connection string.</summary>
    public string ConnectionString { get; set; } = DockerComposeOptionsConstants.DefaultConnectionString;

    /// <summary>Additional environment variables to inject.</summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

    /// <summary>Named volumes to create (volume name → container path).</summary>
    public Dictionary<string, string> Volumes { get; set; } = new()
    {
        [DockerComposeOptionsConstants.VolumeAppData] = DockerComposeOptionsConstants.VolumeAppDataPath,
        [DockerComposeOptionsConstants.VolumeAppLogs] = DockerComposeOptionsConstants.VolumeAppLogsPath
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
    public string CpuLimit { get; set; } = DockerComposeOptionsConstants.DefaultCpuLimit;

    /// <summary>Memory limit for the application container (e.g. "512M").</summary>
    public string MemoryLimit { get; set; } = DockerComposeOptionsConstants.DefaultMemoryLimit;

    public bool Equals(DockerComposeOptions? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ServiceName == other.ServiceName
               && ImageName == other.ImageName
               && HostPort == other.HostPort
               && ContainerPort == other.ContainerPort
               && Environment == other.Environment
               && ConnectionString == other.ConnectionString
               && EnvironmentVariables.OrderBy(kvp => kvp.Key).SequenceEqual(other.EnvironmentVariables.OrderBy(kvp => kvp.Key))
               && Volumes.OrderBy(kvp => kvp.Key).SequenceEqual(other.Volumes.OrderBy(kvp => kvp.Key))
               && IncludeCaddy == other.IncludeCaddy
               && CaddyDomain == other.CaddyDomain
               && IncludePrometheus == other.IncludePrometheus
               && IncludeRedis == other.IncludeRedis
               && CpuLimit == other.CpuLimit
               && MemoryLimit == other.MemoryLimit;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DockerComposeOptions)obj);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ServiceName);
        hash.Add(ImageName);
        hash.Add(HostPort);
        hash.Add(ContainerPort);
        hash.Add(Environment);
        hash.Add(ConnectionString);
        foreach (var kvp in EnvironmentVariables.OrderBy(kvp => kvp.Key))
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        foreach (var kvp in Volumes.OrderBy(kvp => kvp.Key))
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        hash.Add(IncludeCaddy);
        hash.Add(CaddyDomain);
        hash.Add(IncludePrometheus);
        hash.Add(IncludeRedis);
        hash.Add(CpuLimit);
        hash.Add(MemoryLimit);
        return hash.ToHashCode();
    }

    public static bool operator ==(DockerComposeOptions? left, DockerComposeOptions? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(DockerComposeOptions? left, DockerComposeOptions? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"DockerComposeOptions {{ ServiceName = {ServiceName}, ImageName = {ImageName}, HostPort = {HostPort}, ContainerPort = {ContainerPort}, Environment = {Environment}, ConnectionString = {ConnectionString} }}";
    }
}
