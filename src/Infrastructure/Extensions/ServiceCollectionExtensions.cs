#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Infrastructure.Caching;
using DotnetServiceScaffold.Infrastructure.DockerCompose;
using DotnetServiceScaffold.Infrastructure.Formatting;
using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Infrastructure.Logging;
using DotnetServiceScaffold.Presentation.Middleware;
using Microsoft.AspNetCore.Authentication;

namespace DotnetServiceScaffold.Infrastructure.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register application services.
/// Centralizes dependency injection configuration for better maintainability.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services including repositories, services, and background tasks.
    /// </summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        return services;
    }

    /// <summary>
    /// Registers application services together with infrastructure features that rely on configuration.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationServices();
        services.AddSingleton<IDockerComposeGenerator, DockerComposeGenerator>();
        services.AddStructuredLogging(configuration);

        return services;
    }

    /// <summary>
    /// Registers integration services for external API calls and webhooks.
    /// </summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        services.AddHttpClient<IExternalApiClient, ExternalApiClient>();
        services.AddHttpClient<IWebhookClient, WebhookClient>();

        services.AddScoped<ICustomHttpClientFactory, HttpClientFactory>(provider =>
            new HttpClientFactory(
                provider.GetRequiredService<System.Net.Http.IHttpClientFactory>(),
                provider.GetRequiredService<ILogger<HttpClientFactory>>()));

        services.AddSingleton<IResponseFormatterFactory, ResponseFormatterFactory>();

        return services;
    }

    /// <summary>
    /// Registers caching services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        services.AddSingleton<ICacheService, InMemoryCacheService>();

        return services;
    }

    /// <summary>
    /// Registers background services for periodic tasks.
    /// Note: Background services can be implemented following the pattern shown in the
    /// DomainEventPublisher and NotificationService classes.
    /// </summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Registers API key authentication and rate limiting middleware.
    /// </summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
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
    /// <param name="app">Web application.</param>
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
