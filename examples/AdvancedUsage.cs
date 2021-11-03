using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// Example usage of the service scaffold with advanced features.
/// </summary>
public class AdvancedUsage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedUsage"/> class.
    /// </summary>
    public AdvancedUsage()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", "sk_live_YOUR_KEY_HERE");
        _httpClient.BaseAddress = new Uri("http://localhost:5000");
    }

    /// <summary>
    /// Retrieves service metrics asynchronously.
    /// </summary>
    /// <param name="serviceId">The ID of the service to retrieve metrics for.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

    /// <summary>
    /// Simplified model for demonstration of service metrics.
    /// </summary>
    public class MetricsResponse { public MetricsData Data { get; set; } }
    /// <summary>
    /// Simplified model for demonstration of service metrics data.
    /// </summary>
    public class MetricsData { public decimal CpuUsage { get; set; } public decimal ErrorRate { get; set; } }
}
