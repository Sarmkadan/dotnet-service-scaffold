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
/// Extension methods for registering <see cref="DotnetServiceScaffoldOptions"/> with startup validation.
/// </summary>
public static class DotnetServiceScaffoldOptionsExtensions
{
    /// <summary>
    /// Binds <see cref="DotnetServiceScaffoldOptions"/> from the <c>ApplicationSettings</c> section,
    /// validates it with DataAnnotations and <see cref="DotnetServiceScaffoldOptionsValidator"/> on host start.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddDotnetServiceScaffoldOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatedOptions<DotnetServiceScaffoldOptions>(
            configuration,
            DotnetServiceScaffoldOptionsExtensionsConstants.SectionName);
        services.AddSingleton<IValidateOptions<DotnetServiceScaffoldOptions>, DotnetServiceScaffoldOptionsValidator>();

        return services;
    }
}

/// <summary>
/// Constants for <see cref="DotnetServiceScaffoldOptionsExtensions"/>.
/// </summary>
internal static class DotnetServiceScaffoldOptionsExtensionsConstants
{
    /// <summary>The configuration section that <see cref="DotnetServiceScaffoldOptions"/> binds from.</summary>
    public const string SectionName = "ApplicationSettings";
}
