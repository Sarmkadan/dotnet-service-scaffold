# Changelog

All notable changes to dotnet-service-scaffold are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.2] - 2026-05-21

### Fixed
- Fix service discovery DNS TTL cache not respecting record changes
- Added regression test for the fix

## [2.0.0] - 2026-03-04

### Added
- Add service discovery with DNS-based and registry-based resolution
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

## [1.0.0] - 2025-07-04

### Added
- Production-ready release with complete feature set
- Comprehensive Swagger/OpenAPI documentation with examples
- Docker and docker-compose support for containerised deployments
- Systemd service file and Caddy reverse-proxy configuration
- Makefile with build, test, publish, and deploy targets
- Complete example scripts in `examples/` directory
- Full documentation in `docs/` covering deployment, architecture, and FAQ
- NuGet packaging configuration for library distribution

### Changed
- Hardened all middleware defaults for production use
- Finalised public API surface; all interfaces are stable
- Tightened BCrypt work factor to 12 for password hashing
- Pinned all package references to tested versions

### Fixed
- Service status not persisted correctly after restart
- Race condition in concurrent health-check dispatches
- Audit log entries missing `IpAddress` when behind a reverse proxy

## [0.9.0] - 2025-06-16

### Added
- `ExternalApiClient` and `WebhookClient` for outbound HTTP integration
- `HttpClientFactory` wrapper with retry and timeout policies
- Domain event infrastructure: `IDomainEvent` and `IDomainEventHandler`
- `DomainEventPublisher` for in-process event dispatch
- `NotificationService` for alert delivery
- Feature flag service (`FeatureFlagService`) with per-flag enable/disable

### Changed
- `ServiceManagementService` now raises domain events on status transitions
- Improved HTTP client lifetime management to avoid socket exhaustion

### Fixed
- Webhook delivery silently swallowing non-2xx responses
- Domain events fired before the triggering transaction committed

## [0.8.0] - 2025-05-27

### Added
- Systemd unit file template in `examples/systemd-deployment.sh`
- Caddy reverse-proxy configuration example in `examples/caddy-example.txt`
- `DeploymentConfiguration` for environment-specific overrides
- Dockerfile with multi-stage build targeting net10.0
- `docker-compose.yml` for local development
- Health check endpoints: `GET /health` and `GET /status`
- `HealthCheckController` exposing history, failures, and on-demand checks

### Changed
- `appsettings.json` restructured into `ApplicationSettings` block
- Default SQLite path changed to `scaffold.db` in working directory

### Fixed
- Health check endpoint returning 500 when no results existed yet
- SQLite file not created on first run in non-existent directory

## [0.7.0] - 2025-05-08

### Added
- `ConfigurationService` and `IConfigurationService` for runtime key-value config
- `ConfigurationRepository` with typed get/set helpers
- `InMemoryCacheService` with sliding and absolute expiry
- `ICacheService` abstraction for future cache-provider swaps
- `ResponseFormatterFactory` supporting JSON and CSV output
- `CsvResponseFormatter` for data-export endpoints

### Changed
- `ServiceCollectionExtensions` now registers all infrastructure services in one call
- Cache TTL defaults moved to `ApplicationSettings`

### Fixed
- Configuration values not refreshed after in-process update
- CSV formatter omitting header row for empty result sets

## [0.6.0] - 2025-04-17

### Added
- `ApiKeyAuthenticationMiddleware` for header-based API key auth
- `RateLimitingMiddleware` with per-IP sliding-window counter
- `RequestLoggingMiddleware` logging method, path, status, and duration
- `ErrorHandlingMiddleware` returning RFC 7807 problem details on unhandled exceptions
- `ApiKeyController` with create, list, revoke, and validate endpoints
- IP whitelist and scope enforcement in `ApiKey` domain model
- Account lockout after configurable failed login attempts

### Changed
- Middleware pipeline order documented and enforced in `Program.cs`
- Error responses standardised to `Result<T>` wrapper

### Fixed
- Rate limiter not resetting window on IP change
- API key revocation not invalidating in-flight requests within same request cycle

## [0.5.0] - 2025-03-31

### Added
- `AuditService` and `IAuditService` for compliance-grade audit trails
- `AuditLogRepository` with pagination and date-range filtering
- `AuditLogController` exposing query and export endpoints
- `MetricsService` and `IMetricsService` for CPU, memory, disk, and response-time records
- `ServiceMetric` domain model and `MetricsController`
- `PerformanceUtility` for stopwatch-based timing helpers
- `CollectionUtility` with batch-processing and chunking helpers

### Changed
- `ServiceEvent` now records actor identity and IP address
- Metrics recorded automatically on each health-check completion

### Fixed
- Audit log timestamp stored in local time instead of UTC
- Metrics endpoint returning duplicate records for the same interval

## [0.4.0] - 2025-03-10

### Added
- `UserService` and `IUserService` with register, login, and password-change flows
- BCrypt password hashing via `BCrypt.Net-Next`
- `UserRepository` with username and email lookup
- `UserController` with register, login, and profile endpoints
- `ApiKey` domain model with scopes, expiry, and IP whitelist
- `EncryptionUtility` for symmetric encrypt/decrypt helpers
- `ValidationUtility` for common input validation patterns
- `Result<T>` shared model for uniform success/failure responses
- `ServiceScaffoldException` base exception with error-code support

### Changed
- `HttpContextExtensions` now extracts client IP honouring `X-Forwarded-For`
- `StringUtility` extended with slug and truncate helpers

### Fixed
- Null-reference in login flow when username did not exist
- Password change not updating `UpdatedAt` timestamp

## [0.3.0] - 2025-02-21

### Added
- `ServiceManagementService` and `IServiceManagementService`
- `HealthCheckService` and `IHealthCheckService` with HTTP-based probing
- `ServiceController` with register, list, get, enable, and disable endpoints
- `ServiceRepository` and `HealthCheckRepository` with EF Core queries
- `HealthCheckResult` and `ServiceEvent` domain models
- `HealthStatus` and `ServiceEventType` enums
- Background health-check scheduling via `IHostedService`
- Success-rate calculation over rolling window

### Changed
- `ServiceScaffoldDbContext` extended with `HealthCheckResults` and `ServiceEvents` sets
- Repository base class extracted to `Repository<T>` generic

### Fixed
- Entity Framework tracking conflict when updating service status concurrently
- Health check not honouring per-service timeout override

## [0.2.0] - 2025-02-03

### Added
- `ServiceScaffoldDbContext` with SQLite provider via Entity Framework Core
- `ServiceRegistration` domain model with status and owner tracking
- `ServiceStatus` enum (`Unknown`, `Healthy`, `Degraded`, `Unhealthy`)
- `IRepository<T>` generic repository abstraction
- `AuditLog` and `ServiceConfiguration` domain models
- `ServiceCollectionExtensions` for DI registration
- Serilog structured logging with console and rolling-file sinks
- `appsettings.json` with `ConnectionStrings` and logging configuration
- `.editorconfig` enforcing C# style rules

### Changed
- Project restructured into `src/Domain`, `src/Application`, `src/Infrastructure`, `src/Presentation`, `src/Shared` layers

### Fixed
- EF Core migration path not resolving correctly from project root

## [0.1.0] - 2025-01-13

### Added
- Initial project scaffold targeting .NET 10.0
- `Program.cs` with minimal ASP.NET Core host setup
- Solution file and main `.csproj` with core package references
- `dotnet-service-scaffold.sln` linking main project and test project
- `xunit`-based test project with `FluentAssertions` and `Moq`
- `StringUtility`, `DateTimeUtility`, and `JsonUtility` helpers
- `Constants.cs` for application-wide string constants
- `ExceptionExtensions` for inner-exception flattening
- `ReflectionUtility` and `HttpUtility` helpers
- MIT `LICENSE` and initial `README.md`
- `.gitignore` for .NET projects

## Version History Summary

| Version | Release Date | Focus |
|---------|--------------|-------|
| 1.0.0 | 2025-07-04 | Production release, NuGet packaging, full docs |
| 0.9.0 | 2025-06-16 | External integrations, webhooks, domain events |
| 0.8.0 | 2025-05-27 | Deployment: systemd, Caddy, Docker |
| 0.7.0 | 2025-05-08 | Configuration management, caching, formatters |
| 0.6.0 | 2025-04-17 | Middleware pipeline, API keys, rate limiting |
| 0.5.0 | 2025-03-31 | Audit logging, metrics collection |
| 0.4.0 | 2025-03-10 | User auth, password hashing, Result pattern |
| 0.3.0 | 2025-02-21 | Service management, health-check probing |
| 0.2.0 | 2025-02-03 | SQLite, EF Core, domain models, Serilog |
| 0.1.0 | 2025-01-13 | Initial scaffold, utilities, test project |

## Contributing

To contribute to this project:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Ensure tests pass
5. Submit a pull request

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

## License

MIT - Copyright 2025 Vladyslav Zaiets

---

**Last Updated**: 2025-07-04

For more details on each version, see [GitHub Releases](https://github.com/sarmkadan/dotnet-service-scaffold/releases).
