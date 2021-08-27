# ServiceCollectionExtensions

The `ServiceCollectionExtensions` static class provides extension methods designed to streamline the configuration and registration of services and middleware within .NET applications. It facilitates the modular organization of application services, infrastructure components, caching, background processing, authentication, and request pipeline management.

## API

- `AddApplicationServices()`
  Registers primary business logic and application-specific services into the `IServiceCollection`.
  Returns: The `IServiceCollection` instance.

- `AddApplicationServices()`
  An overload for registering application-specific services.
  Returns: The `IServiceCollection` instance.

- `AddIntegrationServices()`
  Registers infrastructure components, external service clients, and integration handlers.
  Returns: The `IServiceCollection` instance.

- `AddCachingServices()`
  Configures and registers the application's caching infrastructure.
  Returns: The `IServiceCollection` instance.

- `AddBackgroundServices()`
  Registers hosted services and background worker tasks.
  Returns: The `IServiceCollection` instance.

- `AddApiAuthentication()`
  Configures authentication schemes and security policies for the API.
  Returns: The `IServiceCollection` instance.

- `UseApplicationMiddleware()`
  Configures the HTTP request processing pipeline components for the application.
  Returns: The `WebApplication` instance.

## Usage

```csharp
// Example: Registering services in Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddIntegrationServices();
builder.Services.AddCachingServices();
builder.Services.AddBackgroundServices();
builder.Services.AddApiAuthentication();

var app = builder.Build();
```

```csharp
// Example: Configuring the middleware pipeline
var app = builder.Build();

app.UseApplicationMiddleware();

app.Run();
```

## Notes

- **Thread Safety:** These extension methods are intended for use during the application's startup phase. They are not thread-safe and must not be invoked after the `ServiceProvider` has been built.
- **Dependency Order:** Certain services registered via these methods may depend on others; ensure that the order of registration matches the dependency requirements of the application.
- **Pipeline Configuration:** The `UseApplicationMiddleware` method modifies the request pipeline. Ensure it is called at the appropriate point in the `Program.cs` file, typically after the `WebApplication` has been built and before `app.Run()`.
