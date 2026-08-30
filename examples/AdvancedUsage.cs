using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// Example usage of the service scaffold with advanced features.
/// </summary>
public class AdvancedUsage : IAdvancedUsage, IEquatable<AdvancedUsage>
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedUsage"/> class.
    /// </summary>
    public AdvancedUsage()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", "sk_live_YOUR_KEY_HERE");
        _httpClient.BaseAddress = new Uri("http://localhost:5000");
        Data = new MetricsResponse { Data = new MetricsData() };
        CpuUsage = 0;
        ErrorRate = 0;
    }

    /// <summary>
    /// Gets or sets the metrics response.
    /// </summary>
    public MetricsResponse Data { get; set; }

    /// <summary>
    /// Gets or sets the CPU usage percentage.
    /// </summary>
    public decimal CpuUsage { get; set; }

    /// <summary>
    /// Gets or sets the error rate.
    /// </summary>
    public decimal ErrorRate { get; set; }

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
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other"> parameter; otherwise, false.</returns>
    public bool Equals(AdvancedUsage? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Data.Equals(other.Data) &&
               CpuUsage == other.CpuUsage &&
               ErrorRate == other.ErrorRate;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as AdvancedUsage);

    /// <summary>
    /// Serves as the default hash function.
    /// </>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => HashCode.Combine(Data, CpuUsage, ErrorRate);

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>true if the operands are equal; otherwise, false.</returns>
    public static bool operator ==(AdvancedUsage? left, AdvancedUsage? right) => EqualityComparer<AdvancedUsage>.Default.Equals(left, right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>true if the operands are not equal; otherwise, false.</returns>
    public static bool operator !=(AdvancedUsage? left, AdvancedUsage? right) => !(left == right);

    /// <summary>
    /// Simplified model for demonstration of service metrics.
    /// </summary>
    public class MetricsResponse { public MetricsData Data { get; set; } }
    /// <summary>
    /// Simplified model for demonstration of service metrics data.
    /// </summary>
    public class MetricsData { public decimal CpuUsage { get; set; } public decimal ErrorRate { get; set; } }
}
