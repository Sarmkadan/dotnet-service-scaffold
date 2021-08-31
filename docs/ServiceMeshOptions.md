# ServiceMeshOptions

`ServiceMeshOptions` provides a strongly-typed configuration structure for integrating ASP.NET Core services with service mesh infrastructure. It centralizes settings necessary for administrative communication, service readiness management, and traffic header propagation, facilitating consistent service mesh behavior across the application.

## API

### Properties

- **`AdminEndpoint`** (`string`)
  - The URL utilized for service mesh administrative operations or sidecar management endpoints.
- **`ReadinessTimeoutSeconds`** (`int`)
  - The maximum duration, in seconds, the service waits before declaring itself ready to accept traffic.
- **`MeshName`** (`string`)
  - The identifier of the service mesh environment to which the service belongs.
- **`Enabled`** (`bool`)
  - A feature toggle that controls whether service mesh integration features are active.

### Methods and Classes

- **`AddServiceMeshIntegration(IServiceCollection services, Action<ServiceMeshOptions> configure)`** (`static`)
  - Registers the service mesh configuration and required services into the dependency injection container.
  - **Parameters**: `IServiceCollection`, `Action<ServiceMeshOptions>`.
  - **Returns**: `IServiceCollection`.
- **`UseServiceMeshHeaders(WebApplication app)`** (`static`)
  - Configures the application pipeline to utilize `ServiceMeshHeaderPropagationMiddleware` for processing incoming requests.
  - **Parameters**: `WebApplication`.
  - **Returns**: `WebApplication`.
- **`ServiceMeshHeaderPropagationMiddleware`**
  - A middleware component responsible for intercepting incoming HTTP requests, inspecting service mesh-specific headers, and propagating them to downstream components.
- **`InvokeAsync(HttpContext context, RequestDelegate next)`**
  - Executes the middleware logic.
  - **Parameters**: `HttpContext`, `RequestDelegate`.
  - **Returns**: `Task` (asynchronously processed request).
  - **Throws**: May throw standard exceptions related to HTTP context processing or network failures during propagation.

## Usage

### DI Registration in Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceMeshIntegration(options =>
{
    options.Enabled = true;
    options.MeshName = "prod-mesh-east";
    options.AdminEndpoint = "http://sidecar:8001";
    options.ReadinessTimeoutSeconds = 30;
});

var app = builder.Build();
```

### Applying Middleware
```csharp
var app = builder.Build();

// Configure the HTTP request pipeline to use header propagation
app.UseServiceMeshHeaders();

app.MapControllers();
app.Run();
```

## Notes

- **Thread-Safety**: The `ServiceMeshOptions` class is intended for read-only configuration post-initialization. The `ServiceMeshHeaderPropagationMiddleware.InvokeAsync` method is designed to handle concurrent requests and must be thread-safe regarding shared state.
- **Validation**: Ensure `AdminEndpoint` is a valid, reachable URI when `Enabled` is true. Providing a `ReadinessTimeoutSeconds` value less than or equal to zero may lead to immediate readiness failure or unintended behavior depending on the underlying mesh configuration.
- **Dependency**: The `ServiceMeshHeaderPropagationMiddleware` requires the necessary services registered via `AddServiceMeshIntegration` to function correctly.
