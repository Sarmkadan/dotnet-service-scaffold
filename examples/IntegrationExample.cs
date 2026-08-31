using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

// Example: Integrating with ASP.NET Core DI
// Demonstrates how to register the scaffold client in Program.cs

/// <summary>
/// Provides configuration entry point for integrating the scaffold client with ASP.NET Core dependency injection.
/// </summary>
public class Startup : IStartup
{
    	/// <summary>
	/// Configures application services for dependency injection.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to configure.</param>
	public void ConfigureServices(IServiceCollection services)
    {
        // Add HttpClient with configured base address and API key
        services.AddHttpClient("ScaffoldClient", client =>
        {
            client.BaseAddress = new Uri(StartupConstants.BaseAddressUrl);
            client.DefaultRequestHeaders.Add(StartupConstants.ApiKeyHeaderName, StartupConstants.ApiKeyValue);
        });

        // Register a wrapper service that uses the HttpClient
        services.AddScoped<IScaffoldIntegrationService, ScaffoldIntegrationService>();
    }
}

/// <summary>
/// Defines the contract for scaffold integration services that provide HTTP client functionality.
/// </summary>
public interface IScaffoldIntegrationService { /* ... */ }
/// <summary>
/// Provides a wrapper service for making HTTP requests to the scaffold API using the configured HttpClient.
/// </summary>
public class ScaffoldIntegrationService : IScaffoldIntegrationService
{
    private readonly HttpClient _httpClient;
    	/// <summary>
	/// Initializes a new instance of the <see cref="ScaffoldIntegrationService"/> class.
	/// </summary>
	/// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> instance used to create named HttpClient instances.</param>
	public ScaffoldIntegrationService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ScaffoldClient");
    }
}
