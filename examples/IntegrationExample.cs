using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

// Example: Integrating with ASP.NET Core DI
// Demonstrates how to register the scaffold client in Program.cs

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add HttpClient with configured base address and API key
        services.AddHttpClient("ScaffoldClient", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
            client.DefaultRequestHeaders.Add("X-API-Key", "sk_live_YOUR_KEY_HERE");
        });

        // Register a wrapper service that uses the HttpClient
        services.AddScoped<IScaffoldIntegrationService, ScaffoldIntegrationService>();
    }
}

public interface IScaffoldIntegrationService { /* ... */ }
public class ScaffoldIntegrationService : IScaffoldIntegrationService
{
    private readonly HttpClient _httpClient;
    public ScaffoldIntegrationService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ScaffoldClient");
    }
}
