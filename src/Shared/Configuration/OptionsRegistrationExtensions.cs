#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Shared.Configuration;

/// <summary>
/// Extension methods for binding options classes with fail-fast startup validation.
/// </summary>
public static class OptionsRegistrationExtensions
{
    /// <summary>
    /// Binds <typeparamref name="TOptions"/> from a configuration section and validates it
    /// with DataAnnotations. Validation runs on host start, so an invalid section stops the
    /// host before any hosted service starts. Register this before any <c>AddHostedService</c>
    /// call so the validation hosted service is started first.
    /// </summary>
    /// <typeparam name="TOptions">The options type to bind.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="sectionName">The configuration section to bind from.</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that custom validators can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sectionName"/> is null or empty.</exception>
    public static OptionsBuilder<TOptions> AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(sectionName);

        return services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
