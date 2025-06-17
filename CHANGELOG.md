# Changelog

All notable changes to dotnet-service-scaffold are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- Feature flag service for conditional feature enablement
- Webhook integration for service status notifications
- Batch health check optimization for improved throughput
- Redis caching support for distributed environments
- PostgreSQL database support (alternative to SQLite)
- Kubernetes deployment manifests and documentation
- Health check failure analysis dashboard
- Service metrics export to Prometheus format
- Advanced audit log filtering by action type
- API key expiration and rotation support
- Rate limiting per API key scope
- Request/response logging in detailed mode

### Changed
- Improved health check performance with concurrent batching
- Optimized database queries with better indexing strategy
- Enhanced security headers in all HTTP responses
- Updated configuration schema with validation
- Improved error messages for better debugging
- Refactored middleware pipeline for clarity
- Database retention policies now configurable per entity type

### Fixed
- Database connection pooling exhaustion under load
- Health check timeout handling for slow services
- Account lockout duration not properly enforced
- Audit log pagination offset calculation
- Memory leak in cache service with long TTL values
- Race condition in service status updates

### Security
- Implemented password complexity requirements
- Added CORS configuration for cross-origin requests
- Enhanced API key validation with IP whitelist enforcement
- Implemented request signing for webhook delivery
- Added rate limiting per IP and per API key

## [1.1.0] - 2026-03-15

### Added
- Health check response time tracking and analysis
- Service success rate calculations with historical trending
- Advanced search in audit logs with multiple filters
- Bulk service registration capability
- Service group management for organized monitoring
- Custom health check timeout configuration per service
- API documentation improvements with examples
- Docker Compose setup for local development
- Systemd service file for Linux deployment
- Comprehensive example scripts and use cases

### Changed
- Improved user interface responsiveness
- Enhanced service status calculation algorithm
- Optimized database queries for large result sets
- Updated API error response format for consistency
- Refactored service controller for better maintainability
- Improved logging verbosity in debug mode

### Fixed
- Health check results timestamp accuracy
- Service status not updating after manual health check
- API key validation bypassing IP whitelist in certain cases
- Configuration changes not taking effect without restart
- Database query timeouts with large result sets

### Deprecated
- Legacy API endpoint `/api/v0/*` (use `/api/*` instead)
- ConfigurationService direct database access (use ConfigurationRepository)

## [1.0.0] - 2026-01-10

### Added
- Initial release of dotnet-service-scaffold
- Service registration and lifecycle management
- HTTP-based health check monitoring
- User authentication with JWT tokens
- API key authentication with scope management
- Comprehensive audit logging
- Service metrics collection (CPU, memory, disk, response time)
- Performance metrics tracking and analysis
- User account management with password hashing
- Account lockout protection after failed login attempts
- Swagger/OpenAPI documentation
- SQLite database with Entity Framework Core
- Serilog structured logging to console and files
- In-memory caching service
- Configuration management service
- Repository pattern for data access
- Clean architecture implementation
- Comprehensive error handling
- Request logging middleware
- Rate limiting middleware
- .NET 10.0 targeting

## Version History Summary

| Version | Release Date | Focus |
|---------|--------------|-------|
| 1.2.0 | 2026-05-04 | Advanced features, K8s support, caching |
| 1.1.0 | 2026-03-15 | Usability improvements, Docker support |
| 1.0.0 | 2026-01-10 | Initial release, core features |

## Upgrade Guide

### From 1.0.0 to 1.1.0

1. **Backup your database**
   ```bash
   cp scaffold.db scaffold.db.backup
   ```

2. **Update application**
   ```bash
   git pull origin main
   dotnet build -c Release
   ```

3. **Run migrations** (if applicable)
   ```bash
   dotnet ef database update
   ```

4. **Restart service**
   ```bash
   sudo systemctl restart dotnet-scaffold
   ```

### From 1.1.0 to 1.2.0

1. **Backup your database**
   ```bash
   cp scaffold.db scaffold.db.backup
   ```

2. **Review breaking changes**
   - Check if using any deprecated endpoints
   - Update any direct ConfigurationService usage

3. **Update configuration**
   - Add new feature flag settings if desired
   - Configure Redis if using caching feature
   - Configure PostgreSQL if migrating databases

4. **Update and deploy**
   ```bash
   git pull origin main
   dotnet build -c Release
   dotnet publish -c Release -o ./publish
   sudo systemctl restart dotnet-scaffold
   ```

## Roadmap

### Planned for 1.3.0
- Event sourcing for audit trail
- GraphQL API support
- Advanced analytics dashboard
- Multi-tenant support
- Performance profiling tools
- Automated backup scheduling

### Planned for 2.0.0
- Horizontal scaling with load balancing
- Message queue integration (RabbitMQ/Kafka)
- Advanced authentication (OAuth 2.0, SAML)
- Real-time notifications (WebSocket)
- Custom health check scripts
- Dashboard UI

## Contributing

To contribute to this project:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Ensure tests pass
5. Submit a pull request

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

## Migration Notes

### SQLite to PostgreSQL

Migration from SQLite to PostgreSQL is supported in version 1.1.0+:

```bash
# 1. Back up SQLite database
cp scaffold.db scaffold.db.backup

# 2. Update connection string in appsettings.json
# 3. Update DbContext to use PostgreSQL provider
# 4. Run migrations against new database
dotnet ef database update
```

### Configuration Schema Changes

Configuration keys have been standardized. If you have custom settings:

- Old: `HealthCheck.Interval` → New: `ApplicationSettings.HealthCheckInterval`
- Old: `Cache.Enabled` → New: `ApplicationSettings.EnableCaching`

Run migration script:
```bash
dotnet run -- --migrate-config
```

## Support

- **Documentation**: See `/docs` directory
- **Issues**: https://github.com/sarmkadan/dotnet-service-scaffold/issues
- **Discussions**: https://github.com/sarmkadan/dotnet-service-scaffold/discussions
- **Email**: rutova2@gmail.com

## License

MIT - Copyright 2026 Vladyslav Zaiets

---

**Last Updated**: 2026-05-04

For more details on each version, see [GitHub Releases](https://github.com/sarmkadan/dotnet-service-scaffold/releases).
