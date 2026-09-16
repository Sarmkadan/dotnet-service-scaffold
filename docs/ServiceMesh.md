# Service mesh integration

The service mesh integration provides an Envoy-compatible admin client for applications that run beside a proxy such as Istio Envoy or Consul Connect. `SidecarProxyService` reads proxy status and upstream health for diagnostics, probes readiness, and requests graceful connection draining during shutdown. The same application can also run without a sidecar by disabling the integration in configuration.

Registering the integration binds the `ServiceMesh` configuration section to `ServiceMeshOptions` and registers `ISidecarProxyService` as a typed HTTP client. The client uses a 10-second HTTP timeout and sends `dotnet-service-scaffold/mesh-client` as its user agent.

## Configuration options

| Option | Type | Default | Purpose |
| --- | --- | --- | --- |
| `AdminEndpoint` | `string` | `http://localhost:15000` | Base URL of the sidecar proxy admin API. Do not include a trailing slash. |
| `ReadinessTimeoutSeconds` | `int` | `5` | Maximum time allowed for a request to the proxy's `/ready` endpoint. |
| `MeshName` | `string` | `default` | Logical mesh name included in the returned `SidecarProxyInfo` for diagnostics and labeling. |
| `Enabled` | `bool` | `true` | Controls mesh detection. When `false`, `IsServiceMeshEnabledAsync` returns `false` without probing the proxy. |

`ServiceMeshOptions.Validate()` can be used to check configuration explicitly. It requires an absolute HTTP or HTTPS admin endpoint, a readiness timeout from 1 through 60 seconds, and a non-empty mesh name of at most 50 characters without whitespace. `EnsureValid()` throws an `ArgumentException` containing all validation errors. Registration binds the options but does not automatically invoke these validation helpers.

## Sidecar operations

`ISidecarProxyService` exposes the following operations:

- `GetProxyInfoAsync` calls `/server_info` and `/clusters?format=json`, returning proxy identity, version, status, and upstream cluster health. Unreachable or cancelled requests produce a disconnected status rather than escaping as those transport exceptions.
- `CheckReadinessAsync` calls `/ready` using `ReadinessTimeoutSeconds` and returns `false` when the request fails or times out.
- `GetUpstreamClustersAsync` maps Envoy cluster host health into `UpstreamCluster` values. A cluster with hosts but no healthy host is marked with an open circuit breaker.
- `DrainConnectionsAsync` posts to `/drain_listeners?inboundonly&graceful`, then waits for the requested drain period (10 seconds by default).
- `IsServiceMeshEnabledAsync` returns `false` immediately when `Enabled` is `false`; otherwise it performs the readiness check.

All operations accept a `CancellationToken`. The configured admin endpoint must expose the Envoy-compatible admin routes used above.

## Example

Add a `ServiceMesh` section to `appsettings.json`:

```json
{
  "ServiceMesh": {
    "AdminEndpoint": "http://localhost:15000",
    "ReadinessTimeoutSeconds": 5,
    "MeshName": "istio-production",
    "Enabled": true
  }
}
```

Register the integration and optionally enable propagation of common Envoy and B3 request headers:

```csharp
using DotnetServiceScaffold.Infrastructure.ServiceMesh;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceMeshIntegration(builder.Configuration);

var app = builder.Build();

app.UseServiceMeshHeaders();

app.MapGet("/mesh/health", async (
    ISidecarProxyService sidecar,
    CancellationToken cancellationToken) =>
{
    if (!await sidecar.IsServiceMeshEnabledAsync(cancellationToken))
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    var proxy = await sidecar.GetProxyInfoAsync(cancellationToken);
    return Results.Ok(proxy);
});

app.Run();
```

Call `UseServiceMeshHeaders` early in the middleware pipeline when downstream code needs the incoming mesh tracing context. It stores supported request-header values in `HttpContext.Items`; registering the sidecar client does not require this middleware.
