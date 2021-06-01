# Getting Started with dotnet-service-scaffold

This guide walks you through setting up and running the dotnet-service-scaffold project from scratch.

## Prerequisites

### Required Software

- **.NET 10.0 SDK** - [Download from dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git** - [Download from git-scm.com](https://git-scm.com)

### Verify Installation

```bash
# Check .NET version
dotnet --version

# Check Git version
git --version
```

### Optional Tools

- **Visual Studio Code** - Lightweight code editor
- **Visual Studio 2024** - Full-featured IDE
- **Docker** - For containerized deployments
- **Postman** - For API testing

## Step 1: Clone the Repository

```bash
git clone https://github.com/sarmkadan/dotnet-service-scaffold.git
cd dotnet-service-scaffold
```

## Step 2: Restore NuGet Packages

```bash
dotnet restore
```

This downloads all dependencies defined in `dotnet-service-scaffold.csproj`.

## Step 3: Build the Project

```bash
dotnet build
```

For production, use Release configuration:

```bash
dotnet build -c Release
```

## Step 4: Run the Application

### Development Mode

```bash
dotnet run
```

Output should show:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### Production Mode

```bash
dotnet run -c Release
```

## Step 5: Verify the Application

### Check API Status

```bash
curl http://localhost:5000/health
```

Response:
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy"
  }
}
```

### Access Swagger UI

Open browser: `http://localhost:5000/swagger`

You'll see interactive API documentation.

### Check Service Status

```bash
curl http://localhost:5000/status
```

Response:
```json
{
  "status": "healthy",
  "timestamp": "2026-05-04T10:00:00Z",
  "version": "1.0.0"
}
```

## Step 6: Create Your First User

### Register User

```bash
curl -X POST http://localhost:5000/api/user/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "email": "admin@example.com",
    "password": "AdminPassword123!"
  }'
```

Response:
```json
{
  "success": true,
  "data": {
    "userId": "user-12345678",
    "username": "admin",
    "email": "admin@example.com"
  }
}
```

### Login

```bash
curl -X POST http://localhost:5000/api/user/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "AdminPassword123!"
  }'
```

Response:
```json
{
  "success": true,
  "data": {
    "userId": "user-12345678",
    "username": "admin",
    "email": "admin@example.com",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

## Step 7: Create Your First API Key

API Keys are used for service-to-service authentication.

```bash
curl -X POST http://localhost:5000/api/apikey/create \
  -H "Content-Type: application/json" \
  -H "X-API-Key: admin-api-key" \
  -d '{
    "name": "MyFirstKey",
    "description": "For testing",
    "ipWhitelist": ["127.0.0.1"],
    "scopes": ["service:read", "service:write"]
  }'
```

Response:
```json
{
  "success": true,
  "data": {
    "apiKey": "sk_live_abc123xyz789",
    "name": "MyFirstKey",
    "createdAt": "2026-05-04T10:00:00Z"
  }
}
```

**Save this API key - you won't see it again!**

## Step 8: Register a Service

Using the API key from step 7:

```bash
curl -X POST http://localhost:5000/api/service/register \
  -H "Content-Type: application/json" \
  -H "X-API-Key: sk_live_abc123xyz789" \
  -d '{
    "name": "MyService",
    "description": "My first service",
    "healthCheckUrl": "http://localhost:8080/health",
    "ownerId": "user-12345678",
    "isEnabled": true
  }'
```

Response:
```json
{
  "success": true,
  "data": {
    "id": "svc-abc123",
    "name": "MyService",
    "status": "Healthy",
    "successRate": 100.0,
    "isEnabled": true
  }
}
```

## Step 9: Run Your First Health Check

```bash
curl -X POST http://localhost:5000/api/healthcheck/svc-abc123/check \
  -H "X-API-Key: sk_live_abc123xyz789"
```

Response:
```json
{
  "success": true,
  "data": {
    "serviceId": "svc-abc123",
    "status": "Healthy",
    "responseTime": 125,
    "statusCode": 200,
    "checkedAt": "2026-05-04T10:00:30Z"
  }
}
```

## Database Location

The SQLite database is created automatically in your working directory:

```
./scaffold.db
```

To verify it was created:

```bash
ls -lh scaffold.db
```

To inspect the database:

```bash
sqlite3 scaffold.db
sqlite> .tables
sqlite> SELECT * FROM Users;
sqlite> .exit
```

## Common Issues & Troubleshooting

### Issue: Port 5000 already in use

```bash
# Change the port
dotnet run --launch-profile "https"
```

Or specify explicitly:

```bash
dotnet run -- --urls "http://localhost:5001"
```

### Issue: Database locked

Stop the application and remove the lock file:

```bash
rm scaffold.db-wal
rm scaffold.db-shm
```

### Issue: SSL/HTTPS errors on Windows

Run as administrator or disable HTTPS for development:

```bash
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### Issue: NuGet restore fails

Clear NuGet cache:

```bash
dotnet nuget locals all --clear
dotnet restore
```

## Next Steps

1. **Read the Architecture Guide** - Understand the project structure (`docs/architecture.md`)
2. **Explore API Reference** - Learn all available endpoints (`docs/api-reference.md`)
3. **Check Examples** - See practical code samples (`examples/` directory)
4. **Deploy to Production** - Follow deployment guide (`docs/deployment.md`)
5. **Review FAQ** - Common questions and answers (`docs/faq.md`)

## Development Workflow

### Making Code Changes

1. Edit files in `src/`
2. Save file - application auto-reloads with `dotnet watch`
3. Test changes via API calls or Swagger UI

### Running with Auto-Reload

```bash
dotnet watch run
```

This recompiles on file changes.

### Building for Distribution

```bash
dotnet build -c Release -o ./publish
```

Output in `publish/` directory ready for deployment.

## Directory Overview

| Directory | Purpose |
|-----------|---------|
| `src/` | Source code |
| `docs/` | Documentation |
| `examples/` | Example code and scripts |
| `logs/` | Log files (created at runtime) |
| `obj/` | Build artifacts |
| `bin/` | Compiled output |

## Environment Variables

Configure via environment or `appsettings.json`:

```bash
# Linux/Mac
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Data Source=/data/scaffold.db"

# Windows (PowerShell)
$env:ASPNETCORE_ENVIRONMENT="Production"
```

## Useful Commands

```bash
# Clean build artifacts
dotnet clean

# Run tests
dotnet test

# Format code
dotnet format

# Show available project info
dotnet project-info

# Run with specific port
dotnet run -- --urls "http://0.0.0.0:8080"

# Profile startup performance
dotnet run -- --enableStartupDiagnostics
```

## Installing as System Service (Linux)

See `docs/deployment.md` for complete systemd setup.

Quick version:

```bash
sudo cp dotnet-scaffold.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable dotnet-scaffold
sudo systemctl start dotnet-scaffold
```

## Summary

You now have:
- ✅ Cloned the repository
- ✅ Built and run the application
- ✅ Verified it's working
- ✅ Created a user account
- ✅ Generated an API key
- ✅ Registered a service
- ✅ Ran your first health check

**Congratulations!** You're ready to start using dotnet-service-scaffold.
