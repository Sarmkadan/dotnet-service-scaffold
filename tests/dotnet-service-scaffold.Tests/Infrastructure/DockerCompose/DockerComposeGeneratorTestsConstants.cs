#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Tests.Infrastructure.DockerCompose;

/// <summary>
/// Constants for DockerComposeGeneratorTests.
/// </summary>
internal static class DockerComposeGeneratorTestsConstants
{
    // Service names
    public const string MyApiServiceName = "my-api";
    public const string DefaultServiceName = "svc";

    // Image names
    public const string MyApiImageName = "my-api:1.0";
    public const string RedisImageName = "redis:7-alpine";

    // Port mappings
    public const int MyApiPort = 8080;
    public const string MyApiPortMapping = "8080:8080";
    public const int DefaultContainerPort = 5001;
    public const string DefaultHealthCheckUrl = "http://localhost:5001/health";

    // Caddy constants
    public const string CaddyServiceName = "caddy";
    public const string CaddyDataServiceName = "caddy-data";
    public const string CaddyDomain = "example.com";

    // Health check
    public const string HealthCheckSection = "healthcheck:";

    // Redis
    public const string RedisServiceName = "redis";

    // Environment variables
    public const string TestEnvVarName = "MY_VAR";
    public const string TestEnvVarValue = "hello";
    public const string TestEnvVarEntry = "MY_VAR: hello";

    // Prometheus
    public const string PrometheusMetricsPath = "metrics_path: /metrics";

    // Resource limits
    public const string CpuLimitValue = "2";
    public const string MemoryLimitValue = "1G";
    public const string CpusEntry = "cpus: '2'";
    public const string MemoryEntry = "memory: 1G";

    // File operations
    public const string TestComposeFilePrefix = "test-compose-";
    public const string YamlFileExtension = ".yml";
}