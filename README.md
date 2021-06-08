# .NET Service Scaffold

A production-grade template for self-hosted .NET services with built-in monitoring, health checks, SQLite database, and comprehensive service management.

## Features

- **Service Registration & Lifecycle Management**: Register services, track status, enable/disable monitoring
- **Health Checks**: HTTP-based health monitoring with configurable intervals and timeouts
- **Performance Metrics**: Track CPU, memory, disk usage, and response times
- **User Management**: Full authentication, password management, and account lockout protection
- **Audit Logging**: Comprehensive audit trail for compliance and security
- **Configuration Management**: Centralized configuration storage with type validation
- **API Keys**: Secure API authentication with IP whitelisting and scope management
- **SQLite Database**: Persistent storage with Entity Framework Core ORM
- **Serilog Integration**: Structured logging to console and files
- **ASP.NET Core Web API**: RESTful endpoints with Swagger documentation
- **Clean Architecture**: Layered design with proper separation of concerns

## Technology Stack

- **.NET 10.0** - Latest .NET framework
- **Entity Framework Core 10.0** - ORM for data access
- **SQLite** - Lightweight, self-contained database
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **BCrypt.Net** - Secure password hashing

## Project Structure

```
dotnet-service-scaffold/
├── src/
│   ├── Application/
│   │   └── Services/        # Business logic services
│   ├── Domain/
│   │   ├── Models/          # Entity classes
│   │   ├── Enums/           # Domain enumerations
│   │   └── Exceptions/      # Custom exceptions
│   ├── Infrastructure/
│   │   └── Data/            # DbContext, repositories
│   ├── Presentation/
│   │   └── Controllers/     # API controllers
│   └── Shared/              # Constants and utilities
├── Program.cs               # Application entry point
├── appsettings.json         # Configuration
└── dotnet-service-scaffold.csproj
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- SQLite (included with most systems)

### Building

```bash
dotnet build
```

### Running

```bash
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger UI at `/swagger`.

### Database Setup

The database is automatically initialized on startup. SQLite creates a `scaffold.db` file in the working directory.

## API Endpoints

### Health Checks
- `POST /api/healthcheck/{serviceId}/check` - Perform immediate health check
- `GET /api/healthcheck/{serviceId}/status` - Get service status
- `GET /api/healthcheck/{serviceId}/history` - Get health check history
- `GET /api/healthcheck/{serviceId}/failures` - Get failed checks

### Users
- `POST /api/user/register` - Create new user
- `POST /api/user/login` - Authenticate user
- `GET /api/user/{userId}` - Get user information
- `POST /api/user/{userId}/change-password` - Change password
- `POST /api/user/{userId}/unlock` - Unlock locked account

### Services
- `POST /api/service/register` - Register service
- `GET /api/service` - List all services
- `GET /api/service/{serviceId}` - Get service details
- `GET /api/service/owner/{ownerId}` - Get services by owner
- `POST /api/service/{serviceId}/disable` - Disable service
- `POST /api/service/{serviceId}/enable` - Enable service
- `GET /api/service/health/unhealthy` - Get unhealthy services

### System
- `GET /health` - Health check endpoint
- `GET /status` - Service status

## Configuration

Edit `appsettings.json` to customize:

```json
{
  "ApplicationSettings": {
    "HealthCheckInterval": 60,
    "HealthCheckTimeout": 10,
    "MaxConcurrentHealthChecks": 5,
    "AuditLogRetentionDays": 90,
    "HealthCheckResultRetentionDays": 30
  }
}
```

## Services Overview

### UserService
Handles user management, authentication, password changes, and account lockout.

### HealthCheckService
Monitors service health via HTTP probes, tracks metrics, and manages health check history.

### ServiceManagementService
Manages service registration, lifecycle, status tracking, and success rates.

### AuditService
Logs all system actions for compliance and security auditing.

### ConfigurationService
Manages application settings with type validation and encryption support.

## Domain Models

- **User**: System users with authentication and profile information
- **ServiceRegistration**: Registered services with health check configuration
- **HealthCheckResult**: Results of individual health check probes
- **ServiceMetric**: Performance metrics (CPU, memory, response times)
- **ServiceEvent**: Significant service events and status changes
- **ApiKey**: Secure API authentication tokens
- **AuditLog**: Compliance and activity audit trail
- **ServiceConfiguration**: Centralized configuration storage

## Error Handling

The application uses custom exception types for better error handling:

- `ServiceScaffoldException` - Base exception
- `ServiceNotFoundException` - Service not found
- `ServiceValidationException` - Validation failures
- `UnauthorizedException` - Access denied
- `InvalidApiKeyException` - Invalid API credentials
- `DataAccessException` - Database errors

## Security Features

- **Password Security**: BCrypt hashing with salt
- **Account Lockout**: Automatic lockout after failed login attempts
- **API Key Management**: IP whitelisting and scope-based access control
- **Audit Trail**: Complete logging of all actions
- **Data Validation**: Input validation at all layers

## Deployment

### Systemd Service

Create `/etc/systemd/system/dotnet-scaffold.service`:

```ini
[Unit]
Description=DotNet Service Scaffold
After=network.target

[Service]
Type=notify
User=scaffold
WorkingDirectory=/opt/scaffold
ExecStart=/usr/bin/dotnet /opt/scaffold/dotnet-service-scaffold.dll
Restart=on-failure
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl daemon-reload
sudo systemctl enable dotnet-scaffold
sudo systemctl start dotnet-scaffold
```

### Caddy Reverse Proxy

```caddy
scaffold.example.com {
    reverse_proxy localhost:5000
    encode gzip
}
```

## Development

### Adding New Services

1. Create interface in `src/Application/Services/I{Service}Service.cs`
2. Implement in `src/Application/Services/{Service}Service.cs`
3. Register in `Program.cs` dependency injection
4. Create tests in test project

### Database Migrations

```bash
dotnet ef migrations add {MigrationName} -o src/Infrastructure/Migrations
dotnet ef database update
```

## License

MIT - Copyright 2026 Vladyslav Zaiets

## Contact

- Website: https://sarmkadan.com
- GitHub: https://github.com/sarmkadan

---

**This is a production-grade scaffold.** Customize and extend according to your specific needs.
