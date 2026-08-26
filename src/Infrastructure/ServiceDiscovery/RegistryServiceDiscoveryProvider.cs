#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

/// <summary>
/// Resolves, registers, and deregisters service instances via a Consul-compatible
/// HTTP service registry API. Supports health-filtered queries, ACL tokens, and
/// polling-based watch streams.
/// </summary>
public sealed class RegistryServiceDiscoveryProvider : IServiceDiscoveryProvider, IRegistryServiceDiscoveryProvider
{
    internal const string HttpClientName = "ServiceDiscovery.Registry";

    private readonly IHttpClientFactory _httpFactory;
    private readonly ServiceDiscoveryOptions _options;
    private readonly ILogger<RegistryServiceDiscoveryProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc/>
    public string ProviderName => "Registry";

    /// <summary>
    /// Initialises a new <see cref="RegistryServiceDiscoveryProvider"/> using a named
    /// <see cref="IHttpClientFactory"/> client so the provider can safely be registered
    /// as a singleton without capturing a transient <see cref="HttpClient"/>.
    /// </summary>
    public RegistryServiceDiscoveryProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<ServiceDiscoveryOptions> options,
        ILogger<RegistryServiceDiscoveryProvider> logger)
    {
        _httpFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    private HttpClient CreateClient() => _httpFactory.CreateClient(HttpClientName);

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<ServiceDiscoveryRecord>>> ResolveAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reg = _options.Registry;
            var passing = reg.OnlyHealthyInstances ? "&passing=true" : string.Empty;
            var dc = string.IsNullOrEmpty(reg.Datacenter) ? string.Empty : $"&dc={reg.Datacenter}";
            var url = $"/v1/health/service/{Uri.EscapeDataString(serviceName)}?{passing}{dc}";

            using var http = CreateClient();
            var entries = await http.GetFromJsonAsync<List<ConsulServiceEntry>>(url, JsonOptions, cancellationToken)
                          ?? [];

            var records = entries.Select(MapEntry).ToList();
            _logger.LogDebug("Registry resolved {Count} instance(s) for {ServiceName}", records.Count, serviceName);
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Success(records);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Registry HTTP request failed for service {ServiceName}", serviceName);
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Failure(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error resolving {ServiceName} from registry", serviceName);
            return Result<IReadOnlyList<ServiceDiscoveryRecord>>.Failure(ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> RegisterAsync(
        ServiceDiscoveryRecord record,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var self = _options.SelfRegistration;
            var healthUrl = $"{record.Scheme}://{record.Host}:{record.Port}{self.HealthCheckPath}";

            var payload = new ConsulRegistrationPayload
            {
                Id = record.InstanceId.ToString(),
                Name = record.ServiceName,
                Tags = [.. record.Tags],
                Address = record.Host,
                Port = record.Port,
                Meta = record.Metadata,
                Check = new ConsulCheckPayload
                {
                    Http = healthUrl,
                    Interval = $"{_options.Registry.HeartbeatInterval.TotalSeconds:0}s",
                    Timeout = "5s",
                    DeregisterCriticalServiceAfter = "1m"
                }
            };

            if (!string.IsNullOrEmpty(record.Version))
                payload.Meta["version"] = record.Version;

            using var http = CreateClient();
            var response = await http.PutAsJsonAsync("/v1/agent/service/register", payload, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Registered {ServiceName}/{InstanceId} with registry", record.ServiceName, record.InstanceId);
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to register {ServiceName} with registry", record.ServiceName);
            return Result.Failure(ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> DeregisterAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var http = CreateClient();
            var response = await http.PutAsync(
                $"/v1/agent/service/deregister/{instanceId}",
                content: null,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Deregistered instance {InstanceId} from registry", instanceId);
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to deregister instance {InstanceId} from registry", instanceId);
            return Result.Failure(ex);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Polls <see cref="ResolveAsync"/> at <see cref="ServiceDiscoveryOptions.RefreshInterval"/>
    /// and yields a new snapshot whenever the set of instance IDs changes.
    /// </remarks>
    public async IAsyncEnumerable<IReadOnlyList<ServiceDiscoveryRecord>> WatchAsync(
        string serviceName,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var previousIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await ResolveAsync(serviceName, cancellationToken);
            if (result.IsSuccess && result.Value is { } current)
            {
                var currentIds = current.Select(r => r.InstanceId.ToString())
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (!currentIds.SetEquals(previousIds))
                {
                    previousIds = currentIds;
                    yield return current;
                }
            }

            try { await Task.Delay(_options.RefreshInterval, cancellationToken); }
            catch (OperationCanceledException) { yield break; }
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_options.ResolutionTimeout);
            using var http = CreateClient();
            var response = await http.GetAsync("/v1/status/leader", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns all service names currently registered in the catalog.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<Result<IReadOnlyList<string>>> GetAllServiceNamesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dc = string.IsNullOrEmpty(_options.Registry.Datacenter)
                ? string.Empty
                : $"?dc={_options.Registry.Datacenter}";

            using var http = CreateClient();
            var catalog = await http.GetFromJsonAsync<Dictionary<string, List<string>>>(
                $"/v1/catalog/services{dc}", JsonOptions, cancellationToken) ?? [];

            return Result<IReadOnlyList<string>>.Success([.. catalog.Keys]);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve service catalog from registry");
            return Result<IReadOnlyList<string>>.Failure(ex);
        }
    }

    private static ServiceDiscoveryRecord MapEntry(ConsulServiceEntry entry)
    {
        var svc = entry.Service;
        var overallStatus = entry.Checks.Count > 0
            ? MapConsulStatus(entry.Checks.Max(c => ConsulStatusRank(c.Status)))
            : DiscoveryHealthStatus.Unknown;

        return new ServiceDiscoveryRecord
        {
            InstanceId = Guid.TryParse(svc.Id, out var id) ? id : Guid.NewGuid(),
            ServiceName = svc.Service,
            Host = string.IsNullOrEmpty(svc.Address) ? entry.Node.Address : svc.Address,
            Port = svc.Port,
            Scheme = svc.Meta.TryGetValue("scheme", out var scheme) ? scheme : "http",
            Weight = svc.Weights?.Passing ?? 10,
            Tags = [.. svc.Tags],
            Metadata = svc.Meta,
            Source = DiscoverySource.Registry,
            HealthStatus = overallStatus,
            Version = svc.Meta.GetValueOrDefault("version")
        };
    }

    private static int ConsulStatusRank(string status) => status.ToLowerInvariant() switch
    {
        "passing" => 0,
        "warning" => 1,
        "critical" => 2,
        _ => 1
    };

    private static DiscoveryHealthStatus MapConsulStatus(int maxRank) => maxRank switch
    {
        0 => DiscoveryHealthStatus.Passing,
        1 => DiscoveryHealthStatus.Warning,
        _ => DiscoveryHealthStatus.Critical
    };

    // ── Internal Consul wire-format POCOs ────────────────────────────────────

    private sealed class ConsulServiceEntry
    {
        [JsonPropertyName("Node")] public ConsulNode Node { get; set; } = new();
        [JsonPropertyName("Service")] public ConsulService Service { get; set; } = new();
        [JsonPropertyName("Checks")] public List<ConsulCheck> Checks { get; set; } = [];
    }

    private sealed class ConsulNode
    {
        [JsonPropertyName("Node")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("Address")] public string Address { get; set; } = string.Empty;
    }

    private sealed class ConsulService
    {
        [JsonPropertyName("ID")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("Service")] public string Service { get; set; } = string.Empty;
        [JsonPropertyName("Tags")] public List<string> Tags { get; set; } = [];
        [JsonPropertyName("Address")] public string Address { get; set; } = string.Empty;
        [JsonPropertyName("Port")] public int Port { get; set; }
        [JsonPropertyName("Meta")] public Dictionary<string, string> Meta { get; set; } = [];
        [JsonPropertyName("Weights")] public ConsulWeights? Weights { get; set; }
    }

    private sealed class ConsulWeights
    {
        [JsonPropertyName("Passing")] public int Passing { get; set; } = 10;
        [JsonPropertyName("Warning")] public int Warning { get; set; } = 1;
    }

    private sealed class ConsulCheck
    {
        [JsonPropertyName("Status")] public string Status { get; set; } = "unknown";
    }

    private sealed class ConsulRegistrationPayload
    {
        [JsonPropertyName("ID")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("Name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("Tags")] public List<string> Tags { get; set; } = [];
        [JsonPropertyName("Address")] public string Address { get; set; } = string.Empty;
        [JsonPropertyName("Port")] public int Port { get; set; }
        [JsonPropertyName("Meta")] public Dictionary<string, string> Meta { get; set; } = [];
        [JsonPropertyName("Check")] public ConsulCheckPayload? Check { get; set; }
    }

    private sealed class ConsulCheckPayload
    {
        [JsonPropertyName("HTTP")] public string Http { get; set; } = string.Empty;
        [JsonPropertyName("Interval")] public string Interval { get; set; } = "10s";
        [JsonPropertyName("Timeout")] public string Timeout { get; set; } = "5s";
        [JsonPropertyName("DeregisterCriticalServiceAfter")]
        public string DeregisterCriticalServiceAfter { get; set; } = "1m";
    }
}
