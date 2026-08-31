#nullable enable
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
using System.Threading;
using System.Threading.Tasks;

public class BasicServiceSetupExample : IBasicServiceSetupExample
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;

    public BasicServiceSetupExample(string apiKey, string baseUrl = BasicServiceSetupExampleConstants.DefaultBaseUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        ArgumentException.ThrowIfNullOrEmpty(baseUrl);
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
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(description);
        ArgumentException.ThrowIfNullOrEmpty(healthCheckUrl);
        ArgumentException.ThrowIfNullOrEmpty(ownerId);
        cancellationToken.ThrowIfCancellationRequested();

        var request = new
        {
            name = name,
            description = description,
            healthCheckUrl = healthCheckUrl,
            ownerId = ownerId,
            isEnabled = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, BasicServiceSetupExampleConstants.JsonContentType);

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}{BasicServiceSetupExampleConstants.RegisterEndpoint}")
        {
            Content = content
        };
        httpRequest.Headers.Add(BasicServiceSetupExampleConstants.ApiKeyHeader, _apiKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to register service: {response.StatusCode} - {responseJson}");
        }

        using (var doc = JsonDocument.Parse(responseJson))
        {
            var serviceId = doc.RootElement
                .GetProperty(BasicServiceSetupExampleConstants.JsonDataProperty)
                .GetProperty(BasicServiceSetupExampleConstants.JsonIdProperty)
                .GetString();
            return serviceId!;
        }
    }

    /// <summary>
    /// List all registered services
    /// </summary>
    public async Task ListServicesAsync()
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}{BasicServiceSetupExampleConstants.ListServicesEndpoint}?limit={BasicServiceSetupExampleConstants.ListServicesLimit}");
        httpRequest.Headers.Add(BasicServiceSetupExampleConstants.ApiKeyHeader, _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to list services: {response.StatusCode}");
        }

        using (var doc = JsonDocument.Parse(responseJson))
        {
            var services = doc.RootElement.GetProperty(BasicServiceSetupExampleConstants.JsonDataProperty).EnumerateArray();

            Console.WriteLine("=== Registered Services ===\n");
            foreach (var service in services)
            {
                var id = service.GetProperty(BasicServiceSetupExampleConstants.JsonIdProperty).GetString();
                var name = service.GetProperty(BasicServiceSetupExampleConstants.JsonNameProperty).GetString();
                var status = service.GetProperty(BasicServiceSetupExampleConstants.JsonStatusProperty).GetString();
                var successRate = service.GetProperty(BasicServiceSetupExampleConstants.JsonSuccessRateProperty).GetDouble();

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
        ArgumentException.ThrowIfNullOrEmpty(serviceId);
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}{string.Format(BasicServiceSetupExampleConstants.EnableServiceEndpoint, serviceId)}");
        httpRequest.Headers.Add(BasicServiceSetupExampleConstants.ApiKeyHeader, _apiKey);

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
        ArgumentException.ThrowIfNullOrEmpty(serviceId);
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}{string.Format(BasicServiceSetupExampleConstants.DisableServiceEndpoint, serviceId)}");
        httpRequest.Headers.Add(BasicServiceSetupExampleConstants.ApiKeyHeader, _apiKey);

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
        ArgumentException.ThrowIfNullOrEmpty(serviceId);
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}{string.Format(BasicServiceSetupExampleConstants.GetServiceDetailsEndpoint, serviceId)}");
        httpRequest.Headers.Add(BasicServiceSetupExampleConstants.ApiKeyHeader, _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to get service details: {response.StatusCode}");
        }

        using (var doc = JsonDocument.Parse(responseJson))
        {
            var service = doc.RootElement.GetProperty(BasicServiceSetupExampleConstants.JsonDataProperty);

            Console.WriteLine("=== Service Details ===\n");
            Console.WriteLine($"ID: {service.GetProperty(BasicServiceSetupExampleConstants.JsonIdProperty).GetString()}");
            Console.WriteLine($"Name: {service.GetProperty(BasicServiceSetupExampleConstants.JsonNameProperty).GetString()}");
            Console.WriteLine($"Description: {service.GetProperty(BasicServiceSetupExampleConstants.JsonDescriptionProperty).GetString()}");
            Console.WriteLine($"Status: {service.GetProperty(BasicServiceSetupExampleConstants.JsonStatusProperty).GetString()}");
            Console.WriteLine($"Health Check URL: {service.GetProperty(BasicServiceSetupExampleConstants.JsonHealthCheckUrlProperty).GetString()}");
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
        ArgumentNullException.ThrowIfNull(args);
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
