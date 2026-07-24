#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Infrastructure.Caching;
using DotnetServiceScaffold.Infrastructure.DockerCompose;
using DotnetServiceScaffold.Infrastructure.Formatting;
using DotnetServiceScaffold.Infrastructure.Http;
using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Infrastructure.Logging;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using DotnetServiceScaffold.Presentation.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register application services.
/// Centralizes dependency injection configuration for better maintainability.
/// </summary>
/// <remarks>
/// This class provides extension methods for configuring the application's service collection
/// with all infrastructure and application services in a centralized location.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services including repositories, services, and domain event infrastructure.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        return services;
    }

    /// <summary>
    /// Registers application services together with infrastructure features that rely on configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddApplicationServices();
        services.AddSingleton<IDockerComposeGenerator, DockerComposeGenerator>();
        services.AddStructuredLogging(configuration);

        return services;
    }

    /// <summary>
    /// Registers integration services for external API calls and webhooks.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<HttpClientOptions>()
            .BindConfiguration("HttpClient")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<HttpClientOptions>, HttpClientOptionsValidator>();

        services.AddOptions<ResilienceOptions>()
            .BindConfiguration("Resilience")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<ResilienceOptions>, ResilienceOptionsValidator>();
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<ResilienceOptions>>().Value;
            return new CircuitBreakerState(
                options.CircuitBreakerFailureThreshold,
                TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds));
        });
        services.AddTransient<ResilientHttpMessageHandler>();
            services.AddTransient<CorrelationIdDelegatingHandler>();

        services.AddHttpClient("external-api", (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
        })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            .AddHttpMessageHandler<ResilientHttpMessageHandler>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddHttpClient("webhook", (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(10); // Specific timeout for webhooks
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
        })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddHttpClient<IExternalApiClient, ExternalApiClient>(client =>
        {
            // Configure the typed client to use the "external-api" configuration
            // Or just configure it here directly
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "DotnetServiceScaffold/1.0");
        })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            .AddHttpMessageHandler<ResilientHttpMessageHandler>();

        services.AddHttpClient<IWebhookClient, WebhookClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "DotnetServiceScaffold/1.0");
        });

        services.AddSingleton<IResponseFormatterFactory, ResponseFormatterFactory>();

        return services;
    }

    /// <summary>
    /// Registers caching services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICacheService, InMemoryCacheService>();

        return services;
    }

    /// <summary>
    /// Registers service discovery infrastructure including providers and the orchestrating service.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">Application configuration for binding options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddServiceDiscovery(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ServiceDiscoveryOptions>(
            configuration.GetSection(ServiceDiscoveryOptions.SectionName));

        services.AddHttpClient(RegistryServiceDiscoveryProvider.HttpClientName, (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
            client.BaseAddress = new Uri(opts.Registry.AgentEndpoint);
            client.Timeout = opts.ResolutionTimeout + TimeSpan.FromSeconds(2);
            client.DefaultRequestHeaders.Add(
                "User-Agent",
                $"dotnet-service-scaffold/{typeof(ServiceDiscoveryProviderSelector).Assembly.GetName().Version}");

            if (!string.IsNullOrEmpty(opts.Registry.AclToken))
                client.DefaultRequestHeaders.Add("X-Consul-Token", opts.Registry.AclToken);
        });

        services.AddSingleton<DnsServiceDiscoveryProvider>();
        services.AddSingleton<RegistryServiceDiscoveryProvider>();
        services.AddSingleton<InMemoryServiceDiscoveryProvider>();
        services.AddSingleton<IServiceDiscoveryProviderSelector, ServiceDiscoveryProviderSelector>();
        services.AddSingleton<IServiceDiscoveryService, ServiceDiscoveryService>();
        services.AddSingleton<IHostedService, ServiceHeartbeatBackgroundService>();
        services.AddSingleton<IHostedService, ServiceStaleEvictionBackgroundService>();

        return services;
    }

    /// <summary>
    /// Registers background services for periodic tasks.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }

    /// <summary>
    /// Registers API key authentication and rate limiting middleware.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = ApiKeyAuthenticationOptions.DefaultScheme;
        })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.DefaultScheme,
                null);

        services.AddSingleton(_ => new RateLimitOptions
        {
            AnonymousRequestsPerMinute = 60,
            AuthenticatedRequestsPerMinute = 300
        });

        return services;
    }

    /// <summary>
    /// Registers all middleware components.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure middleware on.</param>
    /// <returns>The <see cref="WebApplication"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}