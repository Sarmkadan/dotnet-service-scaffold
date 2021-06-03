// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Infrastructure.Caching;
using DotnetServiceScaffold.Infrastructure.Formatting;
using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Presentation.Middleware;
using Microsoft.AspNetCore.Authentication;

namespace DotnetServiceScaffold.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register application services.
/// Centralizes dependency injection configuration for better maintainability.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services including repositories, services, and background tasks.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        return services;
    }

    /// <summary>
    /// Registers integration services for external API calls and webhooks.
    /// </summary>
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        // HTTP clients
        services.AddHttpClient<IExternalApiClient, ExternalApiClient>();
        services.AddHttpClient<IWebhookClient, WebhookClient>();

        // Factories
        services.AddScoped<ICustomHttpClientFactory, HttpClientFactory>(provider =>
            new HttpClientFactory(provider.GetRequiredService<System.Net.Http.IHttpClientFactory>(), provider.GetRequiredService<ILogger<HttpClientFactory>>())
        );

        services.AddSingleton<IResponseFormatterFactory, ResponseFormatterFactory>();

        return services;
    }

    /// <summary>
    /// Registers caching services.
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        // Use in-memory cache for single-node deployments
        services.AddSingleton<ICacheService, InMemoryCacheService>();

        return services;
    }

    /// <summary>
    /// Registers background services for periodic tasks.
    /// Note: Background services can be implemented following the pattern shown in the
    /// DomainEventPublisher and NotificationService classes.
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        // Background services would be registered here when implemented
        // Example: services.AddHostedService<YourBackgroundService>();

        return services;
    }

    /// <summary>
    /// Registers API key authentication and rate limiting middleware.
    /// </summary>
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.DefaultScheme, null);

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
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        // Error handling (should be first)
        app.UseMiddleware<ErrorHandlingMiddleware>();

        // Request logging
        app.UseMiddleware<RequestLoggingMiddleware>();

        // Rate limiting
        app.UseMiddleware<RateLimitingMiddleware>();

        // Authentication
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
