using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using DotnetServiceScaffold.Infrastructure.Extensions;
using DotnetServiceScaffold.Application.Services;
using DotnetServiceScaffold.Infrastructure.DockerCompose;
using DotnetServiceScaffold.Infrastructure.Integration;
using DotnetServiceScaffold.Infrastructure.Caching;
using DotnetServiceScaffold.Presentation.Middleware;

namespace DotnetServiceScaffold.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplicationServices_Null_Throws()
    {
        IServiceCollection? services = null;
        Assert.Throws<ArgumentNullException>(() => services.AddApplicationServices());
    }

    [Fact]
    public void AddApplicationServices_RegistersDomainEventPublisher()
    {
        var services = new ServiceCollection();
        services.AddApplicationServices();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IDomainEventPublisher>());
    }

    [Fact]
    public void AddApplicationServices_WithConfig_NullConfig_Throws()
    {
        var services = new ServiceCollection();
        IConfiguration? config = null;
        Assert.Throws<ArgumentNullException>(() => services.AddApplicationServices(config));
    }

    [Fact]
    public void AddApplicationServices_WithConfig_RegistersDockerGenerator()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddApplicationServices(configuration);

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IDockerComposeGenerator>());
    }

    [Fact]
    public void AddIntegrationServices_RegistersClients()
    {
        var services = new ServiceCollection();
        services.AddIntegrationServices();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IExternalApiClient>());
        Assert.NotNull(provider.GetService<IWebhookClient>());
        Assert.NotNull(provider.GetService<ICustomHttpClientFactory>());
    }

    [Fact]
    public void AddCachingServices_RegistersCache()
    {
        var services = new ServiceCollection();
        services.AddCachingServices();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ICacheService>());
    }

    [Fact]
    public void AddBackgroundServices_ReturnsCollection()
    {
        var services = new ServiceCollection();
        var result = services.AddBackgroundServices();
        Assert.Same(services, result);
    }

    [Fact]
    public void AddApiAuthentication_RegistersRateLimitOptions()
    {
        var services = new ServiceCollection();
        services.AddApiAuthentication();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<RateLimitOptions>());
    }

    [Fact]
    public void UseApplicationMiddleware_Null_Throws()
    {
        WebApplication? app = null;
        Assert.Throws<ArgumentNullException>(() => app.UseApplicationMiddleware());
    }

    [Fact]
    public void UseApplicationMiddleware_RegistersMiddleware()
    {
        var builder = WebApplication.CreateBuilder();
        // AddApiAuthentication is required because UseApplicationMiddleware calls UseAuthentication
        builder.Services.AddApiAuthentication();
        var app = builder.Build();

        var result = app.UseApplicationMiddleware();

        Assert.Same(app, result);
    }
}
