# Architecture Guide

This document describes the actual architecture of dotnet-service-scaffold as implemented in the code. It is a single ASP.NET Core web application (one `.csproj`, entry point `Program.cs` in the repository root) organized into layered folders under `src/`, plus a test project and a BenchmarkDotNet project.

## Overview

dotnet-service-scaffold is a service-registry / health-monitoring style API:

- Users register **services** (name + endpoint + health check URL).
- The app can **probe** those endpoints over HTTP and persist `HealthCheckResult` rows.
- **Metrics**, **audit logs**, **API keys**, and per-service **configuration** are stored alongside.
- Persistence is **SQLite via EF Core** (WAL mode enabled at startup).
- Logging is **Serilog** (console + rolling file `logs/scaffold-*.txt`).
- The app self-exposes `/health` (ASP.NET Core health checks), `/status` (DB ping), and `/metrics` (Prometheus text format from the in-process `MetricsService`).

## Project Layout

```
Program.cs                     Composition root (DI, pipeline, endpoints, DB init)
src/
  Domain/                      Entities, enums, domain events, exceptions
  Application/Services/        Business logic (UserService, HealthCheckService, ...)
  Infrastructure/
    Data/                      ServiceScaffoldDbContext + repositories
    Caching/                   ICacheService / InMemoryCacheService
    Metrics/                   MetricsService, PrometheusFormatter
    Logging/                   Serilog enrichment, CorrelationIdMiddleware
    HealthChecks/              SqliteHealthCheck (file/disk probe)
    Integration/               ExternalApiClient, WebhookClient, HttpClientFactory
    Formatting/                JSON/CSV response formatters + factory
    DockerCompose/             docker-compose YAML generator
    Configuration/             systemd/Caddy deployment file generators
    ServiceDiscovery/          DNS + registry discovery providers (opt-in)
    ServiceMesh/               Envoy-compatible sidecar admin client (opt-in)
    Extensions/                ServiceCollectionExtensions (DI helpers)
  Presentation/
    Controllers/               User, Service, HealthCheck, Metrics, AuditLog,
                               ApiKey, DockerCompose controllers
    Middleware/                ApiKeyAuthenticationHandler (+ Options),
                               ErrorHandling, RequestLogging, RateLimiting
  Shared/                      Result/Result<T>, options, utilities, constants
tests/                         xUnit unit + integration tests (in-memory/SQLite)
benchmarks/                    BenchmarkDotNet micro-benchmarks
docs/                          Per-type reference docs + guides
examples/                      Standalone usage examples (not compiled into the app)
```

## What Is Actually Wired at Startup

`Program.cs` is the source of truth. It registers:

- `ServiceScaffoldDbContext` (SQLite, connection string `ConnectionStrings:DefaultConnection`, default `Data Source=scaffold.db`).
- Repositories (scoped): `IUserRepository`, `IServiceRepository`, `IHealthCheckRepository`, `IAuditLogRepository`, `IConfigurationRepository`, `IApiKeyRepository`.
- Application services (scoped): `IUserService`, `IServiceManagementService`, `IAuditService`, `IConfigurationService`. `IHealthCheckService` is registered as a **typed HTTP client** (`AddHttpClient<IHealthCheckService, HealthCheckService>`, 30s timeout) because it performs outbound HTTP probes.
- `AddApplicationServices(configuration)`: `IDomainEventPublisher`, `IDockerComposeGenerator`, structured-logging services.
- Singletons: `IMetricsService`, `IPrometheusFormatter` (in-process metric state must survive across requests, hence singleton).
- Authentication: `AddApiAuthentication()` registers the `ApiKey` scheme (`ApiKeyAuthenticationHandler`, `X-Api-Key` header validated against the database through `IUserService`). Controllers marked `[Authorize]` (e.g. `MetricsController`) rely on this scheme.
- Health checks: `AddDbContextCheck` (EF connectivity) + custom `SqliteHealthCheck` (file accessibility / disk space).

Pipeline order: Swagger (Development only) → optional `CorrelationIdMiddleware` (when `StructuredLogging:EnableCorrelationId` is true) → HTTPS redirect → authentication → authorization → controllers → `/health`, `/status`, `/metrics` endpoints. After building the pipeline, the app calls `ServiceScaffoldDbContext.InitializeDatabaseAsync()` (schema creation + `PRAGMA journal_mode=WAL`) and aborts startup if it fails.

### Opt-in components (present in the codebase, NOT wired by default)

These compile into the assembly but are only activated if you call their extension methods yourself:

- `ErrorHandlingMiddleware`, `RequestLoggingMiddleware`, `RateLimitingMiddleware` — available via `ServiceCollectionExtensions.UseApplicationMiddleware(app)`.
- `ICacheService` / `InMemoryCacheService` — via `AddCachingServices()`.
- `IExternalApiClient`, `IWebhookClient`, `ICustomHttpClientFactory`, `IResponseFormatterFactory` — via `AddIntegrationServices()`.
- Service discovery (`DnsServiceDiscoveryProvider`, `RegistryServiceDiscoveryProvider`, `ServiceDiscoveryService`) and service mesh (`SidecarProxyService`) — via their respective `Add*` extensions in `src/Infrastructure/ServiceDiscovery` and `src/Infrastructure/ServiceMesh`.
- `NotificationService`, `FeatureFlagService` — implemented and unit-tested, but not registered in `Program.cs`.

Docs or examples that show these components in use assume you opt in explicitly.

## Domain Model

EF Core DbSets (see `ServiceScaffoldDbContext`): `Users`, `ServiceRegistrations`, `HealthCheckResults`, `ServiceMetrics`, `ServiceEvents`, `ApiKeys`, `AuditLogs`, `ServiceConfigurations`.

Key relationships: `ServiceRegistration` belongs to a `User` (OwnerId) and has many `HealthCheckResults`, `ServiceMetrics`, `ServiceEvents`, and optional `ServiceConfigurations`; `ApiKey` belongs to a `User`; `AuditLog` references user/entity ids loosely (no hard FK to arbitrary entities).

Enums (`src/Domain/Enums/`):

```csharp
public enum ServiceStatus  { Unknown, Healthy, Degraded, Unhealthy, Disabled, Maintenance }
public enum HealthStatus   { Unknown, Healthy, Degraded, Unhealthy, Timeout, Error }
public enum ServiceEventType { /* registration/health/status lifecycle events */ }
```

Exceptions derive from `ServiceScaffoldException`: `ServiceNotFoundException`, `ServiceValidationException`, `HealthCheckException`, `UnauthorizedException`, `InvalidApiKeyException`, `DataAccessException`, `ConfigurationException`, `ResourceExhaustedException`.

`Result` / `Result<T>` (`src/Shared/Models/`) provide a railway-style success/failure type used by service discovery and utility code instead of exceptions for expected failures.

## Data Flow: Health Check

```
POST /api/healthcheck/... (HealthCheckController)
  → IHealthCheckService.PerformHealthCheckAsync(serviceId)
      → IServiceRepository.GetByIdAsync            (load registration)
      → HttpClient GET service.HealthCheckUrl       (typed client, 30s timeout)
      → new HealthCheckResult { IsHealthy, ResponseTimeMs, StatusCode, ... }
      → IHealthCheckRepository.AddAsync + SaveChangesAsync
  → controller serializes result to JSON
```

Failed probes are recorded as unhealthy results rather than thrown, so history/analytics queries (`GetFailedResultsAsync`, `GetAverageResponseTimeAsync`, `GetFailureCountAsync`) work uniformly.

## Key Design Decisions

- **Single project, layered folders** rather than one assembly per layer. Trade-off: simpler build/deploy for a scaffold; the layering is by convention (nothing stops Presentation referencing Infrastructure directly).
- **SQLite + WAL** as the default store. Zero-dependency local run and good read concurrency; the trade-off is a single writer and no horizontal scaling. `InitializeDatabaseAsync` uses EF migrations when present and falls back to `EnsureCreatedAsync` (the repo currently ships no migrations, so the fallback path is the one that runs).
- **Repository pattern over EF Core.** Generic `Repository<T>` plus per-aggregate repositories with intent-revealing queries. Trade-off: some duplication of what LINQ-on-DbContext already gives you, in exchange for mockable seams (used heavily by the unit tests).
- **API-key authentication as an `AuthenticationHandler`** (scheme-based) rather than custom middleware, so standard `[Authorize]`/`[AllowAnonymous]` attributes and 401/403 semantics apply.
- **In-process metrics** (`MetricsService` singleton + Prometheus text formatter) instead of a metrics library dependency. Cheap and dependency-free; counters reset on restart and are per-instance only.
- **Serilog configured in code** from the `StructuredLogging` options section (not from the `Serilog` config section, which exists in `appsettings.json` but is effectively documentation — the logger is built programmatically in `Program.cs`).

## Extension Points

- **New entity/feature**: model in `src/Domain/Models` → DbSet in `ServiceScaffoldDbContext` → repository interface + implementation → application service → controller → register in `Program.cs`.
- **Response formats**: implement `IResponseFormatter` and register it on `ResponseFormatterFactory` (JSON and CSV ship built-in).
- **Discovery backends**: implement `IServiceDiscoveryProvider` (DNS and HTTP-registry providers included).
- **Caching**: swap `InMemoryCacheService` for a distributed `ICacheService` implementation.
- **Cross-cutting middleware**: wire `UseApplicationMiddleware()` to get error handling, request logging, and rate limiting in one call.

## Known Limitations

- No EF Core migrations are checked in; schema comes from `EnsureCreatedAsync`, so model changes on an existing database require manual handling.
- In-memory cache and metrics are per-process; not suitable as-is for multi-instance deployments.
- Health checks are executed on demand; there is no background scheduler registered (`AddBackgroundServices()` is currently a no-op).
- Rate limiting, request logging, and centralized error handling exist but are opt-in (see above); the default pipeline relies on framework defaults.
- SQLite means a single writer; heavy concurrent write load will serialize.

## Testing

- `tests/dotnet-service-scaffold.Tests` — xUnit; unit tests mock repositories, integration tests exercise repositories against real (temporary) SQLite databases via `IntegrationTestBase`.
- `benchmarks/dotnet-service-scaffold.Benchmarks` — BenchmarkDotNet suites for database CRUD, cache, metrics, and string utilities.

Run with `dotnet test` / `dotnet run -c Release --project benchmarks/dotnet-service-scaffold.Benchmarks`.
