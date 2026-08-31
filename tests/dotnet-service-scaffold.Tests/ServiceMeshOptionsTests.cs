using System.Reflection;
using DotnetServiceScaffold.Infrastructure.ServiceMesh;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Tests;

public class ServiceMeshOptionsTests
{
    [Fact]
    public void Constructor_InitializesExpectedDefaults()
    {
        var options = new ServiceMeshOptions();

        Assert.Equal("http://localhost:15000", options.AdminEndpoint);
        Assert.Equal(5, options.ReadinessTimeoutSeconds);
        Assert.Equal("default", options.MeshName);
        Assert.True(options.Enabled);
    }

    [Fact]
    public void Properties_AcceptEmptyNullAndBoundaryValues()
    {
        var options = new ServiceMeshOptions
        {
            AdminEndpoint = null!,
            ReadinessTimeoutSeconds = 0,
            MeshName = string.Empty,
            Enabled = false
        };

        Assert.Null(options.AdminEndpoint);
        Assert.Equal(0, options.ReadinessTimeoutSeconds);
        Assert.Empty(options.MeshName);
        Assert.False(options.Enabled);
    }

    [Fact]
    public void AddServiceMeshIntegration_BindsOptionsAndConfiguresHttpClient()
    {
        var values = new Dictionary<string, string?>
        {
            ["ServiceMesh:AdminEndpoint"] = "https://mesh.example:15000",
            ["ServiceMesh:ReadinessTimeoutSeconds"] = int.MaxValue.ToString(),
            ["ServiceMesh:MeshName"] = "istio",
            ["ServiceMesh:Enabled"] = "false"
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        var services = new ServiceCollection();

        var result = services.AddServiceMeshIntegration(configuration);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ServiceMeshOptions>>().Value;
        var client = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(nameof(ISidecarProxyService));

        Assert.Same(services, result);
        Assert.Equal("https://mesh.example:15000", options.AdminEndpoint);
        Assert.Equal(int.MaxValue, options.ReadinessTimeoutSeconds);
        Assert.Equal("istio", options.MeshName);
        Assert.False(options.Enabled);
        Assert.Equal(new Uri("https://mesh.example:15000"), client.BaseAddress);
        Assert.Equal(TimeSpan.FromSeconds(10), client.Timeout);
    }

    [Fact]
    public void AddServiceMeshIntegration_UsesDefaultsForEmptyConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        services.AddServiceMeshIntegration(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.Equal(
            "http://localhost:15000/",
            provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(ISidecarProxyService)).BaseAddress!.AbsoluteUri);
    }

    [Fact]
    public void AddServiceMeshIntegration_ThrowsForNullInputs()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(() =>
            ServiceMeshExtensions.AddServiceMeshIntegration(null!, configuration));
        Assert.Throws<ArgumentNullException>(() =>
            services.AddServiceMeshIntegration(null!));
    }

    [Fact]
    public async Task UseServiceMeshHeaders_ReturnsApplicationAndRejectsNull()
    {
        var builder = WebApplication.CreateBuilder();
        await using var app = builder.Build();

        Assert.Same(app, app.UseServiceMeshHeaders());
        Assert.Throws<ArgumentNullException>(() =>
            ServiceMeshExtensions.UseServiceMeshHeaders(null!));
    }

    [Fact]
    public async Task Middleware_InvokeAsync_PropagatesKnownHeadersAndCallsNext()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();
        context.Request.Headers["x-request-id"] = "request-123";
        context.Request.Headers["x-b3-traceid"] = "trace-456";
        context.Request.Headers["unrelated"] = "ignored";

        await InvokeMiddlewareAsync(middleware, context);

        Assert.True(nextCalled);
        Assert.Equal("request-123", context.Items["mesh:x-request-id"]);
        Assert.Equal("trace-456", context.Items["mesh:x-b3-traceid"]);
        Assert.DoesNotContain("mesh:unrelated", context.Items.Keys);
    }

    [Fact]
    public async Task Middleware_HandlesNoHeadersAndRejectsNullDependenciesOrContext()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = new DefaultHttpContext();

        await InvokeMiddlewareAsync(middleware, context);

        Assert.True(nextCalled);
        Assert.Empty(context.Items);
        Assert.IsType<ArgumentNullException>(
            Assert.Throws<TargetInvocationException>(() => CreateMiddleware(null!)).InnerException);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            InvokeMiddlewareAsync(middleware, null!));
    }

    private static object CreateMiddleware(RequestDelegate next)
    {
        var type = GetMiddlewareType();
        var loggerType = typeof(NullLogger<>).MakeGenericType(type);
        var logger = loggerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null);
        return Activator.CreateInstance(type, next, logger!)!;
    }

    private static async Task InvokeMiddlewareAsync(object middleware, HttpContext context)
    {
        var task = (Task)GetMiddlewareType().GetMethod("InvokeAsync")!
            .Invoke(middleware, [context])!;
        await task;
    }

    private static Type GetMiddlewareType() =>
        typeof(ServiceMeshOptions).Assembly.GetType(
            "DotnetServiceScaffold.Infrastructure.ServiceMesh.ServiceMeshHeaderPropagationMiddleware",
            throwOnError: true)!;
}
