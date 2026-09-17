# CLAUDE.md

## Project overview

ASP.NET Core (.NET 10) microservice scaffold: REST API with API-key auth, health checks, Prometheus metrics, service discovery, audit logging, EF Core + SQLite, Serilog.

## Build

```bash
dotnet build                                   # main project, Debug
dotnet build dotnet-service-scaffold.sln       # main + tests + benchmarks
dotnet build -c Release                        # or: make build-release
dotnet run                                     # http://localhost:5000, Swagger at /swagger
dotnet publish -c Release -o ./publish         # or: make publish
```

SDK pinned in `global.json` (10.0.100, rollForward latestMinor). `make help` lists Docker/systemd/db targets. The main `.csproj` excludes `tests/**`, `examples/**`, `benchmarks/**` from compilation.

## Test

```bash
dotnet test                                                        # whole solution
dotnet test tests/dotnet-service-scaffold.Tests                    # test project only
dotnet test --filter "FullyQualifiedName~HealthCheckServiceTests"  # single class
```

- Framework: xUnit 2.9 + FluentAssertions 7 + NSubstitute (preferred) / Moq; EF Core InMemory for repository tests.
- Test project: `tests/dotnet-service-scaffold.Tests/`, mirrors `src/` layout (`Application/Services`, `Infrastructure/...`, `Presentation/Middleware`, `Shared`).
- Naming: `<Class>Tests.cs`, methods `Method_ShouldX_WhenY`, Arrange/Act/Assert comments.
- Several test folders are excluded via `<Compile Remove>` in the test `.csproj` (`Application/Services/**`, `IntegrationTests/**`, `Infrastructure/Data/Repository/**`, and a few files) - they do not compile. Check the csproj before assuming a test runs.
- `tests/DotnetServiceScaffold.Tests/` and `tests/Infrastructure/` are stray directories with no csproj; ignore.

## Lint / Format

```bash
dotnet format                                  # or: make format
dotnet build /p:EnforceCodeStyleInBuild=true   # or: make analyze
```

Style is in `.editorconfig`: 4 spaces, LF, Allman braces, `insert_final_newline`. `TreatWarningsAsErrors=false`; CS1591 suppressed. `GenerateDocumentationFile=true` - public members carry XML docs.

## Architecture

Single web project, layered by folder (Clean Architecture-style, all in one assembly):

- `Program.cs` - entry point, top-level statements; all DI registration and middleware pipeline live here. Constants in `ProgramConstants`.
- `src/Domain/` - `Models/` (entities + `I<Entity>` interfaces, `<Entity>Builder`, `<Entity>Constants`), `Enums/`, `Events/` (`IDomainEvent`), `Exceptions/` (`ServiceScaffoldException`).
- `src/Application/Services/` - business services (`IUserService`/`UserService`, `HealthCheckService`, `AuditService`, `ConfigurationService`, `FeatureFlagService`, `NotificationService`, `DomainEventPublisher`).
- `src/Infrastructure/` - `Data/` (`ServiceScaffoldDbContext`, `Repository/` generic `IRepository<T>`/`Repository<T>` + per-entity repos), `HealthChecks/`, `Metrics/` (`MetricsService`, `PrometheusFormatter`), `ServiceDiscovery/`, `ServiceMesh/`, `Logging/`, `Caching/`, `Http/` (`ExternalApiClient`, `CircuitBreaker`), `Middleware/`, `Formatting/` (`ProblemDetailsFactory`), `Extensions/ServiceCollectionExtensions.cs`.
- `src/Presentation/` - `Controllers/` (attribute-routed MVC controllers, each with `I<Name>Controller`), `Middleware/` (`ErrorHandlingMiddleware`, `ApiKeyAuthenticationMiddleware`, `RateLimitingMiddleware`, `RequestLoggingMiddleware`).
- `src/Shared/` - `Models/Result.cs` (`Result` / `Result<T>`), `Configuration/` (options classes), `Utilities/`, `Extensions/`.
- `docs/` - one markdown file per class; `examples/` - sample usage (not compiled); `benchmarks/` - BenchmarkDotNet project.
- Config: `appsettings.json` (`appsettings.example.json` as template), section names in `ProgramConstants`. DB: SQLite `scaffold.db`, `busy_timeout` appended to connection string.

## Conventions

- Namespaces: `DotnetServiceScaffold.<Layer>.<Folder>`, file-scoped. Every file starts with `#nullable enable` and the author banner comment.
- One type per file. Companion files per type are the norm: `<Type>Constants.cs` (magic strings/numbers), `<Type>Builder.cs`, `<Type>Extensions.cs`, `<Type>JsonExtensions.cs`, `<Type>Validation.cs`. Put new literals into the matching `*Constants` class, not inline.
- Interfaces for nearly everything, including controllers and test classes (`IHealthCheckServiceTests`).
- DI: constructor injection, interface-to-implementation registration in `Program.cs`. Repositories and services are `Scoped`; `MetricsService`, `PrometheusFormatter` are `Singleton`. `HealthCheckService` is registered only via `AddHttpClient<IHealthCheckService, HealthCheckService>()` - do not add a second registration. Options via `AddOptions<T>().Bind(...).ValidateOnStart()` with `IValidateOptions<T>` validators.
- Error handling: services return `Result`/`Result<T>` (`Result.Success()`, `Result.Failure(message, code)`) for expected failures; throw `ServiceScaffoldException` or BCL exceptions for programmer errors. `ErrorHandlingMiddleware` maps exceptions to RFC 7807 `application/problem+json` (`ServiceScaffoldException`/`ArgumentException` -> 400, `InvalidOperationException` -> 409, `KeyNotFoundException` -> 404) and logs with an `ErrorId`. Guard args with `ArgumentException.ThrowIfNullOrEmpty` / `ArgumentNullException.ThrowIfNull`.
- Logging: Serilog, structured message templates (`{ErrorId}`), configured from `StructuredLoggingOptions`.
- Auth: `[Authorize]` controllers rely on the API-key scheme from `AddApiAuthentication()`.
- Do not add `Microsoft.AspNetCore.OpenApi` / `Microsoft.OpenApi` 2.x - conflicts with Swashbuckle (see csproj comment).
- Repo hygiene: `*.backup` files, `.aider*`, `update_*.py`, `aider_buildcmd.py` and stray root files (`}`, `You can proceed with`) are leftovers from automated edits, not part of the build.
