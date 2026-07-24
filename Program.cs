// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Infrastructure.Data;
using DotnetServiceScaffold.Infrastructure.Data.Repository;
using DotnetServiceScaffold.Infrastructure.Extensions;
using DotnetServiceScaffold.Infrastructure.HealthChecks;
using DotnetServiceScaffold.Infrastructure.Logging;
using DotnetServiceScaffold.Infrastructure.Metrics;
using DotnetServiceScaffold.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure DotnetServiceScaffoldOptions with validation
builder.Services.AddOptions<DotnetServiceScaffoldOptions>()
    .Bind(builder.Configuration.GetSection("ApplicationSettings"))
    .ValidateOnStart();

var structuredLoggingOptions = builder.Configuration
    .GetSection("StructuredLogging")
    .Get<StructuredLoggingOptions>() ?? new StructuredLoggingOptions();

var minimumLevel = Enum.TryParse<LogEventLevel>(
    structuredLoggingOptions.MinimumLevel,
    ignoreCase: true,
    out var parsedMinimumLevel)
    ? parsedMinimumLevel
    : LogEventLevel.Information;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(minimumLevel)
    .EnrichFromOptions(structuredLoggingOptions)
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

builder.Services.AddDbContext<ServiceScaffoldDbContext>((provider, options) =>
{
	options.UseSqlite(connectionString);
});

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IHealthCheckRepository, HealthCheckRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
// IHealthCheckService is registered below via AddHttpClient<IHealthCheckService, HealthCheckService>,
// which supplies the HttpClient the implementation requires. Do not also register it as a plain
// scoped service: that descriptor cannot resolve HttpClient and breaks IEnumerable<IHealthCheckService>.
builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddSingleton<IPrometheusFormatter, PrometheusFormatter>();

// Register HTTP Client for health checks
builder.Services.AddHttpClient<IHealthCheckService, HealthCheckService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "DotnetServiceScaffold/1.0");
    });

// Add controllers and API support
builder.Services.AddControllers();
// Register the API key authentication scheme. Several controllers are decorated with
// [Authorize]; without a registered authentication scheme those endpoints throw
// InvalidOperationException ("No authenticationScheme was specified") at runtime.
builder.Services.AddApiAuthentication();
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
        tags: ["db", "sqlite", "live"]))
    .AddCheck<MemoryHealthCheck>("memory", tags: ["system", "live"]);

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

if (structuredLoggingOptions.EnableCorrelationId)
{
    app.UseCorrelationId();
}

app.UseHttpsRedirection();
app.UseAuthentication();
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

app.MapGet("/metrics", async (IMetricsService metricsService, IPrometheusFormatter prometheusFormatter) =>
{
    var metrics = await metricsService.GetMetricsAsync();
    var text = prometheusFormatter.Format(metrics, "scaffold");
    return Results.Content(text, "text/plain; version=0.0.4; charset=utf-8");
})
.AllowAnonymous()
.WithName("Prometheus Metrics")
.WithDescription("Exposes metrics in Prometheus text format");

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
