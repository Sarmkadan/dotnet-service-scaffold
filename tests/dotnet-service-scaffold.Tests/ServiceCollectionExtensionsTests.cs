using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Infrastructure.Caching;
using DotnetServiceScaffold.Infrastructure.DockerCompose;
using DotnetServiceScaffold.Infrastructure.Extensions;
using DotnetServiceScaffold.Infrastructure.Formatting;
using DotnetServiceScaffold.Infrastructure.Http;
using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Presentation.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplicationServices_RegistersScopedApplicationServicesAndReturnsCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddApplicationServices();

        Assert.Same(services, result);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IDomainEventPublisher) &&
            descriptor.ImplementationType == typeof(DomainEventPublisher) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(INotificationService) &&
            descriptor.ImplementationType == typeof(NotificationService) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplicationServices_WithEmptyConfiguration_RegistersConfiguredServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var result = services.AddApplicationServices(configuration);

        Assert.Same(services, result);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IDockerComposeGenerator) &&
            descriptor.ImplementationType == typeof(DockerComposeGenerator) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDomainEventPublisher));
    }

    [Fact]
    public void AddIntegrationServices_RegistersClientsOptionsHandlersAndFormatterFactory()
    {
        var services = new ServiceCollection();

        var result = services.AddIntegrationServices();

        Assert.Same(services, result);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IExternalReadClient));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IExternalWriteClient));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IExternalApiClient));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IWebhookClient));
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IResponseFormatterFactory) &&
            descriptor.ImplementationType == typeof(ResponseFormatterFactory) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IValidateOptions<HttpClientOptions>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IValidateOptions<ResilienceOptions>));
    }

    [Fact]
    public void AddCachingServices_RegistersSingletonCacheAndReturnsCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddCachingServices();

        Assert.Same(services, result);
        var descriptor = Assert.Single(services, item => item.ServiceType == typeof(ICacheService));
        Assert.Equal(typeof(InMemoryCacheService), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddBackgroundServices_WithEmptyCollection_DoesNotAddRegistrations()
    {
        var services = new ServiceCollection();

        var result = services.AddBackgroundServices();

        Assert.Same(services, result);
        Assert.Empty(services);
    }

    [Fact]
    public void AddApiAuthentication_RegistersSchemeAndBoundaryRateLimits()
    {
        var services = new ServiceCollection();

        var result = services.AddApiAuthentication();
        using var provider = services.BuildServiceProvider();
        var authentication = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        var rateLimits = provider.GetRequiredService<RateLimitOptions>();

        Assert.Same(services, result);
        Assert.Equal(ApiKeyAuthenticationOptions.DefaultScheme, authentication.DefaultScheme);
        Assert.Equal(60, rateLimits.AnonymousRequestsPerMinute);
        Assert.Equal(300, rateLimits.AuthenticatedRequestsPerMinute);
    }

    [Fact]
    public void UseApplicationMiddleware_ReturnsSameApplication()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddApiAuthentication();
        builder.Services.AddAuthorization();
        var app = builder.Build();

        var result = app.UseApplicationMiddleware();

        Assert.Same(app, result);
    }

    [Fact]
    public void PublicExtensions_WithNullInputs_ThrowArgumentNullException()
    {
        IServiceCollection? services = null;
        IConfiguration? configuration = null;
        WebApplication? app = null;

        Assert.Throws<ArgumentNullException>(() => services!.AddApplicationServices());
        Assert.Throws<ArgumentNullException>(() => services!.AddApplicationServices(new ConfigurationBuilder().Build()));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddApplicationServices(configuration!));
        Assert.Throws<ArgumentNullException>(() => services!.AddIntegrationServices());
        Assert.Throws<ArgumentNullException>(() => services!.AddCachingServices());
        Assert.Throws<ArgumentNullException>(() => services!.AddBackgroundServices());
        Assert.Throws<ArgumentNullException>(() => services!.AddApiAuthentication());
        Assert.Throws<ArgumentNullException>(() => app!.UseApplicationMiddleware());
    }
}
