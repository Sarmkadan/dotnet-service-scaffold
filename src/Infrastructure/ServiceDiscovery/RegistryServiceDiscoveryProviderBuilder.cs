#nullable enable
using System;
using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery
{
    /// <summary>
    /// Builder for creating <see cref="RegistryServiceDiscoveryProvider"/> instances with a fluent interface.
    /// </summary>
    public class RegistryServiceDiscoveryProviderBuilder
    {
        private IHttpClientFactory? _httpFactory;
        private IOptions<ServiceDiscoveryOptions>? _options;
        private ILogger<RegistryServiceDiscoveryProvider>? _logger;

        /// <summary>
        /// Sets the HTTP client factory used for communicating with the service registry.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithHttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            ArgumentNullException.ThrowIfNull(httpClientFactory);
            _httpFactory = httpClientFactory;
            return this;
        }

        /// <summary>
        /// Sets the service discovery options configuration.
        /// </summary>
        /// <param name="options">The service discovery options.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithOptions(IOptions<ServiceDiscoveryOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _options = options;
            return this;
        }

        /// <summary>
        /// Sets the logger for the provider.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithLogger(ILogger<RegistryServiceDiscoveryProvider> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
            return this;
        }

        /// <summary>
        /// Sets the service name for self-registration.
        /// Corresponds to the 'Service' and 'Name' properties in service registration.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithServiceName(string serviceName)
        {
            ArgumentException.ThrowIfNullOrEmpty(serviceName);
            EnsureOptions();
            _options.Value.SelfRegistration.ServiceName = serviceName;
            return this;
        }

        /// <summary>
        /// Sets the host address for self-registration.
        /// Corresponds to the 'Address' property in service registration.
        /// </summary>
        /// <param name="address">The host address or IP address.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithAddress(string address)
        {
            ArgumentException.ThrowIfNullOrEmpty(address);
            EnsureOptions();
            _options.Value.SelfRegistration.AdvertiseHost = address;
            return this;
        }

        /// <summary>
        /// Sets the port for self-registration.
        /// Corresponds to the 'Port' property in service registration.
        /// </summary>
        /// <param name="port">The TCP port.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithPort(int port)
        {
            if (port <= 0 || port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
            }

            EnsureOptions();
            _options.Value.SelfRegistration.AdvertisePort = port;
            return this;
        }

        /// <summary>
        /// Sets the tags for self-registration.
        /// Corresponds to the 'Tags' property in service registration.
        /// </summary>
        /// <param name="tags">The tags to associate with the service instance.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithTags(params string[] tags)
        {
            ArgumentNullException.ThrowIfNull(tags);
            EnsureOptions();
            _options.Value.SelfRegistration.Tags = [.. tags];
            return this;
        }

        /// <summary>
        /// Sets the health check path for self-registration.
        /// Corresponds to the 'Checks' property in service registration (defines how health is checked).
        /// </summary>
        /// <param name="healthCheckPath">The HTTP path for health checks (e.g., "/health").</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithHealthCheckPath(string healthCheckPath)
        {
            ArgumentException.ThrowIfNullOrEmpty(healthCheckPath);
            EnsureOptions();
            _options.Value.SelfRegistration.HealthCheckPath = healthCheckPath;
            return this;
        }

        /// <summary>
        /// Sets the heartbeat interval for registry-registered services.
        /// </summary>
        /// <param name="heartbeatInterval">The interval at which to send TTL heartbeats.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithHeartbeatInterval(TimeSpan heartbeatInterval)
        {
            if (heartbeatInterval <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(heartbeatInterval), "Heartbeat interval must be positive.");
            }

            EnsureOptions();
            _options.Value.Registry.HeartbeatInterval = heartbeatInterval;
            return this;
        }

        /// <summary>
        /// Sets the target datacenter for the service registry.
        /// </summary>
        /// <param name="datacenter">The datacenter name, or null to use the agent default.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithDatacenter(string? datacenter)
        {
            EnsureOptions();
            _options.Value.Registry.Datacenter = datacenter;
            return this;
        }

        /// <summary>
        /// Sets whether to only return instances with passing health checks.
        /// </summary>
        /// <param name="onlyHealthy">True to only return healthy instances, false to include all instances.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithOnlyHealthyInstances(bool onlyHealthy)
        {
            EnsureOptions();
            _options.Value.Registry.OnlyHealthyInstances = onlyHealthy;
            return this;
        }

        /// <summary>
        /// Sets the ACL token for authenticating with the service registry.
        /// </summary>
        /// <param name="aclToken">The ACL token, or null for no authentication.</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithAclToken(string? aclToken)
        {
            EnsureOptions();
            _options.Value.Registry.AclToken = aclToken;
            return this;
        }

        /// <summary>
        /// Sets the base URL of the registry agent.
        /// </summary>
        /// <param name="agentEndpoint">The base URL of the registry agent (e.g., "http://localhost:8500").</param>
        /// <returns>This builder instance.</returns>
        public RegistryServiceDiscoveryProviderBuilder WithAgentEndpoint(string agentEndpoint)
        {
            ArgumentException.ThrowIfNullOrEmpty(agentEndpoint);
            EnsureOptions();
            _options.Value.Registry.AgentEndpoint = agentEndpoint;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="RegistryServiceDiscoveryProvider"/> instance with the configured dependencies and options.
        /// </summary>
        /// <returns>A configured <see cref="RegistryServiceDiscoveryProvider"/> instance.</returns>
        /// <exception cref="ArgumentException">If required dependencies are not set.</exception>
        public RegistryServiceDiscoveryProvider Build()
        {
            if (_httpFactory == null)
            {
                throw new ArgumentException("HTTP client factory must be set.", nameof(_httpFactory));
            }

            if (_options == null)
            {
                throw new ArgumentException("Service discovery options must be set.", nameof(_options));
            }

            if (_logger == null)
            {
                throw new ArgumentException("Logger must be set.", nameof(_logger));
            }

            return new RegistryServiceDiscoveryProvider(_httpFactory, _options, _logger);
        }

        /// <summary>
        /// Creates a builder pre-filled with dependencies and options from an existing <see cref="RegistryServiceDiscoveryProvider"/> instance.
        /// </summary>
        /// <param name="template">The provider to copy dependencies and options from.</param>
        /// <returns>A builder initialized with the template's values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="template"/> is null.</exception>
        public static RegistryServiceDiscoveryProviderBuilder From(RegistryServiceDiscoveryProvider template)
        {
            ArgumentNullException.ThrowIfNull(template);

            // Use reflection to access private fields since they're not exposed via properties
            var httpFactoryField = typeof(RegistryServiceDiscoveryProvider).GetField("_httpFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var optionsField = typeof(RegistryServiceDiscoveryProvider).GetField("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var loggerField = typeof(RegistryServiceDiscoveryProvider).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (httpFactoryField == null || optionsField == null || loggerField == null)
            {
                throw new InvalidOperationException("Failed to access private fields of RegistryServiceDiscoveryProvider.");
            }

            var builder = new RegistryServiceDiscoveryProviderBuilder();
            builder._httpFactory = (IHttpClientFactory)httpFactoryField.GetValue(template)!;
            builder._options = (IOptions<ServiceDiscoveryOptions>)optionsField.GetValue(template)!;
            builder._logger = (ILogger<RegistryServiceDiscoveryProvider>)loggerField.GetValue(template)!;

            return builder;
        }

        private void EnsureOptions()
        {
            if (_options == null)
            {
                _options = Options.Create(new ServiceDiscoveryOptions());
            }
        }
    }
}