#nullable enable
using System;

namespace DotnetServiceScaffold.Shared.Configuration;

/// <summary>
/// Fluent builder for <see cref="DotnetServiceScaffoldOptions"/>.
/// </summary>
public sealed class DotnetServiceScaffoldOptionsBuilder
{
    private int _healthCheckInterval = 60;
    private int _healthCheckTimeout = 10;
    private int _maxConcurrentHealthChecks = 5;
    private bool _maintenanceMode;
    private int _auditLogRetentionDays = 90;
    private int _healthCheckResultRetentionDays = 30;
    private int _maxFailedLoginAttempts = 5;
    private int _accountLockoutDurationMinutes = 30;
    private int _passwordMinimumLength = 8;
    private bool _enableCors;

    /// <summary>
    /// Initializes a new instance of the <see cref="DotnetServiceScaffoldOptionsBuilder"/> class with default values.
    /// </summary>
    public DotnetServiceScaffoldOptionsBuilder()
    {
    }

    /// <summary>
    /// Sets the health check interval in seconds.
    /// </summary>
    /// <param name="value">The health check interval in seconds (between 5 and 3600).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 5 or greater than 3600.</exception>
    public DotnetServiceScaffoldOptionsBuilder WithHealthCheckInterval(int value)
    {
        if (value < 5 || value > 3600)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "HealthCheckInterval must be between 5 and 3600 seconds");
        }

        _healthCheckInterval = value;
        return this;
    }

    /// <summary>
    /// Sets the health check timeout in seconds.
    /// </summary>
    /// <param name="value">The health check timeout in seconds (between 1 and 300).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 1 or greater than 300.</exception>
    public DotnetServiceScaffoldOptionsBuilder WithHealthCheckTimeout(int value)
    {
        if (value < 1 || value > 300)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "HealthCheckTimeout must be between 1 and 300 seconds");
        }

        _healthCheckTimeout = value;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of concurrent health checks.
    /// </summary>
    /// <param name="value">The maximum number of concurrent health checks (between 1 and 100).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 1 or greater than 100.</exception>
    public DotnetServiceScaffoldOptionsBuilder WithMaxConcurrentHealthChecks(int value)
    {
        if (value < 1 || value > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "MaxConcurrentHealthChecks must be between 1 and 100");
        }

        _maxConcurrentHealthChecks = value;
        return this;
    }

    /// <summary>
    /// Sets whether maintenance mode is enabled.
    /// </summary>
    /// <param name="value">True to enable maintenance mode; otherwise, false.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public DotnetServiceScaffoldOptionsBuilder WithMaintenanceMode(bool value)
    {
        _maintenanceMode = value;
        return this;
    }

    /// <summary>
    /// Sets the number of days to retain audit logs.
    /// </summary>
    /// <param name="value">The number of days to retain audit logs (between 1 and 3650).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 1 or greater than 3650.</exception>
    public DotnetServiceScaffoldOptionsBuilder WithAuditLogRetentionDays(int value)
    {
        if (value < 1 || value > 3650)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "AuditLogRetentionDays must be between 1 and 3650 days");
        }

        _auditLogRetentionDays = value;
        return this;
    }

    /// <summary>
    /// Sets the number of days to retain health check results.
    /// </summary>
    /// <param name="value">The number of days to retain health check results (between 1 and 365).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 1 or greater than 365.</exception>
    public DotnetServiceScaffoldOptionsBuilder WithHealthCheckResultRetentionDays(int value)
    {
        if (value < 1 || value > 365)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "HealthCheckResultRetentionDays must be between 1 and 365 days");
        }

        _healthCheckResultRetentionDays = value;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of failed login attempts before account lockout.
    /// </summary>
    /// <param name="value">The maximum number of failed login attempts (between 1 and 20).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 1 or greater than 20.</exception>
    public DotnetServiceScaffoldOptionsBuilder WithMaxFailedLoginAttempts(int value)
    {
        if (value < 1 || value > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "MaxFailedLoginAttempts must be between 1 and 20");
        }

        _maxFailedLoginAttempts = value;
        return this;
    }

    /// <summary>
    /// Sets the duration of account lockout in minutes.
    /// </summary>
    /// <param name="value">The duration of account lockout in minutes (between 1 and 1440).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 1 or greater than 1440.</exception>
    public DotnetServiceScaffoldOptionsBuilder WithAccountLockoutDurationMinutes(int value)
    {
        if (value < 1 || value > 1440)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "AccountLockoutDurationMinutes must be between 1 and 1440 minutes");
        }

        _accountLockoutDurationMinutes = value;
        return this;
    }

    /// <summary>
    /// Sets the minimum password length requirement.
    /// </summary>
    /// <param name="value">The minimum password length (between 4 and 128).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than 4 or greater than 128.</exception>
    public DotnetServiceScaffoldOptionsBuilder WithPasswordMinimumLength(int value)
    {
        if (value < 4 || value > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "PasswordMinimumLength must be between 4 and 128 characters");
        }

        _passwordMinimumLength = value;
        return this;
    }

    /// <summary>
    /// Sets whether CORS is enabled for cross-origin requests.
    /// </summary>
    /// <param name="value">True to enable CORS; otherwise, false.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public DotnetServiceScaffoldOptionsBuilder WithEnableCors(bool value)
    {
        _enableCors = value;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="DotnetServiceScaffoldOptionsBuilder"/> pre-filled with values from an existing <see cref="DotnetServiceScaffoldOptions"/> instance.
    /// </summary>
    /// <param name="template">The template options instance to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static DotnetServiceScaffoldOptionsBuilder From(DotnetServiceScaffoldOptions template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return new DotnetServiceScaffoldOptionsBuilder
        {
            _healthCheckInterval = template.HealthCheckInterval,
            _healthCheckTimeout = template.HealthCheckTimeout,
            _maxConcurrentHealthChecks = template.MaxConcurrentHealthChecks,
            _maintenanceMode = template.MaintenanceMode,
            _auditLogRetentionDays = template.AuditLogRetentionDays,
            _healthCheckResultRetentionDays = template.HealthCheckResultRetentionDays,
            _maxFailedLoginAttempts = template.MaxFailedLoginAttempts,
            _accountLockoutDurationMinutes = template.AccountLockoutDurationMinutes,
            _passwordMinimumLength = template.PasswordMinimumLength,
            _enableCors = template.EnableCors
        };
    }

    /// <summary>
    /// Builds a new <see cref="DotnetServiceScaffoldOptions"/> instance with the configured values.
    /// </summary>
    /// <returns>A new <see cref="DotnetServiceScaffoldOptions"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the built options fail validation.</exception>
    public DotnetServiceScaffoldOptions Build()
    {
        var options = new DotnetServiceScaffoldOptions
        {
            HealthCheckInterval = _healthCheckInterval,
            HealthCheckTimeout = _healthCheckTimeout,
            MaxConcurrentHealthChecks = _maxConcurrentHealthChecks,
            MaintenanceMode = _maintenanceMode,
            AuditLogRetentionDays = _auditLogRetentionDays,
            HealthCheckResultRetentionDays = _healthCheckResultRetentionDays,
            MaxFailedLoginAttempts = _maxFailedLoginAttempts,
            AccountLockoutDurationMinutes = _accountLockoutDurationMinutes,
            PasswordMinimumLength = _passwordMinimumLength,
            EnableCors = _enableCors
        };

        try
        {
            options.Validate();
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            throw new ArgumentException("Invalid configuration options: " + ex.Message, ex);
        }

        return options;
    }
}