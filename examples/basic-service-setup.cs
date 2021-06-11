// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
// Example: Basic Service Setup
// This example demonstrates how to register and manage services using
// the dotnet-service-scaffold API.

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class BasicServiceSetupExample
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;

    public BasicServiceSetupExample(string apiKey, string baseUrl = "http://localhost:5000")
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Register a new service for monitoring
    /// </summary>
    public async Task<string> RegisterServiceAsync(
        string name,
        string description,
        string healthCheckUrl,
        string ownerId)
    {
        var request = new
        {
            name = name,
            description = description,
            healthCheckUrl = healthCheckUrl,
            ownerId = ownerId,
            isEnabled = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/api/service/register")
        {
            Content = content
        };
        httpRequest.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to register service: {response.StatusCode} - {responseJson}");
        }

        using (var doc = JsonDocument.Parse(responseJson))
        {
            var serviceId = doc.RootElement
                .GetProperty("data")
                .GetProperty("id")
                .GetString();
            return serviceId;
        }
    }

    /// <summary>
    /// List all registered services
    /// </summary>
    public async Task ListServicesAsync()
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}/api/service?limit=50");
        httpRequest.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to list services: {response.StatusCode}");
        }

        using (var doc = JsonDocument.Parse(responseJson))
        {
            var services = doc.RootElement.GetProperty("data").EnumerateArray();

            Console.WriteLine("=== Registered Services ===\n");
            foreach (var service in services)
            {
                var id = service.GetProperty("id").GetString();
                var name = service.GetProperty("name").GetString();
                var status = service.GetProperty("status").GetString();
                var successRate = service.GetProperty("successRate").GetDouble();

                Console.WriteLine($"ID: {id}");
                Console.WriteLine($"  Name: {name}");
                Console.WriteLine($"  Status: {status}");
                Console.WriteLine($"  Success Rate: {successRate:F1}%\n");
            }
        }
    }

    /// <summary>
    /// Enable monitoring for a service
    /// </summary>
    public async Task EnableServiceAsync(string serviceId)
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/api/service/{serviceId}/enable");
        httpRequest.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to enable service: {response.StatusCode}");
        }

        Console.WriteLine($"Service {serviceId} enabled");
    }

    /// <summary>
    /// Disable monitoring for a service
    /// </summary>
    public async Task DisableServiceAsync(string serviceId)
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/api/service/{serviceId}/disable");
        httpRequest.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to disable service: {response.StatusCode}");
        }

        Console.WriteLine($"Service {serviceId} disabled");
    }

    /// <summary>
    /// Get detailed information about a service
    /// </summary>
    public async Task GetServiceDetailsAsync(string serviceId)
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}/api/service/{serviceId}");
        httpRequest.Headers.Add("X-API-Key", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to get service details: {response.StatusCode}");
        }

        using (var doc = JsonDocument.Parse(responseJson))
        {
            var service = doc.RootElement.GetProperty("data");

            Console.WriteLine("=== Service Details ===\n");
            Console.WriteLine($"ID: {service.GetProperty("id").GetString()}");
            Console.WriteLine($"Name: {service.GetProperty("name").GetString()}");
            Console.WriteLine($"Description: {service.GetProperty("description").GetString()}");
            Console.WriteLine($"Status: {service.GetProperty("status").GetString()}");
            Console.WriteLine($"Health Check URL: {service.GetProperty("healthCheckUrl").GetString()}");
            Console.WriteLine($"Success Rate: {service.GetProperty("successRate").GetDouble():F1}%");
            Console.WriteLine($"Last Checked: {service.GetProperty("lastCheckedAt").GetString()}");
            Console.WriteLine($"Enabled: {service.GetProperty("isEnabled").GetBoolean()}");
        }
    }

    /// <summary>
    /// Example usage
    /// </summary>
    public static async Task Main(string[] args)
    {
        const string apiKey = "sk_live_your_api_key_here";
        var example = new BasicServiceSetupExample(apiKey);

        try
        {
            // Register a new service
            Console.WriteLine("Registering UserService...\n");
            var serviceId = await example.RegisterServiceAsync(
                name: "UserService",
                description: "User authentication and management service",
                healthCheckUrl: "https://users.internal:8443/health",
                ownerId: "user-12345678");
            Console.WriteLine($"Service registered with ID: {serviceId}\n");

            // List all services
            await example.ListServicesAsync();

            // Get service details
            Console.WriteLine("\nGetting service details...\n");
            await example.GetServiceDetailsAsync(serviceId);

            // Disable service
            Console.WriteLine("\n\nDisabling service...\n");
            await example.DisableServiceAsync(serviceId);

            // Re-enable service
            Console.WriteLine("Re-enabling service...\n");
            await example.EnableServiceAsync(serviceId);

            Console.WriteLine("\nExample completed successfully!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
