# Architecture Guide

This document describes the overall architecture, design patterns, and structure of dotnet-service-scaffold.

## High-Level Architecture

The application follows **Clean Architecture** principles with clear separation of concerns across four layers:

```
┌────────────────────────────────────────┐
│    PRESENTATION LAYER (Controllers)    │
│  Handles HTTP requests and responses   │
└────────────────┬───────────────────────┘
                 │
┌────────────────▼───────────────────────┐
│    MIDDLEWARE LAYER                    │
│  Cross-cutting concerns (auth, logging)│
└────────────────┬───────────────────────┘
                 │
┌────────────────▼───────────────────────┐
│  APPLICATION LAYER (Services)          │
│  Business logic and orchestration      │
└────────────────┬───────────────────────┘
                 │
┌────────────────▼───────────────────────┐
│   DOMAIN LAYER (Models & Logic)        │
│  Core business rules and entities      │
└────────────────┬───────────────────────┘
                 │
┌────────────────▼───────────────────────┐
│ INFRASTRUCTURE LAYER (Repositories)    │
│  Data access and external services     │
└────────────────┬───────────────────────┘
                 │
┌────────────────▼───────────────────────┐
│    SQLite Database                     │
└────────────────────────────────────────┘
```

## Layer Descriptions

### 1. Presentation Layer

**Location**: `src/Presentation/`

Responsible for handling HTTP requests, validating input, and returning responses.

**Key Components**:
- **Controllers** - Handle API endpoints
  - `UserController` - User authentication and management
  - `ServiceController` - Service registration and lifecycle
  - `HealthCheckController` - Health check execution and history
  - `MetricsController` - Performance metrics retrieval
  - `AuditLogController` - Audit trail access
  - `ApiKeyController` - API key management

**Pattern**: MVC Pattern with REST semantics

```csharp
[ApiController]
[Route("api/[controller]")]
public class ServiceController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterService(RegisterServiceRequest request)
    {
        // Delegate to application service
        var result = await _serviceManagementService.RegisterServiceAsync(request);
        return Ok(new { success = true, data = result });
    }
}
```

### 2. Middleware Layer

**Location**: `src/Presentation/Middleware/`

Provides cross-cutting concerns for the request pipeline.

**Key Middleware**:
- **ApiKeyAuthenticationMiddleware** - Validates API keys
- **ErrorHandlingMiddleware** - Catches exceptions and returns proper responses
- **RequestLoggingMiddleware** - Logs all requests/responses
- **RateLimitingMiddleware** - Prevents abuse

**Example**:
```csharp
public class ApiKeyAuthenticationMiddleware
{
    // Validates X-API-Key header
    // Checks IP whitelist
    // Adds user context to request
}
```

### 3. Application Layer

**Location**: `src/Application/Services/`

Contains business logic and service coordination.

**Key Services**:

| Service | Responsibility |
|---------|-----------------|
| `UserService` | User management, authentication |
| `HealthCheckService` | Health probe execution, monitoring |
| `ServiceManagementService` | Service registration, lifecycle |
| `AuditService` | Audit logging and compliance |
| `ConfigurationService` | Application settings management |
| `FeatureFlagService` | Feature toggles |
| `MetricsService` | Performance metrics collection |

**Design Pattern**: Service/Interface pattern

```csharp
public interface IHealthCheckService
{
    Task<HealthCheckResult> CheckServiceHealthAsync(string serviceId);
    Task<List<HealthCheckResult>> GetHistoryAsync(string serviceId, int days);
    Task<List<HealthCheckResult>> GetFailuresAsync(string serviceId);
}

public class HealthCheckService : IHealthCheckService
{
    private readonly IHealthCheckRepository _repository;
    private readonly HttpClient _httpClient;

    // Implementation delegates to repositories
}
```

### 4. Domain Layer

**Location**: `src/Domain/`

Represents core business entities and rules.

**Key Entities**:

```
User
├── Id
├── Username
├── Email
├── PasswordHash
├── FailedLoginAttempts
├── LastLoginAt
└── IsActive

ServiceRegistration
├── Id
├── Name
├── Description
├── HealthCheckUrl
├── Status (Healthy/Unhealthy/Unknown)
├── SuccessRate
├── LastCheckedAt
└── OwnerId (references User)

HealthCheckResult
├── Id
├── ServiceId (references ServiceRegistration)
├── Status (Healthy/Degraded/Unhealthy)
├── ResponseTime
├── StatusCode
├── Message
└── CheckedAt

ServiceMetric
├── Id
├── ServiceId (references ServiceRegistration)
├── CpuUsage
├── MemoryUsage
├── DiskUsage
├── AverageResponseTime
├── RequestsPerMinute
├── ErrorRate
└── RecordedAt

AuditLog
├── Id
├── UserId (references User)
├── Action (ServiceRegistered, HealthCheckFailed, etc.)
├── EntityType
├── EntityId
├── Changes (JSON)
├── Timestamp
└── IpAddress

ApiKey
├── Id
├── Key (hashed)
├── Name
├── Scopes (service:read, service:write, etc.)
├── IpWhitelist
├── LastUsedAt
├── CreatedAt
└── ExpiresAt (optional)

ServiceConfiguration
├── Id
├── Key (setting name)
├── Value (JSON serialized)
├── Type (string, int, bool, json)
├── IsEncrypted
└── UpdatedAt
```

**Enumerations**:

```csharp
public enum HealthStatus { Healthy, Degraded, Unhealthy }
public enum ServiceStatus { Active, Inactive, Pending, Failed }
public enum ServiceEventType { Registered, HealthCheckPassed, HealthCheckFailed, StatusChanged }
```

**Exceptions**:

```csharp
public class ServiceScaffoldException : Exception { }
public class ServiceNotFoundException : ServiceScaffoldException { }
public class ServiceValidationException : ServiceScaffoldException { }
public class UnauthorizedException : ServiceScaffoldException { }
public class InvalidApiKeyException : ServiceScaffoldException { }
```

### 5. Infrastructure Layer

**Location**: `src/Infrastructure/`

Implements data access and external service integration.

**Key Components**:

#### Data Access (Repository Pattern)

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public class ServiceRepository : Repository<ServiceRegistration>
{
    // Specialized queries for ServiceRegistration
    public async Task<List<ServiceRegistration>> GetByOwnerIdAsync(string ownerId)
    {
        return await _context.Services
            .Where(s => s.OwnerId == ownerId)
            .ToListAsync();
    }
}
```

#### Database Context

```csharp
public class ServiceScaffoldDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<ServiceRegistration> Services { get; set; }
    public DbSet<HealthCheckResult> HealthCheckResults { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    // ... more DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configuration
        modelBuilder.Entity<User>()
            .HasMany(u => u.Services)
            .WithOne()
            .HasForeignKey(s => s.OwnerId);

        modelBuilder.Entity<HealthCheckResult>()
            .HasIndex(h => h.ServiceId)
            .HasIndex(h => h.CheckedAt);
    }
}
```

#### Caching

```csharp
public interface ICacheService
{
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration);
    Task RemoveAsync(string key);
}

// InMemoryCacheService provides basic caching
// Can be extended with Redis for distributed scenarios
```

#### External Integration

```csharp
public class ExternalApiClient
{
    // Calls third-party APIs
    // Implements retry logic and timeout handling
}

public class WebhookClient
{
    // Sends webhooks to registered endpoints
    // Implements signing and verification
}
```

## Data Flow Example: Service Health Check

Here's how a health check request flows through the architecture:

```
1. HTTP Request arrives at HealthCheckController

2. Controller validates request
   - Deserializes JSON
   - Checks authentication (via middleware)

3. Calls IHealthCheckService.CheckServiceHealthAsync()

4. HealthCheckService:
   - Retrieves service details from IServiceRepository
   - Makes HTTP request using HttpClient
   - Creates HealthCheckResult entity
   - Calls IHealthCheckRepository.AddAsync()
   - Updates cache

5. HealthCheckRepository (Infrastructure):
   - Adds entity to ServiceScaffoldDbContext
   - Calls SaveChangesAsync()
   - Entity Framework Core:
     - Maps entity to SQL
     - Executes INSERT against SQLite

6. AuditService logs the action
   - Creates AuditLog entry
   - Records timestamp and user info

7. Controller returns Result<HealthCheckResult>
   - Serialized to JSON
   - HTTP 200 with data
```

## Dependency Injection

All dependencies are registered in `Program.cs`:

```csharp
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

// Infrastructure
builder.Services.AddDbContext<ServiceScaffoldDbContext>();
builder.Services.AddScoped<ICacheService, InMemoryCacheService>();
```

**Key Principle**: Depend on abstractions, not implementations.

## Design Patterns Used

### 1. Repository Pattern
- Abstracts data access layer
- Enables testing with mocks
- Centralizes query logic

### 2. Service Pattern
- Encapsulates business logic
- Coordinates between repositories
- Provides reusable operations

### 3. Middleware Pattern
- Processes requests in pipeline
- Handles cross-cutting concerns
- Allows composability

### 4. Dependency Injection
- Loose coupling between components
- Easier testing and maintenance
- Built into ASP.NET Core

### 5. Value Object Pattern
- Immutable objects representing values
- Example: HealthStatus enum
- Improves type safety

### 6. Domain Event Pattern
- Models significant business events
- Future: event sourcing support
- Currently used for audit logging

## Database Schema

Key tables and relationships:

```sql
-- Users table
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,
    Username TEXT UNIQUE NOT NULL,
    Email TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    IsActive INTEGER DEFAULT 1,
    FailedLoginAttempts INTEGER DEFAULT 0,
    LastLoginAt TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

-- Services table (Foreign Key: UserId)
CREATE TABLE Services (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    HealthCheckUrl TEXT,
    Status TEXT DEFAULT 'Pending',
    IsEnabled INTEGER DEFAULT 1,
    SuccessRate REAL DEFAULT 100.0,
    LastCheckedAt TEXT,
    OwnerId TEXT NOT NULL,
    FOREIGN KEY (OwnerId) REFERENCES Users(Id)
);

-- Health Check Results (Foreign Key: ServiceId)
CREATE TABLE HealthCheckResults (
    Id TEXT PRIMARY KEY,
    ServiceId TEXT NOT NULL,
    Status TEXT NOT NULL,
    ResponseTime INTEGER,
    StatusCode INTEGER,
    Message TEXT,
    CheckedAt TEXT NOT NULL,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id)
);

-- Indexes for performance
CREATE INDEX idx_services_owner ON Services(OwnerId);
CREATE INDEX idx_healthchecks_service ON HealthCheckResults(ServiceId);
CREATE INDEX idx_healthchecks_timestamp ON HealthCheckResults(CheckedAt);
```

## Error Handling

All exceptions inherit from `ServiceScaffoldException`:

```csharp
try
{
    // Business logic
}
catch (ServiceNotFoundException ex)
{
    // Log and return 404
    return NotFound(new { error = ex.Message });
}
catch (ServiceValidationException ex)
{
    // Log and return 400
    return BadRequest(new { error = ex.Message });
}
catch (Exception ex)
{
    // Log unexpected error
    // Return 500 with generic message
    return StatusCode(500, new { error = "Internal server error" });
}
```

## Security Architecture

### Authentication Flow

```
API Key in Header
        ↓
ApiKeyAuthenticationMiddleware validates
        ↓
IP whitelist check
        ↓
Scope verification
        ↓
Request proceeds with ApiKeyContext
```

### Password Security

- BCrypt hashing with salt
- Account lockout after 5 failed attempts
- Minimum 8 character passwords
- No password reversal

### Audit Trail

Every significant action logged:
- Service registration/deletion
- Health check results
- User login/logout
- Configuration changes
- API key usage

## Scalability Considerations

### Current Limitations
- In-memory caching (not distributed)
- Single SQLite database (no sharding)
- Synchronous health checks (limited concurrency)

### Future Improvements
- Redis integration for distributed caching
- PostgreSQL support for horizontal scaling
- Async health check batching
- Event sourcing for audit logs
- Message queue integration (RabbitMQ/Kafka)

## Testing Strategy

### Unit Tests
Test individual services in isolation with mocked repositories:

```csharp
[Fact]
public async Task RegisterService_WithValidData_Success()
{
    // Arrange
    var mockRepository = new Mock<IServiceRepository>();
    var service = new ServiceManagementService(mockRepository.Object);

    // Act
    var result = await service.RegisterServiceAsync(request);

    // Assert
    Assert.NotNull(result);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<ServiceRegistration>()), Times.Once);
}
```

### Integration Tests
Test full request/response cycle with in-memory database.

### Load Testing
Monitor performance under high health check load:
```bash
# Simulate 100 concurrent health checks
wrk -t 8 -c 100 -d 30s http://localhost:5000/api/healthcheck/svc-uuid/check
```

## Extending the Architecture

### Adding a New Feature

1. **Define Domain Model** (`src/Domain/Models/`)
2. **Create Repository Interface** (`src/Infrastructure/Data/Repository/`)
3. **Implement Repository** with DbContext
4. **Create Service Interface** (`src/Application/Services/`)
5. **Implement Service** with business logic
6. **Create Controller** (`src/Presentation/Controllers/`)
7. **Register in Program.cs**
8. **Write Tests**
9. **Update Documentation**

### Example: Adding a Notification Service

```csharp
// 1. Domain Model
public class Notification
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 2. Repository
public interface INotificationRepository : IRepository<Notification>
{
    Task<List<Notification>> GetUnreadAsync(string userId);
}

// 3. Service
public interface INotificationService
{
    Task SendNotificationAsync(string userId, string message);
    Task MarkAsReadAsync(string notificationId);
}

// 4. Controller
[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread(string userId)
    {
        var notifications = await _notificationService.GetUnreadAsync(userId);
        return Ok(notifications);
    }
}

// 5. Register in Program.cs
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
```

## Performance Optimization

### Database Queries
- Use `Include()` for eager loading
- Create indexes on frequently queried columns
- Implement pagination for large result sets

### Caching Strategy
- Cache service list (changes infrequently)
- Cache configuration values
- Set appropriate TTLs

### Health Check Optimization
- Batch health checks
- Implement circuit breaker for failing services
- Use timeout to prevent hung requests

## Conclusion

The architecture is designed to be:
- **Maintainable** - Clear separation of concerns
- **Testable** - Dependency injection and interfaces
- **Extensible** - Easy to add new features
- **Scalable** - Foundation for growth
- **Secure** - Built-in authentication and auditing
