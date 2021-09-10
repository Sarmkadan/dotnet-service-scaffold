# DockerComposeOptions

Represents the complete set of configuration values required to generate a Docker Compose file for a service scaffolded by the `dotnet-service-scaffold` tool. This type captures service identity, port mappings, environment configuration, volume mounts, optional companion services (Caddy reverse proxy, Prometheus metrics, Redis caching), and resource constraints.

## API

### `ServiceName`
- **Type:** `string`
- **Purpose:** The logical name assigned to the service within the generated Docker Compose file. This name is used as the service key under the `services` top-level element and serves as the DNS hostname for inter-container communication on the default Compose network.
- **Constraints:** Must be a non-empty string conforming to Docker Compose service name rules (alphanumeric characters, underscores, hyphens). No validation is performed by this type itself.

### `ImageName`
- **Type:** `string`
- **Purpose:** The Docker image reference used for the service container. Typically includes the repository name and tag (e.g., `myapp:latest`). This value is written directly into the `image` field of the generated service definition.
- **Constraints:** Must be a non-empty string. The caller is responsible for ensuring the image exists or can be built.

### `HostPort`
- **Type:** `int`
- **Purpose:** The port on the Docker host that will be mapped to the container's exposed port. This value appears on the left side of the port mapping (`hostPort:containerPort`).
- **Constraints:** Must be a valid port number (1–65535). No range checking is enforced by this type.

### `ContainerPort`
- **Type:** `int`
- **Purpose:** The port inside the container that the application listens on. This value appears on the right side of the port mapping (`hostPort:containerPort`) and is also used to set the `ASPNETCORE_URLS` environment variable when scaffolding ASP.NET Core projects.
- **Constraints:** Must be a valid port number (1–65535).

### `Environment`
- **Type:** `string`
- **Purpose:** A shorthand identifier for the deployment environment (e.g., `"Development"`, `"Staging"`, `"Production"`). This value is injected into the container as the `ASPNETCORE_ENVIRONMENT` environment variable and may influence which `appsettings.*.json` file is loaded.
- **Constraints:** No fixed set of allowed values; any non-null string is accepted.

### `ConnectionString`
- **Type:** `string`
- **Purpose:** A database or service connection string passed to the container as the `ConnectionStrings__DefaultConnection` environment variable. Used to configure the application's data access layer at runtime without hardcoding credentials in the image.
- **Constraints:** May be empty if no database is required. The value is treated opaquely; no format validation is performed.

### `EnvironmentVariables`
- **Type:** `Dictionary<string, string>`
- **Purpose:** A collection of additional environment variables to inject into the container. Each key-value pair becomes an entry under the `environment` section of the generated service definition. Useful for feature flags, API keys, or custom configuration beyond the standard `ASPNETCORE_ENVIRONMENT` and connection string.
- **Constraints:** Keys must be non-null, non-empty strings. Values may be empty strings. Duplicate keys are not permitted by the dictionary contract.

### `Volumes`
- **Type:** `Dictionary<string, string>`
- **Purpose:** Defines volume mounts for the service container. Each entry maps a host path or named volume (key) to a container path (value). Written into the `volumes` section of the service definition.
- **Constraints:** Both keys and values must be non-null, non-empty strings. Path validity is the caller's responsibility.

### `IncludeCaddy`
- **Type:** `bool`
- **Purpose:** When `true`, a Caddy reverse proxy service is added to the generated Docker Compose file. Caddy is configured to route traffic to the scaffolded service and automatically handle TLS certificates if a domain is provided.
- **Default:** `false`.

### `CaddyDomain`
- **Type:** `string?`
- **Purpose:** The public domain name that Caddy should serve and for which it should obtain TLS certificates. Only meaningful when `IncludeCaddy` is `true`. If `null` or empty while `IncludeCaddy` is `true`, Caddy may be generated with a placeholder or internal-only configuration.
- **Constraints:** Nullable reference type; `null` indicates no domain is specified.

### `IncludePrometheus`
- **Type:** `bool`
- **Purpose:** When `true`, a Prometheus metrics collection service is added to the generated Docker Compose file, pre-configured to scrape metrics from the scaffolded service's `/metrics` endpoint.
- **Default:** `false`.

### `IncludeRedis`
- **Type:** `bool`
- **Purpose:** When `true`, a Redis cache service is added to the generated Docker Compose file. The scaffolded service's connection string or environment variables are adjusted to point to this Redis instance.
- **Default:** `false`.

### `CpuLimit`
- **Type:** `string`
- **Purpose:** The CPU resource limit applied to the service container, expressed in Docker's resource constraint format (e.g., `"0.5"` for half a core, `"2.0"` for two cores). Written under the `deploy.resources.limits.cpus` field.
- **Constraints:** Must be a string parseable by Docker as a CPU quota. No validation is performed by this type.

### `MemoryLimit`
- **Type:** `string`
- **Purpose:** The memory resource limit applied to the service container, expressed in Docker's resource constraint format (e.g., `"256m"`, `"1g"`). Written under the `deploy.resources.limits.memory` field.
- **Constraints:** Must be a string parseable by Docker as a memory quantity. No validation is performed by this type.

## Usage

### Example 1: Basic ASP.NET Core Service with Caddy and Prometheus

```csharp
var options = new DockerComposeOptions
{
    ServiceName = "web-api",
    ImageName = "registry.example.com/web-api:1.2.0",
    HostPort = 8080,
    ContainerPort = 80,
    Environment = "Production",
    ConnectionString = "Host=db;Database=app;Username=user;Password=secret",
    EnvironmentVariables = new Dictionary<string, string>
    {
        ["Logging__LogLevel__Default"] = "Information",
        ["FeatureFlags__EnableNewCheckout"] = "true"
    },
    Volumes = new Dictionary<string, string>
    {
        ["app-logs"] = "/var/log/app"
    },
    IncludeCaddy = true,
    CaddyDomain = "api.example.com",
    IncludePrometheus = true,
    CpuLimit = "1.0",
    MemoryLimit = "512m"
};

// The scaffold tool consumes this options object to produce
// a docker-compose.yml with the web-api service, a Caddy
// reverse proxy handling TLS for api.example.com, and a
// Prometheus instance scraping /metrics.
```

### Example 2: Development Setup with Redis and Custom Ports

```csharp
var options = new DockerComposeOptions
{
    ServiceName = "worker-service",
    ImageName = "worker-service:dev",
    HostPort = 5000,
    ContainerPort = 5000,
    Environment = "Development",
    ConnectionString = "",
    EnvironmentVariables = new Dictionary<string, string>
    {
        ["DOTNET_ENVIRONMENT"] = "Development",
        ["Redis__ConnectionString"] = "redis:6379"
    },
    Volumes = new Dictionary<string, string>
    {
        ["./src"] = "/app/src",
        ["./data"] = "/app/data"
    },
    IncludeCaddy = false,
    IncludePrometheus = false,
    IncludeRedis = true,
    CpuLimit = "0.5",
    MemoryLimit = "256m"
};

// Generates a compose file with the worker-service container
// and a Redis companion. Source code and data directories
// are bind-mounted for live development reload.
```

## Notes

- **Thread Safety:** `DockerComposeOptions` is a plain data object with no internal synchronization. It is not thread-safe for concurrent reads and writes. If multiple threads mutate properties or dictionary contents simultaneously, external locking must be applied by the caller.
- **Dictionary Mutability:** The `EnvironmentVariables` and `Volumes` dictionaries are mutable reference types. After assigning them to an instance, further additions or removals will be reflected in any downstream code that holds a reference to the same dictionary. To prevent unintended mutations, pass a frozen copy or treat the options object as immutable once handed off to the scaffold generator.
- **Nullability of `CaddyDomain`:** When `IncludeCaddy` is `true` and `CaddyDomain` is `null` or whitespace, the generated Caddy configuration may omit TLS directives or produce a `Caddyfile` that listens only on HTTP. Consumers should validate this combination if TLS is required.
- **Resource Limit Format:** The `CpuLimit` and `MemoryLimit` strings are passed through to the Compose file verbatim. Invalid formats (e.g., `"abc"` for memory) will cause Docker Compose to fail at deployment time, not during scaffolding. Callers should validate these against Docker's resource constraint syntax if early error detection is desired.
- **Port Conflicts:** No uniqueness checks are performed on `HostPort`. If multiple scaffolded services are combined in the same Compose file with identical host ports, Docker will report a port conflict at runtime. The caller is responsible for coordinating port assignments across services.
- **Empty Connection Strings:** An empty `ConnectionString` is permitted and results in the `ConnectionStrings__DefaultConnection` variable being set to an empty string. The application must handle this gracefully, typically by disabling database-dependent features or falling back to in-memory providers.
