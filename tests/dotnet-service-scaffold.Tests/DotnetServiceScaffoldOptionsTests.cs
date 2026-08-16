using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DotnetServiceScaffold.Shared.Configuration;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class DotnetServiceScaffoldOptionsTests
{
    [Fact]
    public void Validate_ReturnsTrue_WithDefaultValues()
    {
        var options = new DotnetServiceScaffoldOptions
        {
            // JwtSecret is required and must be at least 32 characters
            JwtSecret = new string('a', 32)
        };

        Assert.True(options.Validate());
    }

    [Fact]
    public void Validate_ReturnsTrue_WithCustomValidValues()
    {
        var options = new DotnetServiceScaffoldOptions
        {
            HealthCheckInterval = 300,
            HealthCheckTimeout = 120,
            MaxConcurrentHealthChecks = 20,
            MaintenanceMode = true,
            AuditLogRetentionDays = 365,
            HealthCheckResultRetentionDays = 180,
            MaxFailedLoginAttempts = 10,
            AccountLockoutDurationMinutes = 60,
            PasswordMinimumLength = 12,
            EnableCors = true,
            AllowedOrigins = new List<string> { "https://example.com" },
            RateLimitPerMinute = 500,
            MaxServiceRegistrations = 500,
            MaxResponseSize = 5_000_000,
            EnableDetailedErrors = false,
            DefaultPageSize = 100,
            MaxPageSize = 500,
            CacheDurationSeconds = 600,
            EnableRequestLogging = false,
            MaxCollectionSize = 5000,
            ApiKeyPrefix = "mykey_",
            ApiKeyLength = 48,
            JwtTokenExpirationMinutes = 120,
            JwtSecret = new string('b', 64),
            DatabaseMigrationStrategy = "Manual",
            EnableDatabaseBackup = true,
            BackupDirectory = "/data/backups",
            MetricsProtectionMode = "ApiKey",
            MetricsApiKey = "metrics_key_123"
        };

        Assert.True(options.Validate());
    }

    [Theory]
    [InlineData(4)]      // below minimum
    [InlineData(3601)]   // above maximum
    public void Validate_Throws_WhenHealthCheckIntervalOutOfRange(int interval)
    {
        var options = new DotnetServiceScaffoldOptions
        {
            HealthCheckInterval = interval,
            JwtSecret = new string('c', 32)
        };

        Assert.Throws<ValidationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenJwtSecretTooShort()
    {
        var options = new DotnetServiceScaffoldOptions
        {
            JwtSecret = "short"
        };

        Assert.Throws<ValidationException>(() => options.Validate());
    }

    [Theory]
    [InlineData("invalidPrefix")]   // missing trailing underscore
    [InlineData("bad!_")]           // illegal character
    public void Validate_Throws_WhenApiKeyPrefixInvalid(string prefix)
    {
        var options = new DotnetServiceScaffoldOptions
        {
            ApiKeyPrefix = prefix,
            JwtSecret = new string('d', 32)
        };

        Assert.Throws<ValidationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenMetricsApiKeyInvalid()
    {
        var options = new DotnetServiceScaffoldOptions
        {
            MetricsApiKey = "invalid key!", // space and exclamation not allowed
            JwtSecret = new string('e', 32)
        };

        Assert.Throws<ValidationException>(() => options.Validate());
    }
}
