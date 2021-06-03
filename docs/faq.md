# Frequently Asked Questions

Common questions and answers about dotnet-service-scaffold.

## Installation & Setup

### Q: What are the system requirements?

**A:** Minimum requirements:
- .NET 10.0 SDK or runtime
- 1 GB RAM
- 100 MB disk space
- Linux, macOS, or Windows

For production:
- 4+ GB RAM
- SSD storage
- Linux (Ubuntu 20.04+ recommended)
- systemd or Docker

### Q: Can I run this on Windows?

**A:** Yes, but Linux is recommended for production. For Windows:
1. Install .NET 10.0 SDK
2. Clone repository
3. Run `dotnet run`
4. Use IIS or standalone hosting

For production on Windows, consider:
- Windows Server 2019+ with IIS
- Azure App Service
- Docker on Windows Server

### Q: How do I change the port?

**A:** Set the port via environment variable or command line:

```bash
# Environment variable
export ASPNETCORE_URLS="http://localhost:8080"
dotnet run

# Or command line
dotnet run -- --urls "http://0.0.0.0:8080"

# Or appsettings.json
{
  "Urls": "http://0.0.0.0:5000"
}
```

### Q: How do I use a different database?

**A:** By default, SQLite is used. To use PostgreSQL:

1. Install EntityFrameworkCore.Npgsql:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.Npgsql
   ```

2. Update Program.cs:
   ```csharp
   builder.Services.AddDbContext<ServiceScaffoldDbContext>(options =>
       options.UseNpgsql(connectionString));
   ```

3. Update connection string in appsettings.json:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=scaffold;User Id=scaffold;"
     }
   }
   ```

## Configuration

### Q: How do I configure health check intervals?

**A:** Edit `appsettings.json`:

```json
{
  "ApplicationSettings": {
    "HealthCheckInterval": 60,          // Check every 60 seconds
    "HealthCheckTimeout": 10,           // Wait max 10 seconds
    "MaxConcurrentHealthChecks": 5,     // Run 5 in parallel
    "HealthCheckResultRetentionDays": 30
  }
}
```

### Q: Can I change password requirements?

**A:** Yes, in `appsettings.json`:

```json
{
  "ApplicationSettings": {
    "PasswordMinimumLength": 12,
    "PasswordRequireUppercase": true,
    "PasswordRequireNumbers": true,
    "PasswordRequireSpecialChars": true
  }
}
```

### Q: How long are logs kept?

**A:** Configure in appsettings.json:

```json
{
  "ApplicationSettings": {
    "AuditLogRetentionDays": 90,
    "HealthCheckResultRetentionDays": 30
  }
}
```

Old logs are automatically deleted based on these settings.

### Q: Can I disable a feature?

**A:** Use feature flags in `ConfigurationService`:

```csharp
// In appsettings.json
{
  "Features": {
    "EnableAuditLogging": true,
    "EnableMetricsCollection": true,
    "EnableHealthChecks": true
  }
}

// In service
var auditEnabled = await _configService.GetFeatureAsync("EnableAuditLogging");
if (auditEnabled)
{
    await _auditService.LogAsync(...);
}
```

## API & Authentication

### Q: How do I create an API key?

**A:** Use the API endpoint:

```bash
curl -X POST http://localhost:5000/api/apikey/create \
  -H "Content-Type: application/json" \
  -H "X-API-Key: existing-admin-key" \
  -d '{
    "name": "MyService",
    "description": "For monitoring",
    "scopes": ["service:read", "healthcheck:read"]
  }'
```

The API key is displayed once. Save it securely.

### Q: How do I rotate API keys?

**A:** Create new key, update clients, then revoke old key:

```bash
# Create new key
curl -X POST http://localhost:5000/api/apikey/create ...

# Update all clients to use new key

# Revoke old key
curl -X POST http://localhost:5000/api/apikey/{keyId}/revoke \
  -H "X-API-Key: new-admin-key"
```

### Q: Can I restrict an API key to specific IPs?

**A:** Yes, use IP whitelist when creating key:

```bash
curl -X POST http://localhost:5000/api/apikey/create \
  -H "X-API-Key: admin-key" \
  -d '{
    "name": "CI/CD",
    "ipWhitelist": [
      "10.0.0.0/8",
      "172.16.0.0/12",
      "203.0.113.5"
    ]
  }'
```

### Q: How do I make API calls in code?

**A:** Use HttpClient:

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-API-Key", "sk_live_key");

var response = await client.GetAsync(
    "http://localhost:5000/api/service");

var json = await response.Content.ReadAsStringAsync();
var services = JsonSerializer.Deserialize<List<Service>>(json);
```

### Q: How do I authenticate as a user instead of API key?

**A:** Login to get JWT token:

```bash
# Login
curl -X POST http://localhost:5000/api/user/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "password"
  }'

# Use returned token
curl http://localhost:5000/api/user/me \
  -H "Authorization: Bearer eyJhbGc..."
```

## Database

### Q: How do I backup the database?

**A:** SQLite makes this simple:

```bash
# Copy database file
cp scaffold.db scaffold.db.backup

# Or use sqlite3
sqlite3 scaffold.db ".backup backup.db"

# Or for incremental backups
tar czf scaffold-$(date +%Y%m%d).db.tar.gz scaffold.db
```

### Q: How do I restore from backup?

**A:**
1. Stop the application
2. Restore the database file
3. Start the application

```bash
sudo systemctl stop dotnet-scaffold
sudo cp scaffold.db.backup /var/lib/dotnet-scaffold/scaffold.db
sudo chown scaffold:scaffold /var/lib/dotnet-scaffold/scaffold.db
sudo systemctl start dotnet-scaffold
```

### Q: Can I access the database directly?

**A:** Yes, use sqlite3:

```bash
sqlite3 /var/lib/dotnet-scaffold/scaffold.db

# List tables
.tables

# Query users
SELECT * FROM Users;

# Query services
SELECT * FROM Services;

# Exit
.exit
```

⚠️ **Warning**: Don't modify data directly - use the API instead.

### Q: Is the database locked?

**A:** Check for lock files:

```bash
ls -la /var/lib/dotnet-scaffold/scaffold.db*
```

If locked after crash:
```bash
rm /var/lib/dotnet-scaffold/scaffold.db-wal
rm /var/lib/dotnet-scaffold/scaffold.db-shm
```

### Q: How do I migrate to PostgreSQL?

**A:** See [Deployment Guide](deployment.md#database-optimization).

## Health Checks

### Q: Why is my service showing as unhealthy?

**A:** Check:

1. Service URL is correct
   ```bash
   curl https://your-service/health
   ```

2. Response time isn't too slow
   ```json
   {
     "ApplicationSettings": {
       "HealthCheckTimeout": 30  // Increase if needed
     }
   }
   ```

3. Service is responding on expected port
   ```bash
   netstat -tlnp | grep your-service
   ```

4. No firewall blocking
   ```bash
   telnet service-ip service-port
   ```

### Q: How often are health checks run?

**A:** By default every 60 seconds. Configure:

```json
{
  "ApplicationSettings": {
    "HealthCheckInterval": 30  // Check every 30 seconds
  }
}
```

### Q: Can I run health checks manually?

**A:** Yes, via API:

```bash
curl -X POST http://localhost:5000/api/healthcheck/svc-uuid/check \
  -H "X-API-Key: key"
```

### Q: What do the health statuses mean?

- **Healthy** - Service responding normally
- **Degraded** - Service responding slowly
- **Unhealthy** - Service not responding

Degraded when response time > 5 seconds (configurable).

## Monitoring & Logs

### Q: Where are logs stored?

**Development**: Console and `logs/` directory
**Production**: Systemd journal and `/var/log/dotnet-scaffold/`

View logs:
```bash
# Systemd
sudo journalctl -u dotnet-scaffold -f

# File
tail -f /var/log/dotnet-scaffold/scaffold-*.txt
```

### Q: How do I change log level?

**A:** In appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",  // Change to Debug/Warning/Error
      "Microsoft": "Warning"
    }
  }
}
```

### Q: How can I export metrics?

**A:** Use metrics API:

```bash
curl http://localhost:5000/api/metrics \
  -H "X-API-Key: key" | jq '.'
```

Then save to file or send to monitoring system.

## Troubleshooting

### Q: Application crashes on startup

**A:** Check logs:

```bash
dotnet run
```

Common issues:
- Port already in use - use different port
- Database locked - remove lock files
- Missing .NET SDK - install .NET 10.0

### Q: Service reports 503 (Service Unavailable)

**A:** Likely database issue:

```bash
# Check database
sqlite3 /var/lib/dotnet-scaffold/scaffold.db "SELECT 1;"

# Restart service
sudo systemctl restart dotnet-scaffold

# Check logs
sudo journalctl -u dotnet-scaffold -n 50
```

### Q: Memory usage keeps growing

**A:** Possible memory leak:

1. Monitor metrics
   ```bash
   curl http://localhost:5000/api/metrics
   ```

2. Check for large result sets
   ```bash
   sqlite3 scaffold.db "SELECT COUNT(*) FROM HealthCheckResults;"
   ```

3. Increase retention cleanup
   ```json
   {
     "ApplicationSettings": {
       "HealthCheckResultRetentionDays": 7
     }
   }
   ```

4. Restart service
   ```bash
   sudo systemctl restart dotnet-scaffold
   ```

### Q: Can't connect to database

**A:** Verify connection string and file permissions:

```bash
# Check file exists
ls -la /var/lib/dotnet-scaffold/scaffold.db

# Check permissions
chmod 600 /var/lib/dotnet-scaffold/scaffold.db
chown scaffold:scaffold /var/lib/dotnet-scaffold/scaffold.db

# Test connection
sqlite3 /var/lib/dotnet-scaffold/scaffold.db "SELECT 1;"
```

### Q: Rate limiting blocking legitimate requests

**A:** Increase rate limit:

```json
{
  "ApplicationSettings": {
    "RateLimitPerMinute": 120  // Default is 60
  }
}
```

Or whitelist IP:

```csharp
// In RateLimitingMiddleware
if (trustedIps.Contains(ipAddress))
{
    await next(context);
    return;
}
```

## Performance

### Q: How many services can I monitor?

**A:** Depends on:
- Hardware (CPU, RAM)
- Health check interval
- Database size

Typical setup:
- 100+ services - fine
- 1000+ services - optimize health check interval
- 10000+ services - consider PostgreSQL

### Q: How do I optimize health checks?

**A:**
1. Increase interval
   ```json
   { "HealthCheckInterval": 300 }  // 5 minutes
   ```

2. Increase batch size
   ```json
   { "MaxConcurrentHealthChecks": 20 }
   ```

3. Reduce result retention
   ```json
   { "HealthCheckResultRetentionDays": 7 }
   ```

### Q: Should I use Docker?

**A:** Consider Docker if:
- Multiple environments (dev/staging/prod)
- Kubernetes deployment
- Containerized infrastructure
- Scaling needs

Use traditional systemd if:
- Single server
- Simple deployment
- Legacy infrastructure

## Security

### Q: How do I secure API keys?

**A:** Best practices:

1. Store in environment variables
   ```bash
   export SCAFFOLD_API_KEY="sk_live_..."
   ```

2. Use secrets management
   ```bash
   # With HashiCorp Vault
   vault kv put secret/scaffold api_key="..."
   ```

3. Rotate regularly
   - Create new key
   - Update clients
   - Revoke old key

4. Use IP whitelisting
5. Set expiration dates

### Q: Can I use HTTPS?

**A:** Yes, recommended for production via Caddy reverse proxy:

```caddy
scaffold.example.com {
    reverse_proxy localhost:5000
    # Automatic HTTPS with Let's Encrypt
}
```

### Q: How do I prevent unauthorized access?

**A:**
1. Keep software updated
2. Use strong passwords
3. Enable audit logging
4. Monitor failed login attempts
5. Use API key IP whitelisting
6. Enable account lockout

### Q: Can I audit who accessed what?

**A:** Yes, audit logs track all actions:

```bash
curl "http://localhost:5000/api/auditlog?userId=user-123" \
  -H "X-API-Key: key" | jq '.data[] | {timestamp, action, userId}'
```

## Development

### Q: How do I extend the application?

**A:** See [Architecture Guide](architecture.md#extending-the-architecture).

Basic steps:
1. Define domain model
2. Create repository
3. Create service
4. Create controller
5. Register in Program.cs

### Q: Can I use this as a template?

**A:** Yes! Fork/clone and customize:

1. Change namespace
2. Rename domain models
3. Add custom services
4. Extend API endpoints
5. Customize styling/UI

### Q: How do I run tests?

**A:**
```bash
dotnet test
```

Current project includes example services. Write tests for custom code.

## Deployment

### Q: What's the best deployment method?

**A:** Depends on your infrastructure:

| Scenario | Method |
|----------|--------|
| Single Linux server | Systemd |
| Kubernetes | K8s manifests |
| AWS | Elastic Beanstalk |
| Azure | App Service |
| Docker | Docker/docker-compose |

### Q: How do I deploy updates?

**A:** See [Deployment Guide - Updates](deployment.md#updates).

Summary:
1. Build release
2. Stop service
3. Backup current
4. Deploy new
5. Start service
6. Verify

### Q: How do I scale horizontally?

**A:** Run multiple instances:

1. Deploy to multiple servers
2. Use load balancer (Caddy/Nginx)
3. Share database (PostgreSQL)

## Support

### Q: Where can I get help?

**A:** Resources:

- **Documentation**: `/docs` directory
- **Examples**: `/examples` directory
- **GitHub Issues**: Report bugs and ask questions
- **Website**: https://sarmkadan.com
- **Email**: rutova2@gmail.com

### Q: Can I contribute?

**A:** Absolutely! Please:

1. Fork repository
2. Create feature branch
3. Make changes
4. Submit pull request

See README for guidelines.

### Q: Is there commercial support?

**A:** Contact via email: rutova2@gmail.com

Services available:
- Consulting
- Custom development
- Deployment assistance
- Training

---

**Still have questions?** Open an issue on GitHub or contact the author.
