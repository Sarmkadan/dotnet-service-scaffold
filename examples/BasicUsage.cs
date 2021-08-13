using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// Example: Minimal setup to register a service
// 1. Initialize HttpClient with your API key
// 2. Define the service registration payload
// 3. Post to the registration endpoint

public class BasicUsage
{
    public async Task RunAsync()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("X-API-Key", "sk_live_YOUR_KEY_HERE");

        var serviceDefinition = new
        {
            name = "OrderService",
            description = "Handles order processing",
            healthCheckUrl = "https://orders.internal/health",
            isEnabled = true
        };

        var response = await httpClient.PostAsJsonAsync(
            "http://localhost:5000/api/service/register", 
            serviceDefinition);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Service registered successfully.");
        }
        else
        {
            Console.WriteLine($"Error registering service: {response.ReasonPhrase}");
        }
    }
}
