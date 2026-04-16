#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    public static IServiceCollection AddStructuredLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StructuredLoggingOptions>(
            configuration.GetSection("StructuredLogging"));

        services.AddScoped<ILogContextService, LogContextService>();

        return services;
    }

    /// <summary>
    /// Registers the <see cref="CorrelationIdMiddleware"/> in the ASP.NET Core pipeline.
    /// Call this before other middleware that needs correlation IDs in logs.
    /// </summary>
    /// <param name="app">Application builder.</param>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();

    /// <summary>
    /// Configures Serilog with structured enrichers derived from <see cref="StructuredLoggingOptions"/>.
    /// Call during application startup before <c>builder.Host.UseSerilog()</c>.
    /// </summary>
    /// <param name="loggerConfig">The Serilog <see cref="LoggerConfiguration"/> to enrich.</param>
    /// <param name="options">Structured logging options.</param>
    /// <returns>The same <see cref="LoggerConfiguration"/> for chaining.</returns>
    public static LoggerConfiguration EnrichFromOptions(
        this LoggerConfiguration loggerConfig,
        StructuredLoggingOptions options)
    {
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
