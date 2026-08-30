#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Extension methods for wiring up the structured logging pipeline in Program.cs.
/// </summary>
public static class StructuredLoggingExtensions
{
    /// <summary>
    /// Registers <see cref="ILogContextService"/> and <see cref="StructuredLoggingOptions"/>
    /// into the DI container and configures Serilog enrichers based on the options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing the structured logging configuration.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddStructuredLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<StructuredLoggingOptions>(
            configuration.GetSection(StructuredLoggingExtensionsConstants.StructuredLoggingSectionName));

        services.AddScoped<ILogContextService, LogContextService>();

        return services;
    }

    /// <summary>
    /// Registers the <see cref="CorrelationIdMiddleware"/> in the ASP.NET Core pipeline.
    /// Call this before other middleware that needs correlation IDs in logs.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IApplicationBuilder"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Configures Serilog with structured enrichers derived from <see cref="StructuredLoggingOptions"/>.
    /// Call during application startup before <c>builder.Host.UseSerilog()</c>.
    /// </summary>
    /// <param name="loggerConfig">The Serilog <see cref="LoggerConfiguration"/> to enrich.</param>
    /// <param name="options">Structured logging options.</param>
    /// <returns>The same <see cref="LoggerConfiguration"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerConfig"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    public static LoggerConfiguration EnrichFromOptions(
        this LoggerConfiguration loggerConfig,
        StructuredLoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(loggerConfig);
        ArgumentNullException.ThrowIfNull(options);

        loggerConfig = loggerConfig
            .Enrich.WithProperty("Application", options.ApplicationName)
            .Enrich.FromLogContext();

        if (options.EnrichWithMachineName)
        {
            loggerConfig = loggerConfig.Enrich.WithMachineName();
        }

        if (options.EnrichWithEnvironment)
        {
            loggerConfig = loggerConfig.Enrich.WithEnvironmentName();
        }

        return loggerConfig;
    }
}