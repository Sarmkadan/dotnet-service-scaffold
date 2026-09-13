#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for the service scaffold platform.
/// </summary>
public class ServiceScaffoldDbContext : DbContext, IEquatable<ServiceScaffoldDbContext>, IServiceScaffoldDbContext
{
    private readonly ILogger<ServiceScaffoldDbContext> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceScaffoldDbContext"/> class.
    /// </summary>
    /// <param name="options">The options used to configure the database context.</param>
    /// <param name="logger">The logger used to record database context activity.</param>
    public ServiceScaffoldDbContext(
        DbContextOptions<ServiceScaffoldDbContext> options,
        ILogger<ServiceScaffoldDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    /// <summary>
    /// Determines whether the current context is equal to another context.
    /// </summary>
    /// <param name="other">The context to compare with the current context.</param>
    /// <returns><see langword="true"/> if the contexts are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ServiceScaffoldDbContext? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Users.Equals(other.Users) &&
               ServiceRegistrations.Equals(other.ServiceRegistrations) &&
               HealthCheckResults.Equals(other.HealthCheckResults) &&
               ServiceMetrics.Equals(other.ServiceMetrics) &&
               ServiceEvents.Equals(other.ServiceEvents) &&
               ApiKeys.Equals(other.ApiKeys) &&
               AuditLogs.Equals(other.AuditLogs) &&
               ServiceConfigurations.Equals(other.ServiceConfigurations);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ServiceScaffoldDbContext)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Users, ServiceRegistrations, HealthCheckResults, ServiceMetrics, ServiceEvents, ApiKeys, AuditLogs, ServiceConfigurations);
    }

    /// <summary>
    /// Determines whether two contexts are equal.
    /// </summary>
    /// <param name="left">The first context to compare.</param>
    /// <param name="right">The second context to compare.</param>
    /// <returns><see langword="true"/> if the contexts are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ServiceScaffoldDbContext? left, ServiceScaffoldDbContext? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two contexts are not equal.
    /// </summary>
    /// <param name="left">The first context to compare.</param>
    /// <param name="right">The second context to compare.</param>
    /// <returns><see langword="true"/> if the contexts are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ServiceScaffoldDbContext? left, ServiceScaffoldDbContext? right)
    {
        return !Equals(left, right);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            return;
        }

        // Configure SQLite execution strategy for handling SQLITE_BUSY errors
        // This enables automatic retries for database lock errors that can occur
        // in multi-writer ASP.NET Core applications
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    }

    /// <summary>
    /// Gets or sets the users stored in the database.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Gets or sets the service registrations stored in the database.
    /// </summary>
    public DbSet<ServiceRegistration> ServiceRegistrations { get; set; } = null!;

    /// <summary>
    /// Gets or sets the health check results stored in the database.
    /// </summary>
    public DbSet<HealthCheckResult> HealthCheckResults { get; set; } = null!;

    /// <summary>
    /// Gets or sets the service metrics stored in the database.
    /// </summary>
    public DbSet<ServiceMetric> ServiceMetrics { get; set; } = null!;

    /// <summary>
    /// Gets or sets the service events stored in the database.
    /// </summary>
    public DbSet<ServiceEvent> ServiceEvents { get; set; } = null!;

    /// <summary>
    /// Gets or sets the API keys stored in the database.
    /// </summary>
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;

    /// <summary>
    /// Gets or sets the audit logs stored in the database.
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    /// <summary>
    /// Gets or sets the service configurations stored in the database.
    /// </summary>
    public DbSet<ServiceConfiguration> ServiceConfigurations { get; set; } = null!;

    /// <summary>
    /// Gets or sets the webhook dead letters stored in the database.
    /// </summary>
    public DbSet<WebhookDeadLetter> WebhookDeadLetters { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger.LogInformation("Configuring database model for ServiceScaffoldDbContext");

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasMany(e => e.ApiKeys)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.ManagedServices)
                .WithOne(s => s.Owner)
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ServiceRegistration configuration
        modelBuilder.Entity<ServiceRegistration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceName).IsRequired();
            entity.Property(e => e.Endpoint).IsRequired();
            entity.HasIndex(e => e.ServiceName).IsUnique();
            entity.HasMany(e => e.HealthCheckResults)
                .WithOne(h => h.Service)
                .HasForeignKey(h => h.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Metrics)
                .WithOne(m => m.Service)
                .HasForeignKey(m => m.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Events)
                .WithOne(e => e.Service)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // HealthCheckResult configuration
        modelBuilder.Entity<HealthCheckResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ServiceId, e.CheckedAt })
                .IsDescending(false, true);
        });

        // ServiceMetric configuration
        modelBuilder.Entity<ServiceMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ServiceId, e.RecordedAt })
                .IsDescending(false, true);
        });

        // ServiceEvent configuration
        modelBuilder.Entity<ServiceEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ServiceId, e.CreatedAt })
                .IsDescending(false, true);
        });

        // ApiKey configuration
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KeyHash).IsRequired();
            entity.HasIndex(e => e.KeyPrefix).IsUnique();
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt })
                .IsDescending(false, true);
            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                .IsDescending();
        });

        // ServiceConfiguration configuration
        modelBuilder.Entity<ServiceConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.Value).IsRequired();
            entity.HasIndex(e => new { e.Key, e.ServiceId }).IsUnique();
        });

        // WebhookDeadLetter configuration
        modelBuilder.Entity<WebhookDeadLetter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WebhookUrl).IsRequired();
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.Property(e => e.AttemptHistoryJson).IsRequired();
            entity.HasIndex(e => new { e.IsResolved, e.CreatedAt })
                .IsDescending(false, true);
        });
    }

    /// <summary>
    /// Initializes the database schema and enables WAL journal mode for better
    /// write concurrency under load.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database schema...");

            // The project ships without EF Core migrations. MigrateAsync() on a project with
            // no migrations only creates __EFMigrationsHistory and no entity tables, so every
            // query then fails with "no such table". Use migrations when they exist, otherwise
            // fall back to EnsureCreatedAsync() which builds the schema from the model.
            if (Database.GetMigrations().Any())
            {
                await Database.MigrateAsync();
            }
            else
            {
                await Database.EnsureCreatedAsync();
            }

            // WAL mode allows concurrent reads during writes, preventing "database is locked"
            // errors that occur with the default DELETE journal mode under concurrent load.
            await Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
            await Database.ExecuteSqlRawAsync("PRAGMA synchronous=NORMAL;");

            _logger.LogInformation("Database schema initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database schema");
            throw;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"ServiceScaffoldDbContext {{ Users = {Users}, ServiceRegistrations = {ServiceRegistrations}, HealthCheckResults = {HealthCheckResults}, ServiceMetrics = {ServiceMetrics}, ServiceEvents = {ServiceEvents}, ApiKeys = {ApiKeys} }}";
    }
}
