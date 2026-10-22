#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Extension methods for registering <see cref="HealthCheckOptions"/> with startup validation.
/// </summary>
public static class HealthCheckOptionsExtensions
{
    /// <summary>
    /// Binds <see cref="HealthCheckOptions"/> from the <c>HealthCheck</c> section and validates it
    /// with <see cref="HealthCheckOptionsValidator"/> when the host starts.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddHealthCheckOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatedOptions<HealthCheckOptions>(configuration, HealthCheckOptions.SectionName);
        services.AddSingleton<IValidateOptions<HealthCheckOptions>, HealthCheckOptionsValidator>();

        return services;
    }
}
