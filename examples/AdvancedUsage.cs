using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

// Example: Advanced usage with error handling and custom options
// Demonstrates handling potential exceptions and parsing structured JSON responses

public class AdvancedUsage
{
    private readonly HttpClient _httpClient;

    public AdvancedUsage()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", "sk_live_YOUR_KEY_HERE");
        _httpClient.BaseAddress = new Uri("http://localhost:5000");
    }

    public async Task GetServiceMetricsAsync(string serviceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/metrics/service/{serviceId}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<MetricsResponse>();
            
            Console.WriteLine($"Service: {serviceId}");
            Console.WriteLine($"CPU: {result.Data.CpuUsage}%");
            Console.WriteLine($"Error Rate: {result.Data.ErrorRate:P2}");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }

    // Simplified model for demonstration
    public class MetricsResponse { public MetricsData Data { get; set; } }
    public class MetricsData { public decimal CpuUsage { get; set; } public decimal ErrorRate { get; set; } }
}
