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
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using DotnetServiceScaffold.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Configure DotnetServiceScaffoldOptions with validation
builder.Services.AddOptions<DotnetServiceScaffoldOptions>()
.Bind(builder.Configuration.GetSection(ProgramConstants.ApplicationSettingsSection))
.ValidateOnStart();

// Startup validation for service discovery options (endpoints, ports, timeouts).
builder.Services.AddSingleton<IValidateOptions<ServiceDiscoveryOptions>, ServiceDiscoveryOptionsValidator>();

var structuredLoggingOptions = builder.Configuration
.GetSection(ProgramConstants.StructuredLoggingSection)
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
    outputTemplate: ProgramConstants.LogOutputTemplate)
.WriteTo.File(
    ProgramConstants.LogFilePath,
    rollingInterval: RollingInterval.Day,
    outputTemplate: ProgramConstants.LogOutputTemplate)
.CreateLogger();

builder.Host.UseSerilog();

// Register Database
var connectionString = builder.Configuration.GetConnectionString(ProgramConstants.DefaultConnectionName) ??
    ProgramConstants.DefaultConnectionString;

builder.Services.AddDbContext<ServiceScaffoldDbContext>((provider, options) =>
{
    // Add busy_timeout to handle concurrent SQLite writes and prevent SQLITE_BUSY errors
    // busy_timeout=5000 means SQLite will wait up to 5 seconds for a lock to be released
    var sqliteConnectionString = $"{connectionString};busy_timeout={ProgramConstants.SqliteBusyTimeoutMilliseconds}";
    options.UseSqlite(sqliteConnectionString);
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
    client.Timeout = TimeSpan.FromSeconds(ProgramConstants.HealthCheckTimeoutSeconds);
    client.DefaultRequestHeaders.Add(ProgramConstants.UserAgentHeaderName, ProgramConstants.UserAgentHeaderValue);
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
.FirstOrDefault(p => p.StartsWith(ProgramConstants.DataSourcePrefix, StringComparison.OrdinalIgnoreCase))
?.Substring(ProgramConstants.DataSourcePrefix.Length) ?? ProgramConstants.DefaultDatabaseFileName;

builder.Services.AddHealthChecks()
.AddDbContextCheck<ServiceScaffoldDbContext>(ProgramConstants.DatabaseHealthCheckName)
.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
    ProgramConstants.SqliteFileHealthCheckName,
    _ => new SqliteHealthCheck(sqliteDbPath),
    failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
    tags: [ProgramConstants.DatabaseTag, ProgramConstants.SqliteTag, ProgramConstants.LiveTag]))
.AddCheck<MemoryHealthCheck>(ProgramConstants.MemoryHealthCheckName, tags: [ProgramConstants.SystemTag, ProgramConstants.LiveTag]);

var app = builder.Build();

// Middleware configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(ProgramConstants.SwaggerDocumentUrl, ProgramConstants.SwaggerDocumentName);
    });
}

if (structuredLoggingOptions.EnableCorrelationId)
{
    app.UseCorrelationId();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.MapControllers();

// Health check endpoint with detailed JSON response
app.MapHealthChecks(ProgramConstants.HealthRoute, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = ProgramConstants.JsonContentType;
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
app.MapGet(ProgramConstants.StatusRoute, async (ServiceScaffoldDbContext context) =>
{
    try
    {
        await context.Database.ExecuteSqlRawAsync(ProgramConstants.DatabaseProbeSql);
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = ProgramConstants.ServiceVersion
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error while checking database status in /status endpoint.");
        return Results.StatusCode(ProgramConstants.ServiceUnavailableStatusCode);
    }
})
.Produces(ProgramConstants.OkStatusCode)
.Produces(ProgramConstants.ServiceUnavailableStatusCode)
.WithName(ProgramConstants.StatusEndpointName)
.WithDescription("Returns the current service status");

// Metrics endpoint with configurable protection
app.MapGet(ProgramConstants.MetricsRoute, async (
    HttpContext context,
    IMetricsService metricsService,
    IPrometheusFormatter prometheusFormatter,
    IOptions<DotnetServiceScaffoldOptions> options) =>
{
    var config = options.Value;

    try
    {
        // Check if metrics endpoint should be protected
        if (config.MetricsProtectionMode.Equals(ProgramConstants.DisabledProtectionMode, StringComparison.OrdinalIgnoreCase))
        {
            // Metrics endpoint is publicly accessible (INSECURE - not recommended for production)
            var metrics = await metricsService.GetMetricsAsync();
            var text = prometheusFormatter.Format(metrics, ProgramConstants.MetricsPrefix);
            return Results.Content(text, ProgramConstants.PrometheusContentType);
        }
        else if (config.MetricsProtectionMode.Equals(ProgramConstants.LocalhostOnlyProtectionMode, StringComparison.OrdinalIgnoreCase))
        {
            // Metrics endpoint only accessible from localhost
            var remoteIpAddress = context.Connection.RemoteIpAddress;
            if (remoteIpAddress == null || !IPAddress.IsLoopback(remoteIpAddress))
            {
                context.Response.StatusCode = ProgramConstants.ForbiddenStatusCode;
                context.Response.ContentType = ProgramConstants.JsonContentType;
                await context.Response.WriteAsJsonAsync(new { error = "Forbidden", message = "Metrics endpoint is restricted to localhost access only." });
                return Results.Empty;
            }

            var metrics = await metricsService.GetMetricsAsync();
            var text = prometheusFormatter.Format(metrics, ProgramConstants.MetricsPrefix);
            return Results.Content(text, ProgramConstants.PrometheusContentType);
        }
        else
        {
            // Default: ApiKey authentication required (recommended)
            // Check for API key in header
            if (!context.Request.Headers.TryGetValue(ProgramConstants.ApiKeyHeaderName, out var apiKeyHeaderValues) ||
                string.IsNullOrWhiteSpace(apiKeyHeaderValues.FirstOrDefault()))
            {
                context.Response.StatusCode = ProgramConstants.UnauthorizedStatusCode;
                context.Response.ContentType = ProgramConstants.JsonContentType;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized", message = "API key is required for metrics endpoint. Provide it in the X-Api-Key header." });
                return Results.Empty;
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            var validMetricsKey = config.MetricsApiKey;

            if (string.IsNullOrWhiteSpace(validMetricsKey) || !providedApiKey.Equals(validMetricsKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = ProgramConstants.ForbiddenStatusCode;
                context.Response.ContentType = ProgramConstants.JsonContentType;
                await context.Response.WriteAsJsonAsync(new { error = "Forbidden", message = "Invalid metrics API key." });
                return Results.Empty;
            }

            var metrics = await metricsService.GetMetricsAsync();
            var text = prometheusFormatter.Format(metrics, ProgramConstants.MetricsPrefix);
            return Results.Content(text, ProgramConstants.PrometheusContentType);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error generating metrics");
        context.Response.StatusCode = ProgramConstants.InternalServerErrorStatusCode;
        context.Response.ContentType = ProgramConstants.JsonContentType;
        await context.Response.WriteAsJsonAsync(new { error = "Internal Server Error", message = "Failed to generate metrics." });
        return Results.Empty;
    }
})
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

file static class ProgramConstants
{
    public const string ApplicationSettingsSection = "ApplicationSettings";
    public const string StructuredLoggingSection = "StructuredLogging";
    public const string LogOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
    public const string LogFilePath = "logs/scaffold-.txt";
    public const string DefaultConnectionName = "DefaultConnection";
    public const string DefaultConnectionString = "Data Source=scaffold.db";
    public const string DefaultDatabaseFileName = "scaffold.db";
    public const string DataSourcePrefix = "Data Source=";
    public const int SqliteBusyTimeoutMilliseconds = 5000;
    public const int HealthCheckTimeoutSeconds = 30;
    public const string UserAgentHeaderName = "User-Agent";
    public const string UserAgentHeaderValue = "DotnetServiceScaffold/1.0";
    public const string DatabaseHealthCheckName = "database";
    public const string SqliteFileHealthCheckName = "sqlite-file";
    public const string MemoryHealthCheckName = "memory";
    public const string DatabaseTag = "db";
    public const string SqliteTag = "sqlite";
    public const string LiveTag = "live";
    public const string SystemTag = "system";
    public const string SwaggerDocumentUrl = "/swagger/v1/swagger.json";
    public const string SwaggerDocumentName = "Service Scaffold API V1";
    public const string HealthRoute = "/health";
    public const string StatusRoute = "/status";
    public const string MetricsRoute = "/metrics";
    public const string JsonContentType = "application/json";
    public const string PrometheusContentType = "text/plain; version=0.0.4; charset=utf-8";
    public const string DatabaseProbeSql = "SELECT 1";
    public const string ServiceVersion = "1.0.0";
    public const string StatusEndpointName = "Status";
    public const string DisabledProtectionMode = "Disabled";
    public const string LocalhostOnlyProtectionMode = "LocalhostOnly";
    public const string MetricsPrefix = "scaffold";
    public const string ApiKeyHeaderName = "X-Api-Key";
    public const int OkStatusCode = 200;
    public const int UnauthorizedStatusCode = 401;
    public const int ForbiddenStatusCode = 403;
    public const int InternalServerErrorStatusCode = 500;
    public const int ServiceUnavailableStatusCode = 503;
}
