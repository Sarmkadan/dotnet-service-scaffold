#nullable enable
using System;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;

namespace DotnetServiceScaffold.Infrastructure.ServiceDiscovery
{
    /// <summary>
    /// Builder for creating <see cref="ServiceDiscoveryOptions"/> instances with a fluent interface.
    /// </summary>
    public class ServiceDiscoveryOptionsBuilder
    {
        private bool _enabled = true;
        private DiscoveryMode _mode = DiscoveryMode.Dns;
        private LoadBalancingStrategy _loadBalancing = LoadBalancingStrategy.RoundRobin;
        private TimeSpan _cacheTtl = TimeSpan.FromSeconds(30);
        private TimeSpan _refreshInterval = TimeSpan.FromSeconds(15);
        private TimeSpan _resolutionTimeout = TimeSpan.FromSeconds(5);
        private DnsDiscoveryOptions _dns = new();
        private RegistryDiscoveryOptions _registry = new();
        private SelfRegistrationOptions _selfRegistration = new();

        /// <summary>
        /// Sets whether the service discovery subsystem is active.
        /// </summary>
        /// <param name="enabled">Whether the service discovery subsystem is active.</param>
        /// <returns>This builder instance.</returns>
        public ServiceDiscoveryOptionsBuilder WithEnabled(bool enabled)
        {
            _enabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets the resolution strategy the discovery engine uses.
        /// </summary>
        /// <param name="mode">The resolution strategy the discovery engine uses.</param>
        /// <returns>This builder instance.</returns>
        public ServiceDiscoveryOptionsBuilder WithMode(DiscoveryMode mode)
        {
            _mode = mode;
            return this;
        }

        /// <summary>
        /// Sets the load-balancing algorithm applied when selecting from healthy instances.
        /// </summary>
        /// <param name="loadBalancing">The load-balancing algorithm applied when selecting from healthy instances.</param>
        /// <returns>This builder instance.</returns>
        public ServiceDiscoveryOptionsBuilder WithLoadBalancing(LoadBalancingStrategy loadBalancing)
        {
            _loadBalancing = loadBalancing;
            return this;
        }

        /// <summary>
        /// Sets how long resolved records are cached before the backend is re-queried.
        /// </summary>
        /// <param name="cacheTtl">How long resolved records are cached before the backend is re-queried.</returns>
        /// <returns>This builder instance.</returns>
        public ServiceDiscoveryOptionsBuilder WithCacheTtl(TimeSpan cacheTtl)
        {
            _cacheTtl = cacheTtl;
            return this;
        }

        /// <summary>
        /// Sets the background poll interval used when watching for instance changes.
        /// </summary>
        /// <param name="refreshInterval">The background poll interval used when watching for instance changes.</param>
        /// <returns>This builder instance.</returns>
        public ServiceDiscoveryOptionsBuilder WithRefreshInterval(TimeSpan refreshInterval)
        {
            _refreshInterval = refreshInterval;
            return this;
        }

        /// <summary>
        /// Sets the per-call timeout applied to individual resolution requests.
        /// </summary>
        /// <param name="resolutionTimeout">The per-call timeout applied to individual resolution requests.</param>
        /// <returns>This builder instance.</returns>
        public ServiceDiscoveryOptionsBuilder WithResolutionTimeout(TimeSpan resolutionTimeout)
        {
            _resolutionTimeout = resolutionTimeout;
            return this;
        }

        /// <summary>
        /// Sets DNS-specific resolution settings.
        /// </summary>
        /// <param name="dns">DNS-specific resolution settings.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="dns"/> is <see langword="null"/>.</exception>
        public ServiceDiscoveryOptionsBuilder WithDns(DnsDiscoveryOptions dns)
        {
            ArgumentNullException.ThrowIfNull(dns);
            _dns = dns;
            return this;
        }

        /// <summary>
        /// Sets HTTP registry discovery settings.
        /// </summary>
        /// <param name="registry">HTTP registry discovery settings.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="registry"/> is <see langword="null"/>.</exception>
        public ServiceDiscoveryOptionsBuilder WithRegistry(RegistryDiscoveryOptions registry)
        {
            ArgumentNullException.ThrowIfNull(registry);
            _registry = registry;
            return this;
        }

        /// <summary>
        /// Sets self-registration settings for the current service instance.
        /// </summary>
        /// <param name="selfRegistration">Self-registration settings for the current service instance.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="selfRegistration"/> is <see langword="null"/>.</exception>
        public ServiceDiscoveryOptionsBuilder WithSelfRegistration(SelfRegistrationOptions selfRegistration)
        {
            ArgumentNullException.ThrowIfNull(selfRegistration);
            _selfRegistration = selfRegistration;
            return this;
        }

        /// <summary>
        /// Sets the DNS search domain appended to bare service names.
        /// </summary>
        /// <param name="searchDomain">The DNS search domain appended to bare service names.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentException">If <paramref name="searchDomain"/> is null or empty.</exception>
        public ServiceDiscoveryOptionsBuilder WithSearchDomain(string searchDomain)
        {
            ArgumentException.ThrowIfNullOrEmpty(searchDomain);
            _dns.SearchDomain = searchDomain;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="ServiceDiscoveryOptions"/> instance with the configured values.
        /// </summary>
        /// <returns>A configured <see cref="ServiceDiscoveryOptions"/> instance.</returns>
        /// <exception cref="ArgumentException">If required properties are missing.</exception>
        public ServiceDiscoveryOptions Build()
        {
            if (_dns == null)
            {
                throw new ArgumentException("DNS settings must not be null.", nameof(_dns));
            }

            if (_registry == null)
            {
                throw new ArgumentException("Registry settings must not be null.", nameof(_registry));
            }

            if (_selfRegistration == null)
            {
                throw new ArgumentException("Self-registration settings must not be null.", nameof(_selfRegistration));
            }

            return new ServiceDiscoveryOptions
            {
                Enabled = _enabled,
                Mode = _mode,
                LoadBalancing = _loadBalancing,
                CacheTtl = _cacheTtl,
                RefreshInterval = _refreshInterval,
                ResolutionTimeout = _resolutionTimeout,
                Dns = _dns,
                Registry = _registry,
                SelfRegistration = _selfRegistration
            };
        }

        /// <summary>
        /// Creates a builder pre-filled with values from an existing <see cref="ServiceDiscoveryOptions"/> instance.
        /// </summary>
        /// <param name="template">The service discovery options to copy values from.</param>
        /// <returns>A builder initialized with the template's values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="template"/> is <see langword="null"/>.</exception>
        public static ServiceDiscoveryOptionsBuilder From(ServiceDiscoveryOptions template)
        {
            ArgumentNullException.ThrowIfNull(template);

            return new ServiceDiscoveryOptionsBuilder
            {
                _enabled = template.Enabled,
                _mode = template.Mode,
                _loadBalancing = template.LoadBalancing,
                _cacheTtl = template.CacheTtl,
                _refreshInterval = template.RefreshInterval,
                _resolutionTimeout = template.ResolutionTimeout,
                _dns = template.Dns ?? new DnsDiscoveryOptions(),
                _registry = template.Registry ?? new RegistryDiscoveryOptions(),
                _selfRegistration = template.SelfRegistration ?? new SelfRegistrationOptions()
            };
        }
    }
}