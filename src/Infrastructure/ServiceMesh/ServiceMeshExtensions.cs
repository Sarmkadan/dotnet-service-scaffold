#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.ServiceMesh;

/// <summary>
/// Configuration options for the service mesh sidecar proxy integration.
/// Bind this section from appsettings.json under the "ServiceMesh" key.
/// </summary>
public class ServiceMeshOptions : IServiceMeshOptions
{
    /// <summary>Configuration section key used in appsettings.json.</summary>
    public const string SectionName = "ServiceMesh";

    /// <summary>
    /// Base URL of the sidecar proxy admin API.
    /// Defaults to the Envoy/Istio standard admin port (15000).
    /// </summary>
    public string AdminEndpoint { get; set; } = "http://localhost:15000";

    /// <summary>
    /// Maximum seconds to wait when probing for sidecar readiness.
    /// Keep this short to avoid blocking application startup.
    /// </summary>
    public int ReadinessTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Logical name of the service mesh environment used for diagnostics and labelling.
    /// Examples: "istio", "linkerd", "consul-connect".
    /// </summary>
    public string MeshName { get; set; } = "default";

    /// <summary>
    /// When false, all service mesh calls are skipped and
    /// <see cref="ISidecarProxyService.IsServiceMeshEnabledAsync"/> returns false immediately.
    /// Allows the same binary to run with or without a sidecar present.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Extension methods to register service mesh components into the DI container
/// and configure the ASP.NET Core request pipeline.
/// </summary>
public static class ServiceMeshExtensions
{
    /// <summary>
    /// Registers the <see cref="ISidecarProxyService"/> and its named HTTP client.
    /// Reads configuration from the <c>ServiceMesh</c> section of <paramref name="configuration"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing service mesh settings.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddServiceMeshIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ServiceMeshOptions>(configuration.GetSection(ServiceMeshOptions.SectionName));

        var adminEndpoint = configuration[$"{ServiceMeshOptions.SectionName}:{nameof(ServiceMeshOptions.AdminEndpoint)}"]
            ?? "http://localhost:15000";

        services.AddHttpClient<ISidecarProxyService, SidecarProxyService>(client =>
        {
            client.BaseAddress = new Uri(adminEndpoint);
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "dotnet-service-scaffold/mesh-client");
        });

        return services;
    }

    /// <summary>
    /// Adds the service mesh header propagation middleware to the request pipeline.
    /// Captures Envoy/B3 tracing headers from inbound requests and stores them in
    /// <see cref="HttpContext.Items"/> for downstream handlers to forward.
    /// Should be placed early in the pipeline, before authentication middleware.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static WebApplication UseServiceMeshHeaders(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<ServiceMeshHeaderPropagationMiddleware>();
        return app;
    }
}

/// <summary>
/// Captures service mesh propagation headers (trace IDs, span IDs, request correlation IDs)
/// from inbound requests and stores them in <see cref="HttpContext.Items"/>.
/// Downstream handlers or delegating HTTP handlers can read these values to forward
/// the mesh context to outbound calls, preserving distributed trace continuity.
/// </summary>
internal sealed class ServiceMeshHeaderPropagationMiddleware
{
    /// <summary>Well-known headers injected by Envoy-compatible sidecars.</summary>
    private static readonly string[] PropagationHeaders =
    [
        "x-request-id",
        "x-b3-traceid",
        "x-b3-spanid",
        "x-b3-parentspanid",
        "x-b3-sampled",
        "x-b3-flags",
        "x-envoy-attempt-count"
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<ServiceMeshHeaderPropagationMiddleware> _logger;

    /// <summary>
    /// Initializes the middleware with the next delegate and a logger.
    /// </summary>
    /// <param name="next">The next <see cref="RequestDelegate"/> in the pipeline.</param>
    /// <param name="logger">The logger for this middleware.</param>
    /// <exception cref="ArgumentNullException"><paramref name="next"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
    public ServiceMeshHeaderPropagationMiddleware(
        RequestDelegate next,
        ILogger<ServiceMeshHeaderPropagationMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);

        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Extracts mesh propagation headers and stores them under the <c>mesh:</c> prefix
    /// in <see cref="HttpContext.Items"/>, then continues the pipeline.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var header in PropagationHeaders)
        {
            if (context.Request.Headers.TryGetValue(header, out var value))
            {
                context.Items[$"mesh:{header}"] = value.ToString();
            }
        }

        if (context.Items.TryGetValue("mesh:x-request-id", out var requestId))
        {
            _logger.LogDebug("Mesh request propagated with ID {RequestId}", requestId);
        }

        await _next(context);
    }
}