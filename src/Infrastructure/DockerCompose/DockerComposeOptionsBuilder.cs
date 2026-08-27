#nullable enable
namespace DotnetServiceScaffold.Infrastructure.DockerCompose;

/// <summary>
/// Builder for <see cref="DockerComposeOptions"/> objects.
/// </summary>
public class DockerComposeOptionsBuilder
{
    private readonly DockerComposeOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockerComposeOptionsBuilder"/> class with default values.
    /// </summary>
    public DockerComposeOptionsBuilder()
    {
        _options = new DockerComposeOptions();
    }

    private DockerComposeOptionsBuilder(DockerComposeOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Creates a new builder initialized with values from the specified <see cref="DockerComposeOptions"/>.
    /// </summary>
    /// <param name="template">The options to copy values from.</param>
    /// <returns>A new builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static DockerComposeOptionsBuilder From(DockerComposeOptions template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new DockerComposeOptionsBuilder(new DockerComposeOptions
        {
            ServiceName = template.ServiceName,
            ImageName = template.ImageName,
            HostPort = template.HostPort,
            ContainerPort = template.ContainerPort,
            Environment = template.Environment,
            ConnectionString = template.ConnectionString,
            EnvironmentVariables = new Dictionary<string, string>(template.EnvironmentVariables),
            Volumes = new Dictionary<string, string>(template.Volumes),
            IncludeCaddy = template.IncludeCaddy,
            CaddyDomain = template.CaddyDomain,
            IncludePrometheus = template.IncludePrometheus,
            IncludeRedis = template.IncludeRedis,
            CpuLimit = template.CpuLimit,
            MemoryLimit = template.MemoryLimit
        });
    }

    /// <summary>
    /// Sets the service name.
    /// </summary>
    /// <param name="serviceName">The service name.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serviceName"/> is null, empty, or consists only of white-space.</exception>
    public DockerComposeOptionsBuilder WithServiceName(string serviceName)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        _options.ServiceName = serviceName;
        return this;
    }

    /// <summary>
    /// Sets the Docker image name.
    /// </summary>
    /// <param name="imageName">The Docker image name (e.g. "myapp:latest").</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="imageName"/> is null, empty, or consists only of white-space.</exception>
    public DockerComposeOptionsBuilder WithImageName(string imageName)
    {
        ArgumentException.ThrowIfNullOrEmpty(imageName);
        _options.ImageName = imageName;
        return this;
    }

    /// <summary>
    /// Sets the host port.
    /// </summary>
    /// <param name="hostPort">The host port (must be between 1 and 65535).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="hostPort"/> is less than 1 or greater than 65535.</exception>
    public DockerComposeOptionsBuilder WithHostPort(int hostPort)
    {
        if (hostPort < 1 || hostPort > 65535)
            throw new ArgumentOutOfRangeException(nameof(hostPort), "Host port must be between 1 and 65535.");
        _options.HostPort = hostPort;
        return this;
    }

    /// <summary>
    /// Sets the container port.
    /// </summary>
    /// <param name="containerPort">The container port (must be between 1 and 65535).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="containerPort"/> is less than 1 or greater than 65535.</exception>
    public DockerComposeOptionsBuilder WithContainerPort(int containerPort)
    {
        if (containerPort < 1 || containerPort > 65535)
            throw new ArgumentOutOfRangeException(nameof(containerPort), "Container port must be between 1 and 65535.");
        _options.ContainerPort = containerPort;
        return this;
    }

    /// <summary>
    /// Sets the ASP.NET Core environment.
    /// </summary>
    /// <param name="environment">The environment (e.g. "Development" or "Production").</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="environment"/> is null, empty, or consists only of white-space.</exception>
    public DockerComposeOptionsBuilder WithEnvironment(string environment)
    {
        ArgumentException.ThrowIfNullOrEmpty(environment);
        _options.Environment = environment;
        return this;
    }

    /// <summary>
    /// Sets the connection string.
    /// </summary>
    /// <param name="connectionString">The connection string (e.g. SQLite connection string).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null, empty, or consists only of white-space.</exception>
    public DockerComposeOptionsBuilder WithConnectionString(string connectionString)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);
        _options.ConnectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Sets the environment variables.
    /// </summary>
    /// <param name="environmentVariables">The environment variables to inject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="environmentVariables"/> is null.</exception>
    public DockerComposeOptionsBuilder WithEnvironmentVariables(Dictionary<string, string> environmentVariables)
    {
        ArgumentNullException.ThrowIfNull(environmentVariables);
        _options.EnvironmentVariables = environmentVariables;
        return this;
    }

    /// <summary>
    /// Sets the named volumes.
    /// </summary>
    /// <param name="volumes">The named volumes (volume name → container path).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="volumes"/> is null.</exception>
    public DockerComposeOptionsBuilder WithVolumes(Dictionary<string, string> volumes)
    {
        ArgumentNullException.ThrowIfNull(volumes);
        _options.Volumes = volumes;
        return this;
    }

    /// <summary>
    /// Sets whether to include a Caddy reverse-proxy service.
    /// </summary>
    /// <param name="includeCaddy">Whether to include Caddy.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DockerComposeOptionsBuilder WithIncludeCaddy(bool includeCaddy)
    {
        _options.IncludeCaddy = includeCaddy;
        return this;
    }

    /// <summary>
    /// Sets the domain name for the Caddy service.
    /// </summary>
    /// <param name="caddyDomain">The domain name for the Caddy service (required when <see cref="WithIncludeCaddy"/> is true).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="caddyDomain"/> is null, empty, or consists only of white-space.</exception>
    public DockerComposeOptionsBuilder WithCaddyDomain(string caddyDomain)
    {
        ArgumentException.ThrowIfNullOrEmpty(caddyDomain);
        _options.CaddyDomain = caddyDomain;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="DockerComposeOptions"/> instance.
    /// </summary>
    /// <returns>A configured <see cref="DockerComposeOptions"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <see cref="IncludeCaddy"/> is true and <see cref="CaddyDomain"/> is null, empty, or consists only of white-space.</exception>
    public DockerComposeOptions Build()
    {
        if (_options.IncludeCaddy && string.IsNullOrWhiteSpace(_options.CaddyDomain))
            throw new ArgumentException("CaddyDomain is required when IncludeCaddy is true.", nameof(_options.CaddyDomain));

        return _options;
    }
}