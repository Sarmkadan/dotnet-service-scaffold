using DotnetServiceScaffold.Infrastructure.Extensions;
using DotnetServiceScaffold.Infrastructure.Logging;
using DotnetServiceScaffold.Infrastructure.ServiceDiscovery;
using DotnetServiceScaffold.Infrastructure.ServiceMesh;
using DotnetServiceScaffold.Shared.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.Configuration;

/// <summary>
/// Starts a generic host with the real options registrations and checks that invalid configuration
/// fails at host start, before any hosted service runs.
/// </summary>
public class OptionsStartupValidationTests
{
    private const string TestJwtSecret = "startup-test-jwt-secret-of-sufficient-length";
    private const string TestMetricsApiKey = "metrics_startup_test_key";

    [Fact]
    public async Task StartAsync_ShouldStartHostedServices_WhenAllOptionsAreValid()
    {
        // Arrange
        var probe = new StartupProbeService();
        using var host = BuildHost(ValidApplicationSettings(), probe);

        // Act
        await host.StartAsync();

        // Assert
        probe.Started.Should().BeTrue();
        await host.StopAsync();
    }

    [Fact]
    public async Task StartAsync_ShouldThrowBeforeHostedServices_WhenApplicationSettingsAreInvalid()
    {
        // Arrange
        var values = ValidApplicationSettings();
        values["ApplicationSettings:HealthCheckInterval"] = "1";
        values["ApplicationSettings:JwtSecret"] = string.Empty;
        values["ApplicationSettings:MetricsApiKey"] = string.Empty;
        var probe = new StartupProbeService();
        using var host = BuildHost(values, probe);

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<OptionsValidationException>();
        exception.Which.Message.Should()
            .Contain("HealthCheckInterval")
            .And.Contain("JwtSecret")
            .And.Contain("HealthCheckTimeout")
            .And.Contain("MetricsApiKey");
        probe.Started.Should().BeFalse();
    }

    [Fact]
    public async Task StartAsync_ShouldReportEveryFailingProperty_WhenTwoPropertiesAreOutOfRange()
    {
        // Arrange
        var values = ValidApplicationSettings();
        values["ApplicationSettings:MaxConcurrentHealthChecks"] = "0";
        values["ApplicationSettings:RateLimitPerMinute"] = "1";
        using var host = BuildHost(values, new StartupProbeService());

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<OptionsValidationException>();
        exception.Which.Message.Should().Contain("MaxConcurrentHealthChecks").And.Contain("RateLimitPerMinute");
    }

    [Fact]
    public async Task StartAsync_ShouldThrowAndNameStructuredLoggingProperties_WhenLoggingOptionsAreInvalid()
    {
        // Arrange
        var values = ValidApplicationSettings();
        values["StructuredLogging:MinimumLevel"] = "Loud";
        values["StructuredLogging:ApplicationName"] = string.Empty;
        var probe = new StartupProbeService();
        using var host = BuildHost(values, probe);

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<OptionsValidationException>();
        exception.Which.Message.Should().Contain("ApplicationName").And.Contain("MinimumLevel");
        probe.Started.Should().BeFalse();
    }

    [Fact]
    public async Task StartAsync_ShouldThrowAndNameServiceMeshProperties_WhenServiceMeshOptionsAreInvalid()
    {
        // Arrange
        var values = ValidApplicationSettings();
        values["ServiceMesh:AdminEndpoint"] = "not a url";
        values["ServiceMesh:ReadinessTimeoutSeconds"] = "0";
        var probe = new StartupProbeService();
        using var host = BuildHost(values, probe);

        // Act
        var act = () => host.StartAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<OptionsValidationException>();
        exception.Which.Message.Should().Contain("AdminEndpoint").And.Contain("ReadinessTimeoutSeconds");
        probe.Started.Should().BeFalse();
    }

    [Fact]
    public void Options_ShouldThrowAndNameServiceDiscoveryProperties_WhenDiscoveryOptionsAreInvalid()
    {
        // Arrange
        // Resolving IOptions runs the same DataAnnotations and validator that ValidateOnStart uses.
        // Starting the host would also activate the discovery background services, which need the
        // data layer, so this check stays at the options level.
        var values = ValidApplicationSettings();
        values["ServiceDiscovery:Registry:AgentEndpoint"] = "not a url";
        values["ServiceDiscovery:Dns:DnsServerPort"] = "70000";
        var services = new ServiceCollection();
        services.AddServiceDiscovery(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        using var provider = services.BuildServiceProvider();

        // Act
        var act = () => provider.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .Which.Message.Should().Contain("AgentEndpoint").And.Contain("DnsServerPort");
    }

    private static Dictionary<string, string?> ValidApplicationSettings()
    {
        return new Dictionary<string, string?>
        {
            ["ApplicationSettings:HealthCheckInterval"] = "60",
            ["ApplicationSettings:HealthCheckTimeout"] = "10",
            ["ApplicationSettings:MaxConcurrentHealthChecks"] = "5",
            ["ApplicationSettings:RateLimitPerMinute"] = "60",
            ["ApplicationSettings:JwtSecret"] = TestJwtSecret,
            ["ApplicationSettings:MetricsProtectionMode"] = "ApiKey",
            ["ApplicationSettings:MetricsApiKey"] = TestMetricsApiKey,
            ["StructuredLogging:ApplicationName"] = "StartupTests",
            ["StructuredLogging:CorrelationIdHeader"] = "X-Correlation-Id",
            ["StructuredLogging:MinimumLevel"] = "Information",
            ["ServiceMesh:AdminEndpoint"] = "http://localhost:15000",
            ["ServiceMesh:ReadinessTimeoutSeconds"] = "5",
            ["ServiceMesh:MeshName"] = "default"
        };
    }

    private static IHost BuildHost(IDictionary<string, string?> values, StartupProbeService probe)
    {
        return new HostBuilder()
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(values))
            .ConfigureServices((context, services) =>
            {
                // Same order as Program.cs: options first, then anything that adds hosted services.
                services.AddDotnetServiceScaffoldOptions(context.Configuration);
                services.AddApplicationServices(context.Configuration);
                services.AddServiceMeshIntegration(context.Configuration);
                services.AddSingleton<IHostedService>(probe);
            })
            .Build();
    }

    private sealed class StartupProbeService : IHostedService
    {
        public bool Started { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Started = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
