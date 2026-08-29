using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides configuration entry point for integrating the scaffold client with ASP.NET Core dependency injection.
/// </summary>
public interface IStartup
{
    void ConfigureServices(IServiceCollection services);
}