#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetServiceScaffold.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for the service scaffold platform.
/// </summary>
public class ServiceScaffoldDbContext : DbContext
{
    public ServiceScaffoldDbContext(DbContextOptions<ServiceScaffoldDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<ServiceRegistration> ServiceRegistrations { get; set; } = null!;

    public DbSet<HealthCheckResult> HealthCheckResults { get; set; } = null!;

    public DbSet<ServiceMetric> ServiceMetrics { get; set; } = null!;

    public DbSet<ServiceEvent> ServiceEvents { get; set; } = null!;

    public DbSet<ApiKey> ApiKeys { get; set; } = null!;

    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public DbSet<ServiceConfiguration> ServiceConfigurations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }

    /// <summary>
    /// Seeds initial configuration data if the database is empty.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        await Database.MigrateAsync();
    }
}
