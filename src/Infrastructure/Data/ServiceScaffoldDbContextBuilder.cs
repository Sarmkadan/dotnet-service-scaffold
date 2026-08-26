#nullable enable
using System;
using DotnetServiceScaffold.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetServiceScaffold.Infrastructure.Data
{
    /// <summary>
    /// Builder for creating <see cref="ServiceScaffoldDbContext"/> instances with a fluent interface.
    /// </summary>
    public class ServiceScaffoldDbContextBuilder
    {
        private DbSet<User>? _users;
        private DbSet<ServiceRegistration>? _serviceRegistrations;
        private DbSet<HealthCheckResult>? _healthCheckResults;
        private DbSet<ServiceMetric>? _serviceMetrics;
        private DbSet<ServiceEvent>? _serviceEvents;
        private DbSet<ApiKey>? _apiKeys;
        private DbSet<AuditLog>? _auditLogs;
        private DbSet<ServiceConfiguration>? _serviceConfigurations;
        private DbSet<WebhookDeadLetter>? _webhookDeadLetters;
        private DbContextOptions<ServiceScaffoldDbContext>? _options;
        private ILogger<ServiceScaffoldDbContext>? _logger;

        /// <summary>
        /// Sets the Users DbSet.
        /// </summary>
        /// <param name="users">The Users DbSet.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="users"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithUsers(DbSet<User> users)
        {
            ArgumentNullException.ThrowIfNull(users);
            _users = users;
            return this;
        }

        /// <summary>
        /// Sets the ServiceRegistrations DbSet.
        /// </summary>
        /// <param name="serviceRegistrations">The ServiceRegistrations DbSet.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceRegistrations"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithServiceRegistrations(DbSet<ServiceRegistration> serviceRegistrations)
        {
            ArgumentNullException.ThrowIfNull(serviceRegistrations);
            _serviceRegistrations = serviceRegistrations;
            return this;
        }

        /// <summary>
        /// Sets the HealthCheckResults DbSet.
        /// </summary>
        /// <param name="healthCheckResults">The HealthCheckResults DbSet.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="healthCheckResults"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithHealthCheckResults(DbSet<HealthCheckResult> healthCheckResults)
        {
            ArgumentNullException.ThrowIfNull(healthCheckResults);
            _healthCheckResults = healthCheckResults;
            return this;
        }

        /// <summary>
        /// Sets the ServiceMetrics DbSet.
        /// </summary>
        /// <param name="serviceMetrics">The ServiceMetrics DbSet.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceMetrics"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithServiceMetrics(DbSet<ServiceMetric> serviceMetrics)
        {
            ArgumentNullException.ThrowIfNull(serviceMetrics);
            _serviceMetrics = serviceMetrics;
            return this;
        }

        /// <summary>
        /// Sets the ServiceEvents DbSet.
        /// </summary>
        /// <param name="serviceEvents">The ServiceEvents DbSet.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceEvents"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithServiceEvents(DbSet<ServiceEvent> serviceEvents)
        {
            ArgumentNullException.ThrowIfNull(serviceEvents);
            _serviceEvents = serviceEvents;
            return this;
        }

        /// <summary>
        /// Sets the ApiKeys DbSet.
        /// </summary>
        /// <param name="apiKeys">The ApiKeys DbSet.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="apiKeys"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithApiKeys(DbSet<ApiKey> apiKeys)
        {
            ArgumentNullException.ThrowIfNull(apiKeys);
            _apiKeys = apiKeys;
            return this;
        }

        /// <summary>
        /// Sets the AuditLogs DbSet.
        /// </summary>
        /// <param name="auditLogs">The AuditLogs DbSet.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="auditLogs"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithAuditLogs(DbSet<AuditLog> auditLogs)
        {
            ArgumentNullException.ThrowIfNull(auditLogs);
            _auditLogs = auditLogs;
            return this;
        }

        /// <summary>
        /// Sets the ServiceConfigurations DbSet.
        /// </summary>
        /// <param name="serviceConfigurations">The ServiceConfigurations DbSet.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceConfigurations"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithServiceConfigurations(DbSet<ServiceConfiguration> serviceConfigurations)
        {
            ArgumentNullException.ThrowIfNull(serviceConfigurations);
            _serviceConfigurations = serviceConfigurations;
            return this;
        }

        /// <summary>
        /// Sets the WebhookDeadLetters DbSet.
        /// </summary>
        /// <param name="webhookDeadLetters">The WebhookDeadLetters DbSet.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="webhookDeadLetters"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithWebhookDeadLetters(DbSet<WebhookDeadLetter> webhookDeadLetters)
        {
            ArgumentNullException.ThrowIfNull(webhookDeadLetters);
            _webhookDeadLetters = webhookDeadLetters;
            return this;
        }

        /// <summary>
        /// Sets the DbContextOptions.
        /// </summary>
        /// <param name="options">The DbContextOptions.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="options"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithOptions(DbContextOptions<ServiceScaffoldDbContext> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _options = options;
            return this;
        }

        /// <summary>
        /// Sets the ILogger.
        /// </summary>
        /// <param name="logger">The ILogger.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/> is <see langword="null"/>.</exception>
        public ServiceScaffoldDbContextBuilder WithLogger(ILogger<ServiceScaffoldDbContext> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="ServiceScaffoldDbContext"/> instance with the configured values.
        /// </summary>
        /// <returns>A configured <see cref="ServiceScaffoldDbContext"/> instance.</returns>
        /// <exception cref="ArgumentException">If required properties are missing.</exception>
        public ServiceScaffoldDbContext Build()
        {
            if (_options == null)
            {
                throw new ArgumentException("DbContextOptions must be set.", nameof(_options));
            }

            if (_logger == null)
            {
                throw new ArgumentException("ILogger must be set.", nameof(_logger));
            }

            var context = new ServiceScaffoldDbContext(_options, _logger)
            {
                Users = _users ?? throw new ArgumentException("Users DbSet must be set.", nameof(_users)),
                ServiceRegistrations = _serviceRegistrations ?? throw new ArgumentException("ServiceRegistrations DbSet must be set.", nameof(_serviceRegistrations)),
                HealthCheckResults = _healthCheckResults ?? throw new ArgumentException("HealthCheckResults DbSet must be set.", nameof(_healthCheckResults)),
                ServiceMetrics = _serviceMetrics ?? throw new ArgumentException("ServiceMetrics DbSet must be set.", nameof(_serviceMetrics)),
                ServiceEvents = _serviceEvents ?? throw new ArgumentException("ServiceEvents DbSet must be set.", nameof(_serviceEvents)),
                ApiKeys = _apiKeys ?? throw new ArgumentException("ApiKeys DbSet must be set.", nameof(_apiKeys)),
                AuditLogs = _auditLogs ?? throw new ArgumentException("AuditLogs DbSet must be set.", nameof(_auditLogs)),
                ServiceConfigurations = _serviceConfigurations ?? throw new ArgumentException("ServiceConfigurations DbSet must be set.", nameof(_serviceConfigurations)),
                WebhookDeadLetters = _webhookDeadLetters ?? throw new ArgumentException("WebhookDeadLetters DbSet must be set.", nameof(_webhookDeadLetters))
            };

            return context;
        }

        /// <summary>
        /// Creates a builder pre-filled with values from an existing <see cref="ServiceScaffoldDbContext"/> instance.
        /// </summary>
        /// <param name="template">The service scaffold DbContext to copy values from.</param>
        /// <returns>A builder initialized with the template's values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="template"/> is <see langword="null"/>.</exception>
        public static ServiceScaffoldDbContextBuilder From(ServiceScaffoldDbContext template)
        {
            ArgumentNullException.ThrowIfNull(template);

            // The template's DbContextOptions and ILogger are private to the instance and cannot be
            // copied, so start with unconfigured defaults that the caller can override via WithOptions/WithLogger.
            var options = new DbContextOptionsBuilder<ServiceScaffoldDbContext>().Options;
            var logger = NullLogger<ServiceScaffoldDbContext>.Instance;

            return new ServiceScaffoldDbContextBuilder
            {
                _options = options,
                _logger = logger,
                _users = template.Users,
                _serviceRegistrations = template.ServiceRegistrations,
                _healthCheckResults = template.HealthCheckResults,
                _serviceMetrics = template.ServiceMetrics,
                _serviceEvents = template.ServiceEvents,
                _apiKeys = template.ApiKeys,
                _auditLogs = template.AuditLogs,
                _serviceConfigurations = template.ServiceConfigurations,
                _webhookDeadLetters = template.WebhookDeadLetters
            };
        }
    }
}