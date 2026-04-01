// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Infrastructure.Data;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using DotnetServiceScaffold.Infrastructure.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/scaffold-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Register Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                      "Data Source=scaffold.db";

builder.Services.AddDbContext<ServiceScaffoldDbContext>(options =>
    options.UseSqlite(connectionString));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IHealthCheckRepository, HealthCheckRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

// Register HTTP Client for health checks
builder.Services.AddHttpClient<IHealthCheckService, HealthCheckService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "DotnetServiceScaffold/1.0");
    });

// Add controllers and API support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
// SqliteHealthCheck verifies file accessibility and available disk space.
// AddDbContextCheck verifies that EF Core can reach the database.
var sqliteDbPath = connectionString
    .Split(';', StringSplitOptions.RemoveEmptyEntries)
    .Select(p => p.Trim())
    .FirstOrDefault(p => p.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
    ?.Substring("Data Source=".Length) ?? "scaffold.db";

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ServiceScaffoldDbContext>("database")
    .Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
        "sqlite-file",
        _ => new SqliteHealthCheck(sqliteDbPath),
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: ["db", "sqlite"]));

var app = builder.Build();

// Middleware configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Service Scaffold API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint with detailed JSON response
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            }),
            totalDurationMs = report.TotalDuration.TotalMilliseconds
        });
        await context.Response.WriteAsync(result);
    }
});

// Status endpoint
app.MapGet("/status", async (ServiceScaffoldDbContext context) =>
{
    try
    {
        await context.Database.ExecuteSqlRawAsync("SELECT 1");
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error while checking database status in /status endpoint.");
        return Results.StatusCode(503);
    }
})
.Produces(200)
.Produces(503)
.WithName("Status")
.WithDescription("Returns the current service status");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceScaffoldDbContext>();
    try
    {
        await dbContext.InitializeDatabaseAsync();
        Log.Information("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Failed to initialize database");
        throw;
    }
}

Log.Information("Starting DotnetServiceScaffold application");
await app.RunAsync();
