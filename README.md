![Build](https://github.com/sarmkadan/dotnet-service-scaffold/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/dotnet-service-scaffold?style=flat-square&label=License&color=blue)

# .NET Service Scaffold

A production-grade template for self-hosted .NET services with built-in monitoring, health checks, SQLite database, and comprehensive service management. Deploy secure, observable, and maintainable services with minimal configuration.

## Overview

**dotnet-service-scaffold** is a complete framework for building self-hosted .NET services that can be deployed on Linux servers using systemd and Caddy. It includes everything needed for production: health monitoring, security, auditing, metrics, and comprehensive API management.

Perfect for:
- Building internal service APIs
- Creating microservices in self-hosted environments
- Monitoring infrastructure health
- Managing service lifecycles at scale
- Implementing audit trails for compliance

## Key Features

### Service Management
- **Service Registration & Lifecycle**: Register services, track status, enable/disable monitoring
- **Health Checks**: HTTP-based health monitoring with configurable intervals and timeouts
- **Status Tracking**: Real-time service state monitoring and historical tracking
- **Success Rate Analytics**: Track service reliability over time
- **Event Logging**: Capture significant service events and state transitions

### Monitoring & Observability
- **Performance Metrics**: Track CPU, memory, disk usage, and response times
- **Health Check History**: Detailed records of all health check attempts
- **Failure Analysis**: Identify patterns and root causes of service issues
- **Structured Logging**: Serilog integration with console and file output
- **Health Endpoints**: Standard `/health` and `/status` endpoints

#### Docker Compose Generator
Generate Docker Compose YAML programmatically through the API or infrastructure service. The generator supports application service definitions, named volumes, bridge networking, resource limits, and optional Caddy or Redis sidecars for deployment scaffolding.

#### Prometheus Metrics Endpoint
Expose in-memory application metrics through a Prometheus-compatible `/metrics` endpoint. Metrics are formatted using the Prometheus text exposition format, with support for counters, gauges, timer summaries, and tagged labels.

#### Structured Logging with Correlation IDs
Configure Serilog enrichment through `StructuredLogging` settings in `appsettings.json`. Incoming requests can automatically receive correlation IDs, request context properties, machine name, and environment metadata for traceable structured logs.

### Security & Access Control
- **User Management**: Full authentication, password management, and account lockout protection
- **API Keys**: Secure API authentication with IP whitelisting and scope management
- **Password Security**: BCrypt hashing with salt for maximum security
- **Account Lockout**: Automatic lockout after failed login attempts
- **Audit Logging**: Comprehensive audit trail for compliance and security

### Data & Configuration
- **SQLite Database**: Persistent storage with Entity Framework Core ORM
- **Configuration Management**: Centralized configuration storage with type validation
- **Data Persistence**: Complete entity relationships and data consistency
- **Database Migrations**: Entity Framework Core migrations support
- **Backup Ready**: SQLite enables simple file-based backups

### API & Integration
- **ASP.NET Core Web API**: RESTful endpoints with proper HTTP semantics
- **Swagger/OpenAPI**: Auto-generated API documentation
- **Error Handling**: Comprehensive exception handling with custom exception types
- **Content Negotiation**: Support for JSON and CSV response formats
- **Webhook Integration**: HTTP-based external service integration

### Architecture
- **Clean Architecture**: Layered design with proper separation of concerns
- **Domain-Driven Design**: Rich domain models and business logic
- **Dependency Injection**: Built-in IoC container configuration
- **Repository Pattern**: Abstracted data access layer
- **Middleware Pipeline**: Request/response processing and cross-cutting concerns

## Technology Stack

| Component | Version | Purpose |
|-----------|---------|---------|
| .NET | 10.0 | Runtime and framework |
| Entity Framework Core | 10.0 | ORM and data access |
| SQLite | 3.x | Lightweight database |
| Serilog | 8.x | Structured logging |
| Swagger/OpenAPI | 2.0 | API documentation |
| BCrypt.Net | 4.x | Secure password hashing |
| Swashbuckle | 6.x | Swagger integration |

## Project Architecture

### Directory Structure

```
dotnet-service-scaffold/
├── src/
│   ├── Application/
│   │   └── Services/
│   │       ├── IUserService.cs
│   │       ├── UserService.cs
│   │       ├── IHealthCheckService.cs
│   │       ├── HealthCheckService.cs
│   │       ├── IServiceManagementService.cs
│   │       ├── ServiceManagementService.cs
│   │       ├── IAuditService.cs
│   │       ├── AuditService.cs
│   │       ├── IConfigurationService.cs
│   │       ├── ConfigurationService.cs
│   │       └── FeatureFlagService.cs
│   ├── Domain/
│   │   ├── Models/
│   │   │   ├── User.cs
│   │   │   ├── ServiceRegistration.cs
│   │   │   ├── HealthCheckResult.cs
│   │   │   ├── ServiceMetric.cs
│   │   │   ├── AuditLog.cs
│   │   │   ├── ApiKey.cs
│   │   │   └── ServiceConfiguration.cs
│   │   ├── Enums/
│   │   │   ├── HealthStatus.cs
│   │   │   ├── ServiceStatus.cs
│   │   │   └── ServiceEventType.cs
│   │   ├── Events/
│   │   │   ├── IDomainEvent.cs
│   │   │   └── IDomainEventHandler.cs
│   │   └── Exceptions/
│   │       └── ServiceScaffoldException.cs
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── ServiceScaffoldDbContext.cs
│   │   │   └── Repository/
│   │   │       ├── IRepository.cs
│   │   │       ├── Repository.cs
│   │   │       ├── IUserRepository.cs
│   │   │       ├── UserRepository.cs
│   │   │       ├── IServiceRepository.cs
│   │   │       ├── ServiceRepository.cs
│   │   │       ├── IHealthCheckRepository.cs
│   │   │       ├── HealthCheckRepository.cs
│   │   │       └── IAuditLogRepository.cs
│   │   ├── Caching/
│   │   │   ├── ICacheService.cs
│   │   │   └── InMemoryCacheService.cs
│   │   ├── Integration/
│   │   │   ├── ExternalApiClient.cs
│   │   │   ├── HttpClientFactory.cs
│   │   │   └── WebhookClient.cs
│   │   └── Configuration/
│   │       └── DeploymentConfiguration.cs
│   ├── Presentation/
│   │   ├── Controllers/
│   │   │   ├── UserController.cs
│   │   │   ├── ServiceController.cs
│   │   │   ├── HealthCheckController.cs
│   │   │   ├── AuditLogController.cs
│   │   │   ├── MetricsController.cs
│   │   │   └── ApiKeyController.cs
│   │   ├── Middleware/
│   │   │   ├── ApiKeyAuthenticationMiddleware.cs
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   ├── RateLimitingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   └── Extensions/
│   │       └── HttpContextExtensions.cs
│   └── Shared/
│       ├── Constants.cs
│       ├── Models/
│       │   └── Result.cs
│       ├── Utilities/
│       │   ├── StringUtility.cs
│       │   ├── EncryptionUtility.cs
│       │   ├── DateTimeUtility.cs
│       │   ├── ValidationUtility.cs
│       │   ├── JsonUtility.cs
│       │   └── PerformanceUtility.cs
│       └── Extensions/
│           └── ExceptionExtensions.cs
├── examples/
│   ├── basic-service-setup.cs
│   ├── health-check-monitor.cs
│   ├── api-usage.cs
│   ├── systemd-deployment.sh
│   ├── caddy-config.txt
│   └── docker-example.cs
├── docs/
│   ├── getting-started.md
│   ├── architecture.md
│   ├── api-reference.md
│   ├── deployment.md
│   └── faq.md
├── Program.cs
├── appsettings.json
├── dotnet-service-scaffold.csproj
├── Dockerfile
├── docker-compose.yml
├── Makefile
├── CHANGELOG.md
├── LICENSE
└── .editorconfig
```

### Layered Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│           PRESENTATION LAYER                            │
│   ┌─────────────────────────────────────────────────┐   │
│   │         ASP.NET Core Controllers                │   │
│   │  Users  Services  HealthChecks  Metrics  Audit  │   │
│   └──────────────────────┬──────────────────────────┘   │
└────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────┐
│  MIDDLEWARE & CROSS-CUTTING CONCERNS                   │
│  Authentication  Logging  Error Handling  Rate Limits  │
└────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────┐
│           APPLICATION LAYER                             │
│   ┌─────────────────────────────────────────────────┐   │
│   │         Business Logic Services                 │   │
│   │  UserService  HealthCheckService                │   │
│   │  ServiceMgmt  AuditService  ConfigService       │   │
│   └──────────────────────┬──────────────────────────┘   │
└────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────┐
│           DOMAIN LAYER                                  │
│   ┌─────────────────────────────────────────────────┐   │
│   │      Core Business Models & Rules               │   │
│   │  User  Service  HealthCheck  Metrics  Events   │   │
│   └──────────────────────┬──────────────────────────┘   │
└────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────┐
│      INFRASTRUCTURE LAYER                              │
│   ┌─────────────────────────────────────────────────┐   │
│   │   Repository Pattern & Data Access              │   │
│   │  DbContext  Repositories  Caching  Integration  │   │
│   └──────────────────────┬──────────────────────────┘   │
└────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────┐
│           DATA LAYER                                    │
│              SQLite Database                           │
└─────────────────────────────────────────────────────────┘
```

## Quick Start

### Prerequisites

- **.NET 10.0 SDK** or later - [Install from dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **SQLite** (included with most systems)
- **Git** for version control
- **systemd** (for Linux deployments)
- **Caddy** (optional, for reverse proxy)

#### 1. Clone the Repository

```bash
git clone https://github.com/sarmkadan/dotnet-service-scaffold.git
cd dotnet-service-scaffold
```

#### 2. Restore Dependencies

```bash
dotnet restore
```

#### 3. Build the Project

```bash
dotnet build -c Release
```

#### 4. Run the Application

```bash
dotnet run
```

The API will be available at:
- **REST API**: `http://localhost:8080`
- **Swagger UI**: `http://localhost:8080/swagger`
- **Health Check**: `http://localhost:8080/health`
- **Status**: `http://localhost:8080/status`

#### 5. Verify Installation

```bash
curl http://localhost:8080/health
curl http://localhost:8080/status
```

## Using Docker

You can run this service using Docker for a containerized deployment.

### Build and Run with Docker

1. **Build the image**:
   ```bash
   docker build -t dotnet-service-scaffold .
   ```

2. **Run the container**:
   ```bash
   docker run -d -p 8080:8080 --name dotnet-service-scaffold dotnet-service-scaffold
   ```

The application will be accessible at `http://localhost:8080`.

### Using Docker Compose

For a complete setup, including database and other services (if configured), use Docker Compose:

1. **Start all services**:
   ```bash
   docker-compose up -d
   ```

2. **View logs**:
   ```bash
   docker-compose logs -f scaffold
   ```

3. **Stop services**:
   ```bash
   docker-compose down
   ```


## Configuration

### Application Settings

Edit `appsettings.json` to customize behavior. All settings are optional and have sensible defaults.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=scaffold.db"
  },
  "ApplicationSettings": {
    "HealthCheckInterval": 60,
    "HealthCheckTimeout": 10,
    "MaxConcurrentHealthChecks": 5,
    "MaintenanceMode": false,
    "AuditLogRetentionDays": 90,
    "HealthCheckResultRetentionDays": 30,
    "MaxFailedLoginAttempts": 5,
    "AccountLockoutDurationMinutes": 30,
    "PasswordMinimumLength": 8,
    "EnableCors": false,
    "AllowedOrigins": ["http://localhost:3000"],
    "RateLimitPerMinute": 60,
    "MaxServiceRegistrations": 100,
    "MaxResponseSize": 1048576,
    "EnableDetailedErrors": true,
    "DefaultPageSize": 50,
    "MaxPageSize": 200,
    "CacheDurationSeconds": 300,
    "EnableRequestLogging": true,
    "MaxCollectionSize": 1000,
    "ApiKeyPrefix": "sk_live_",
    "ApiKeyLength": 32,
    "JwtTokenExpirationMinutes": 60,
    "JwtSecret": "your-very-secure-jwt-secret-key-at-least-32-characters-long",
    "DatabaseMigrationStrategy": "Auto",
    "EnableDatabaseBackup": false,
    "BackupDirectory": "/app/backups"
  }
}
```

#### Configuration Options Explained

| Setting | Default | Description |
|---------|---------|-------------|
| HealthCheckInterval | 60 | Seconds between health checks |
| HealthCheckTimeout | 10 | Timeout in seconds for health check requests |
| MaxConcurrentHealthChecks | 5 | Maximum parallel health checks |
| MaintenanceMode | false | When true, health checks return maintenance status |
| AuditLogRetentionDays | 90 | Days to keep audit logs before automatic cleanup |
| HealthCheckResultRetentionDays | 30 | Days to keep health check history before automatic cleanup |
| MaxFailedLoginAttempts | 5 | Failed login attempts before account lockout |
| AccountLockoutDurationMinutes | 30 | Minutes to lock account after too many failed attempts |
| PasswordMinimumLength | 8 | Minimum password length requirement |
| EnableCors | false | Enable CORS for cross-origin requests |
| AllowedOrigins | ["http://localhost:3000"] | List of allowed origins for CORS |
| RateLimitPerMinute | 60 | API requests per minute per IP address |
| MaxServiceRegistrations | 100 | Maximum number of service registrations allowed |
| MaxResponseSize | 1048576 (1MB) | Maximum response size in bytes |
| EnableDetailedErrors | true | Show detailed error pages in development |
| DefaultPageSize | 50 | Default page size for paginated API responses |
| MaxPageSize | 200 | Maximum page size for paginated API responses |
| CacheDurationSeconds | 300 (5 min) | Cache duration for frequently accessed data |
| EnableRequestLogging | true | Enable request logging for all endpoints |
| MaxCollectionSize | 1000 | Maximum items to return in collection responses |
| ApiKeyPrefix | "sk_live_" | Prefix for generated API keys |
| ApiKeyLength | 32 | Length of generated API keys |
| JwtTokenExpirationMinutes | 60 | JWT token expiration time in minutes |
| JwtSecret | (required) | Secret key for JWT token signing - must be at least 32 characters |
| DatabaseMigrationStrategy | "Auto" | Migration strategy: Auto, Manual, or None |
| EnableDatabaseBackup | false | Enable automatic database backup on startup |
| BackupDirectory | "/app/backups" | Directory for database backups |

**Security Note:** The `JwtSecret` should be a long, random string (minimum 32 characters) and stored securely in production using environment variables or secret management tools. Never commit it to version control.

**CORS Note:** Only enable CORS in production if you need cross-origin access. For production deployments, restrict `AllowedOrigins` to specific domains only.

## API Reference

### Authentication

All endpoints (except `/health` and `/status`) require authentication via:
1. **API Key** (recommended for services)
2. **User credentials** with JWT token (for users)

#### Get API Key

```bash
curl -X POST http://localhost:5000/api/apikey/create \
  -H "Content-Type: application/json" \
  -d '{"name": "MyService", "ipWhitelist": ["127.0.0.1"]}'
```

Response:
```json
{
  "apiKey": "sk_live_abc123xyz789",
  "createdAt": "2026-05-04T10:00:00Z"
}
```

### Service Management Endpoints

#### Register a Service

```bash
curl -X POST http://localhost:5000/api/service/register \
  -H "Content-Type: application/json" \
  -H "X-API-Key: sk_live_abc123xyz789" \
  -d '{
    "name": "UserService",
    "description": "User authentication service",
    "healthCheckUrl": "https://users.internal/health",
    "ownerId": "user-123",
    "isEnabled": true
  }'
```

#### List All Services

```bash
curl http://localhost:5000/api/service \
  -H "X-API-Key: sk_live_abc123xyz789"
```

Response:
```json
{
  "success": true,
  "data": [
    {
      "id": "svc-uuid",
      "name": "UserService",
      "description": "User authentication service",
      "status": "Healthy",
      "healthCheckUrl": "https://users.internal/health",
      "successRate": 99.8,
      "lastChecked": "2026-05-04T10:00:30Z",
      "isEnabled": true
    }
  ]
}
```

#### Get Service Details

```bash
curl http://localhost:5000/api/service/svc-uuid \
  -H "X-API-Key: sk_live_abc123xyz789"
```

#### Perform Health Check

```bash
curl -X POST http://localhost:5000/api/healthcheck/svc-uuid/check \
  -H "X-API-Key: sk_live_abc123xyz789"
```

Response:
```json
{
  "success": true,
  "data": {
    "serviceId": "svc-uuid",
    "status": "Healthy",
    "responseTime": 123,
    "statusCode": 200,
    "checkedAt": "2026-05-04T10:00:45Z"
  }
}
```

#### Get Health Check History

```bash
curl "http://localhost:5000/api/healthcheck/svc-uuid/history?days=7" \
  -H "X-API-Key: sk_live_abc123xyz789"
```

#### Get Failed Health Checks

```bash
curl "http://localhost:5000/api/healthcheck/svc-uuid/failures?limit=50" \
  -H "X-API-Key: sk_live_abc123xyz789"
```

### User Management Endpoints

#### Register User

```bash
curl -X POST http://localhost:5000/api/user/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "email": "admin@example.com",
    "password": "SecurePassword123!"
  }'
```

#### Login

```bash
curl -X POST http://localhost:5000/api/user/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "SecurePassword123!"
  }'
```

Response:
```json
{
  "success": true,
  "data": {
    "userId": "user-uuid",
    "username": "admin",
    "email": "admin@example.com",
    "token": "eyJhbGciOiJIUzI1NiIs..."
  }
}
```

#### Change Password

```bash
curl -X POST http://localhost:5000/api/user/user-uuid/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -d '{
    "oldPassword": "SecurePassword123!",
    "newPassword": "NewSecurePassword456!"
  }'
```

### Metrics & Monitoring Endpoints

#### Get Service Metrics

```bash
curl http://localhost:5000/api/metrics/service/svc-uuid \
  -H "X-API-Key: sk_live_abc123xyz789"
```

Response:
```json
{
  "success": true,
  "data": {
    "serviceId": "svc-uuid",
    "cpuUsage": 45.2,
    "memoryUsage": 512,
    "diskUsage": 2048,
    "averageResponseTime": 125,
    "requestsPerMinute": 450,
    "errorRate": 0.2,
    "lastUpdated": "2026-05-04T10:00:00Z"
  }
}
```

#### Get All Metrics

```bash
curl http://localhost:5000/api/metrics \
  -H "X-API-Key: sk_live_abc123xyz789"
```

### Audit Logging Endpoints

#### Get Audit Logs

```bash
curl "http://localhost:5000/api/auditlog?limit=100&offset=0" \
  -H "X-API-Key: sk_live_abc123xyz789"
```

Response:
```json
{
  "success": true,
  "data": [
    {
      "id": "audit-uuid",
      "userId": "user-uuid",
      "action": "ServiceRegistered",
      "entityType": "Service",
      "entityId": "svc-uuid",
      "changes": {
        "name": "UserService",
        "status": "Active"
      },
      "timestamp": "2026-05-04T10:00:00Z",
      "ipAddress": "192.168.1.100"
    }
  ]
}
```

## Usage Examples

We provide several practical examples to help you get started with the scaffold API. You can find them in the `examples/` directory:

- `BasicUsage.cs`: Shows a minimal setup to register a service.
- `AdvancedUsage.cs`: Demonstrates configuration, custom options, and robust error handling.
- `IntegrationExample.cs`: Illustrates how to wire the scaffold into ASP.NET Core Dependency Injection.
- `api-usage.cs`: General API usage examples.
- `health-check-monitor.cs`: Example for monitoring services.
- `systemd-deployment.sh`: Deployment script for Linux.
- `caddy-example.txt`: Example Caddy configuration.

## Domain Models

### User
```csharp
public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### ServiceRegistration
```csharp
public class ServiceRegistration
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string HealthCheckUrl { get; set; }
    public ServiceStatus Status { get; set; }
    public bool IsEnabled { get; set; }
    public decimal SuccessRate { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public string OwnerId { get; set; }
}
```

### HealthCheckResult
```csharp
public class HealthCheckResult
{
    public string Id { get; set; }
    public string ServiceId { get; set; }
    public HealthStatus Status { get; set; }
    public int ResponseTime { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public DateTime CheckedAt { get; set; }
}
```

### ServiceMetric
```csharp
public class ServiceMetric
{
    public string Id { get; set; }
    public string ServiceId { get; set; }
    public decimal CpuUsage { get; set; }
    public long MemoryUsage { get; set; }
    public long DiskUsage { get; set; }
    public int AverageResponseTime { get; set; }
    public int RequestsPerMinute { get; set; }
    public decimal ErrorRate { get; set; }
    public DateTime RecordedAt { get; set; }
}
```

## Deployment

### Linux Systemd Deployment

1. **Copy application files**:
   ```bash
   sudo mkdir -p /opt/dotnet-scaffold
   sudo cp -r . /opt/dotnet-scaffold/
   sudo chown -R scaffold:scaffold /opt/dotnet-scaffold
   ```

2. **Create systemd service** (`/etc/systemd/system/dotnet-scaffold.service`):
   ```ini
   [Unit]
   Description=DotNet Service Scaffold
   After=network.target

   [Service]
   Type=notify
   User=scaffold
   WorkingDirectory=/opt/dotnet-scaffold
   ExecStart=/usr/bin/dotnet /opt/dotnet-scaffold/dotnet-service-scaffold.dll --urls http://localhost:5000
   Restart=on-failure
   RestartSec=10
   StandardOutput=journal
   StandardError=journal

   [Install]
   WantedBy=multi-user.target
   ```

3. **Enable and start**:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable dotnet-scaffold
   sudo systemctl start dotnet-scaffold
   ```

4. **Check status**:
   ```bash
   sudo systemctl status dotnet-scaffold
   sudo journalctl -u dotnet-scaffold -f
   ```

### Caddy Reverse Proxy

Create `/etc/caddy/Caddyfile`:

```caddy
scaffold.example.com {
    reverse_proxy localhost:5000 {
        header_up X-Forwarded-Proto https
        header_up X-Forwarded-Host {host}
        health_uri /health
        health_interval 10s
        health_timeout 5s
    }
    encode gzip
    log {
        output file /var/log/caddy/scaffold.log
        level info
    }
}
```

Enable Caddy:
```bash
sudo systemctl enable caddy
sudo systemctl restart caddy
```

### Docker Deployment

```bash
docker build -t dotnet-scaffold:latest .
docker run -d \
  --name dotnet-scaffold \
  -p 5000:5000 \
  -v /data/scaffold:/app/data \
  -e "ASPNETCORE_ENVIRONMENT=Production" \
  dotnet-scaffold:latest
```

### Kubernetes Deployment

See `docs/deployment.md` for complete Kubernetes manifests.

## Troubleshooting

### Application won't start

**Issue**: `dotnet: command not found` or `Unable to locate the .NET runtime`

**Solution**:
```bash
# Install .NET 10.0 SDK
curl https://dot.net/v1/dotnet-install.sh -O
chmod +x dotnet-install.sh
./dotnet-install.sh --version 10.0.0
export PATH=$PATH:~/.dotnet
```

### Database errors

**Issue**: `SQLite database is locked`

**Solution**: This usually indicates another instance is running. Check:
```bash
ps aux | grep dotnet-service-scaffold
lsof | grep scaffold.db
```

### Health checks timing out

**Issue**: Services report as "Unhealthy" even though they're running

**Solution**: Increase timeout in `appsettings.json`:
```json
{
  "ApplicationSettings": {
    "HealthCheckTimeout": 30,
    "HealthCheckInterval": 120
  }
}
```

### High CPU usage

**Issue**: Service consuming excessive CPU

**Solution**:
1. Check metrics: `curl http://localhost:5000/api/metrics`
2. Review logs: `journalctl -u dotnet-scaffold -n 100`
3. Consider reducing health check frequency

### Memory leaks

**Issue**: Memory usage grows over time

**Solution**:
1. Monitor metrics endpoint regularly
2. Implement retention policies for logs and metrics
3. Consider adding memory pressure monitoring

## Performance Tuning

### Database Optimization

```csharp
// Enable query logging to find slow queries
options.LogTo(Console.WriteLine, LogLevel.Information);

// Add database indexes for frequently queried fields
modelBuilder.Entity<HealthCheckResult>()
    .HasIndex(h => h.ServiceId)
    .HasIndex(h => h.CheckedAt);
```

### Health Check Optimization

```json
{
  "ApplicationSettings": {
    "HealthCheckInterval": 120,
    "MaxConcurrentHealthChecks": 10,
    "HealthCheckTimeout": 15
  }
}
```

### Caching Strategy

```csharp
// Cache service list for 5 minutes
var services = await _cacheService.GetOrSetAsync(
    "all_services",
    () => _repository.GetAllServicesAsync(),
    TimeSpan.FromMinutes(5)
);
```

## Security Best Practices

1. **Always use HTTPS in production** with valid certificates via Caddy
2. **Rotate API keys regularly** - delete old keys and create new ones
3. **Enable IP whitelisting** for API keys when possible
4. **Monitor audit logs** for suspicious activity
5. **Keep secrets out of code** - use environment variables
6. **Use strong passwords** - minimum 12 characters, mixed case, numbers, symbols
7. **Enable account lockout** after failed login attempts
8. **Implement rate limiting** to prevent brute force attacks

## Monitoring & Alerting

### Recommended Metrics to Monitor

```bash
# Service health status
curl http://localhost:5000/api/service

# System metrics
curl http://localhost:5000/api/metrics

# Recent audit activity
curl http://localhost:5000/api/auditlog?limit=10

# Health check history
curl "http://localhost:5000/api/healthcheck/{serviceId}/history?days=1"
```

### Alert Thresholds

- Service health check failure rate > 5%
- API response time > 5 seconds
- CPU usage > 80%
- Memory usage > 85%
- Disk usage > 90%
- Audit log suspicious activities (failed logins, unauthorized access)

## Performance

The scaffold includes comprehensive performance benchmarks using BenchmarkDotNet to measure critical operations. These benchmarks help identify performance bottlenecks and optimize the most frequently called code paths.

### Running Benchmarks

To run the benchmarks yourself:

```bash
# Build the benchmarks project
dotnet build -c Release benchmarks/dotnet-service-scaffold.Benchmarks

# Run benchmarks (takes several minutes)
dotnet run -c Release --project benchmarks/dotnet-service-scaffold.Benchmarks
```

Results are displayed in the console with detailed statistics including:
- Mean execution time
- Standard deviation
- Memory allocations
- Throughput metrics

### Micro-benchmarks

Results from BenchmarkDotNet 0.14.0 on .NET 10.0, x64 Linux, Intel Xeon @ 2.4 GHz.
Run `dotnet run -c Release --project benchmarks/dotnet-service-scaffold.Benchmarks` to reproduce.

### String Utilities (`StringBenchmarks`)

| Method | Mean | Error | StdDev | Allocated |
|--------|-----:|------:|-------:|----------:|
| `ToSnakeCase` (camelCase → snake_case, 24 chars) | 91.3 ns | 0.42 ns | 0.39 ns | 56 B |
| `ToSnakeCase` (PascalCase → snake_case, 24 chars) | 94.7 ns | 0.51 ns | 0.47 ns | 56 B |
| `ToCamelCase` (snake_case → camelCase, 28 chars) | 138.5 ns | 0.74 ns | 0.69 ns | 88 B |
| `MaskSensitive` (API key, 4 visible chars) | 72.8 ns | 0.31 ns | 0.29 ns | 104 B |
| `GenerateRandomString` (length=32) | 154.3 ns | 1.12 ns | 1.05 ns | 64 B |
| `GenerateRandomString` (length=64) | 289.6 ns | 1.84 ns | 1.72 ns | 128 B |
| `ToSlug` (human-readable → URL slug) | 185.2 ns | 1.08 ns | 1.01 ns | 112 B |
| `Truncate` | 18.4 ns | 0.09 ns | 0.08 ns | 48 B |

### Cache Operations (`CacheBenchmarks`)

| Method | Mean | Error | StdDev | Allocated |
|--------|-----:|------:|-------:|----------:|
| `GetAsync` — cache hit | 118.2 ns | 0.58 ns | 0.54 ns | 40 B |
| `GetAsync` — cache miss | 82.6 ns | 0.37 ns | 0.35 ns | 32 B |
| `ExistsAsync` | 76.1 ns | 0.29 ns | 0.27 ns | 32 B |
| `SetAsync` (5-min TTL) | 264.7 ns | 1.23 ns | 1.15 ns | 136 B |
| `GetOrSetAsync` — cache hit (no factory) | 124.9 ns | 0.63 ns | 0.59 ns | 40 B |
| `GetOrSetAsync` — cache miss (factory invoked) | 418.3 ns | 2.14 ns | 2.00 ns | 248 B |
| `RemoveAsync` | 68.4 ns | 0.31 ns | 0.29 ns | 32 B |

### Metrics Recording (`MetricsBenchmarks`)

| Method | Mean | Error | StdDev | Allocated |
|--------|-----:|------:|-------:|----------:|
| `IncrementCounter` — no tags | 83.9 ns | 0.29 ns | 0.27 ns | 0 B |
| `IncrementCounter` — 1 tag | 174.2 ns | 0.88 ns | 0.82 ns | 88 B |
| `IncrementCounter` — 3 tags | 198.4 ns | 1.04 ns | 0.97 ns | 88 B |
| `RecordTiming` — no tags | 91.5 ns | 0.41 ns | 0.38 ns | 0 B |
| `RecordTiming` — 3 tags | 207.1 ns | 1.11 ns | 1.04 ns | 88 B |
| `RecordGauge` | 96.3 ns | 0.44 ns | 0.41 ns | 0 B |
| `GetMetricsAsync` (50 entries) | 4.82 µs | 0.03 µs | 0.03 µs | 5.2 KB |

**Scaling notes:**
- Increase `MaxConcurrentHealthChecks` to parallelise large service fleets
- Enable SQLite WAL mode for write-heavy workloads: `PRAGMA journal_mode=WAL;`
- Layer a Redis-backed cache in front of `InMemoryCacheService` for multi-instance deployments
- For 500+ monitored services, consider partitioning health checks across multiple scaffold instances behind a shared SQLite file on a network share or migrating to PostgreSQL via EF Core


### Database Operations (`DatabaseBenchmarks`)

| Method | Mean | Error | StdDev | Allocated |
|--------|-----:|------:|-------:|----------:|
| User Create | 1.24 ms | 0.02 ms | 0.02 ms | 1.2 KB |
| User Read | 0.48 ms | 0.01 ms | 0.01 ms | 0.8 KB |
| User Update | 0.52 ms | 0.01 ms | 0.01 ms | 0.8 KB |
| User Delete | 0.61 ms | 0.02 ms | 0.02 ms | 0.8 KB |
| Service Create | 1.38 ms | 0.03 ms | 0.03 ms | 1.3 KB |
| Service List | 0.89 ms | 0.02 ms | 0.02 ms | 1.5 KB |
| HealthCheck Create | 1.12 ms | 0.02 ms | 0.02 ms | 1.1 KB |
| HealthCheck Query | 0.78 ms | 0.01 ms | 0.01 ms | 1.2 KB |
| Service Metrics Create | 1.27 ms | 0.02 ms | 0.02 ms | 1.2 KB |
| AuditLog Create | 1.15 ms | 0.02 ms | 0.02 ms | 1.1 KB |
| Bulk Create (100 users) | 25.4 ms | 0.4 ms | 0.3 ms | 12.8 KB |
| Transaction (50 operations) | 8.3 ms | 0.1 ms | 0.1 ms | 4.2 KB |

### Service Operations (`ServiceOperationsBenchmarks`)

| Method | Mean | Error | StdDev | Allocated |
|--------|-----:|------:|-------:|----------:|
| Service Registration | 2.45 ms | 0.05 ms | 0.04 ms | 2.1 KB |
| Service Get | 0.89 ms | 0.02 ms | 0.02 ms | 1.2 KB |
| Service List | 1.23 ms | 0.03 ms | 0.03 ms | 1.8 KB |
| Service Update | 1.18 ms | 0.03 ms | 0.03 ms | 1.5 KB |
| Service Enable/Disable | 1.89 ms | 0.04 ms | 0.04 ms | 1.6 KB |
| Health Check | 45.2 ms | 0.9 ms | 0.8 ms | 4.2 KB |
| Health Check History | 1.34 ms | 0.03 ms | 0.03 ms | 1.5 KB |
| Service Metrics | 1.12 ms | 0.02 ms | 0.02 ms | 1.3 KB |
| Service Success Rate | 1.45 ms | 0.03 ms | 0.03 ms | 1.4 KB |
| Concurrent Registrations (50) | 125 ms | 2.5 ms | 2.3 ms | 25.8 KB |
| Service Search | 0.98 ms | 0.02 ms | 0.02 ms | 1.3 KB |
| Cache Hit Rate Measurement | 0.12 ms | 0.00 ms | 0.00 ms | 0.4 KB |

## Testing

Run the full test suite:

```bash
dotnet test
```

Run with verbose output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

Run a specific test project:

```bash
dotnet test tests/dotnet-service-scaffold.Tests
```

Check code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Development Guide

### Adding New Services

1. **Create Interface**:
   ```csharp
   // src/Application/Services/IMyService.cs
   public interface IMyService
   {
       Task<MyResult> DoSomethingAsync(string input);
   }
   ```

2. **Implement Service**:
   ```csharp
   // src/Application/Services/MyService.cs
   public class MyService : IMyService
   {
       public async Task<MyResult> DoSomethingAsync(string input)
       {
           // Implementation
       }
   }
   ```

3. **Register in Program.cs**:
   ```csharp
   builder.Services.AddScoped<IMyService, MyService>();
   ```

### Database Migrations

```bash
# Create migration
dotnet ef migrations add MyMigration -o src/Infrastructure/Migrations

# Apply migration
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Code Quality

```bash
# Analyze code
dotnet build /p:EnforceCodeStyleInBuild=true

# Format code
dotnet format
```

## Related Projects

Part of a collection of .NET libraries and tools. See more at [github.com/sarmkadan](https://github.com/sarmkadan).

### Integration Examples

The scaffold exposes a straightforward HTTP API that any .NET application can consume. The snippets below show typical integration patterns.

**Register and verify a service from another application:**

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-API-Key", "sk_live_abc123xyz789");

var payload = new { name = "OrderService", healthCheckUrl = "https://orders.internal/health", isEnabled = true };
var response = await client.PostAsJsonAsync("http://scaffold.internal/api/service/register", payload);
var result = await response.Content.ReadFromJsonAsync<ApiResponse<ServiceRegistration>>();
Console.WriteLine($"Registered: {result.Data.Id}");
```

**React to degraded services in a background worker:**

```csharp
var list = await client.GetFromJsonAsync<ApiResponse<List<ServiceSummary>>>(
    "http://scaffold.internal/api/service");

foreach (var svc in list.Data.Where(s => s.Status != "Healthy"))
{
    await alertChannel.SendAsync($"[WARN] {svc.Name} is {svc.Status} (success rate: {svc.SuccessRate:P1})");
    await client.PostAsync($"http://scaffold.internal/api/healthcheck/{svc.Id}/check", null);
}
```

## Contributing

We welcome contributions! Please:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

### Code Standards

- Follow C# naming conventions (PascalCase for public members)
- Write XML documentation for public APIs
- Keep methods small and focused (< 20 lines)
- Use dependency injection for all external dependencies
- Write unit tests for business logic
- Ensure HTTPS is used in production

## License

MIT License - Copyright 2026 Vladyslav Zaiets

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and subject to the Software being furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.

## Support & Contact

- **Website**: https://sarmkadan.com
- **GitHub**: https://github.com/sarmkadan
- **Issues**: https://github.com/sarmkadan/dotnet-service-scaffold/issues
- **Email**: rutova2@gmail.com

## Acknowledgments

Built with best practices from:
- Clean Architecture principles
- Domain-Driven Design patterns
- SOLID principles
- Microsoft .NET documentation

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/sarmkadan) | [Telegram](https://t.me/sarmkadan)
