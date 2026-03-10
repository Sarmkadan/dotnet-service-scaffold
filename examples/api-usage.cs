// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
// Example: Complete API Usage
// Demonstrates all major API operations: users, services, health checks,
// metrics, and audit logs.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class CompleteApiUsageExample
{
    private readonly string _baseUrl;
    private HttpClient _httpClient;
    private string _adminApiKey;
    private string _currentUserToken;

    public CompleteApiUsageExample(string baseUrl = "http://localhost:5000")
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    public async Task<string> RegisterUserAsync(string username, string email, string password)
    {
        var request = new
        {
            username = username,
            email = email,
            password = password
        };

        var response = await PostAsync("/api/user/register", request);
        using (var doc = JsonDocument.Parse(response))
        {
            return doc.RootElement.GetProperty("data").GetProperty("userId").GetString();
        }
    }

    /// <summary>
    /// Login and get JWT token
    /// </summary>
    public async Task LoginAsync(string username, string password)
    {
        var request = new { username = username, password = password };
        var response = await PostAsync("/api/user/login", request);

        using (var doc = JsonDocument.Parse(response))
        {
            _currentUserToken = doc.RootElement.GetProperty("data").GetProperty("token").GetString();
        }
    }

    /// <summary>
    /// Create API key for service authentication
    /// </summary>
    public async Task<string> CreateApiKeyAsync(string name, List<string> scopes, List<string> ipWhitelist = null)
    {
        var request = new
        {
            name = name,
            description = $"API key for {name}",
            scopes = scopes,
            ipWhitelist = ipWhitelist ?? new List<string> { "0.0.0.0/0" }
        };

        var response = await PostAsync("/api/apikey/create", request, useApiKey: true);
        using (var doc = JsonDocument.Parse(response))
        {
            var apiKey = doc.RootElement.GetProperty("data").GetProperty("apiKey").GetString();
            _adminApiKey = apiKey;
            return apiKey;
        }
    }

    /// <summary>
    /// Register service for monitoring
    /// </summary>
    public async Task<string> RegisterServiceAsync(string name, string healthCheckUrl, string ownerId)
    {
        var request = new
        {
            name = name,
            description = $"Service: {name}",
            healthCheckUrl = healthCheckUrl,
            ownerId = ownerId,
            isEnabled = true
        };

        var response = await PostAsync("/api/service/register", request, useApiKey: true);
        using (var doc = JsonDocument.Parse(response))
        {
            return doc.RootElement.GetProperty("data").GetProperty("id").GetString();
        }
    }

    /// <summary>
    /// Get all services
    /// </summary>
    public async Task<string> GetServicesAsync()
    {
        return await GetAsync("/api/service?limit=50", useApiKey: true);
    }

    /// <summary>
    /// Run health check
    /// </summary>
    public async Task<string> PerformHealthCheckAsync(string serviceId)
    {
        return await PostAsync($"/api/healthcheck/{serviceId}/check", null, useApiKey: true);
    }

    /// <summary>
    /// Get health history
    /// </summary>
    public async Task<string> GetHealthHistoryAsync(string serviceId, int days = 7)
    {
        return await GetAsync($"/api/healthcheck/{serviceId}/history?days={days}&limit=100", useApiKey: true);
    }

    /// <summary>
    /// Get service metrics
    /// </summary>
    public async Task<string> GetMetricsAsync(string serviceId = null)
    {
        var endpoint = serviceId != null
            ? $"/api/metrics/service/{serviceId}"
            : "/api/metrics";
        return await GetAsync(endpoint, useApiKey: true);
    }

    /// <summary>
    /// Get audit logs
    /// </summary>
    public async Task<string> GetAuditLogsAsync(string userId = null, int days = 30)
    {
        var endpoint = $"/api/auditlog?days={days}&limit=100";
        if (!string.IsNullOrEmpty(userId))
            endpoint += $"&userId={userId}";

        return await GetAsync(endpoint, useApiKey: true);
    }

    /// <summary>
    /// Enable service monitoring
    /// </summary>
    public async Task EnableServiceAsync(string serviceId)
    {
        await PostAsync($"/api/service/{serviceId}/enable", null, useApiKey: true);
    }

    /// <summary>
    /// Disable service monitoring
    /// </summary>
    public async Task DisableServiceAsync(string serviceId)
    {
        await PostAsync($"/api/service/{serviceId}/disable", null, useApiKey: true);
    }

    /// <summary>
    /// Change password
    /// </summary>
    public async Task ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var request = new
        {
            oldPassword = oldPassword,
            newPassword = newPassword
        };

        await PostAsync($"/api/user/{userId}/change-password", request, useToken: true);
    }

    // Helper methods

    private async Task<string> GetAsync(string endpoint, bool useApiKey = false, bool useToken = false)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}{endpoint}");

        if (useApiKey && !string.IsNullOrEmpty(_adminApiKey))
            httpRequest.Headers.Add("X-API-Key", _adminApiKey);

        if (useToken && !string.IsNullOrEmpty(_currentUserToken))
            httpRequest.Headers.Add("Authorization", $"Bearer {_currentUserToken}");

        var response = await _httpClient.SendAsync(httpRequest);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed: {response.StatusCode} - {content}");

        return content;
    }

    private async Task<string> PostAsync(string endpoint, object data = null, bool useApiKey = false, bool useToken = false)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{endpoint}");

        if (useApiKey && !string.IsNullOrEmpty(_adminApiKey))
            httpRequest.Headers.Add("X-API-Key", _adminApiKey);

        if (useToken && !string.IsNullOrEmpty(_currentUserToken))
            httpRequest.Headers.Add("Authorization", $"Bearer {_currentUserToken}");

        if (data != null)
        {
            var json = JsonSerializer.Serialize(data);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(httpRequest);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed: {response.StatusCode} - {content}");

        return content;
    }

    private void PrintJson(string json)
    {
        using (var doc = JsonDocument.Parse(json))
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            Console.WriteLine(JsonSerializer.Serialize(doc.RootElement, options));
        }
    }

    /// <summary>
    /// Example usage demonstrating complete workflow
    /// </summary>
    public static async Task Main(string[] args)
    {
        var api = new CompleteApiUsageExample();

        try
        {
            Console.WriteLine("=== API Usage Example ===\n");

            // 1. Register user
            Console.WriteLine("1. Registering user...");
            var userId = await api.RegisterUserAsync(
                "john.doe",
                "john@example.com",
                "SecurePassword123!");
            Console.WriteLine($"   User created: {userId}\n");

            // 2. Login
            Console.WriteLine("2. Logging in...");
            await api.LoginAsync("john.doe", "SecurePassword123!");
            Console.WriteLine("   Login successful\n");

            // 3. Create API key
            Console.WriteLine("3. Creating API key...");
            var apiKey = await api.CreateApiKeyAsync(
                "Monitoring App",
                new List<string> { "service:read", "healthcheck:read" });
            Console.WriteLine($"   API Key: {apiKey}\n");

            // 4. Register service
            Console.WriteLine("4. Registering service...");
            var serviceId = await api.RegisterServiceAsync(
                "PaymentService",
                "https://payments.internal:8443/health",
                userId);
            Console.WriteLine($"   Service registered: {serviceId}\n");

            // 5. Get services
            Console.WriteLine("5. Getting all services...");
            var servicesJson = await api.GetServicesAsync();
            api.PrintJson(servicesJson);

            // 6. Perform health check
            Console.WriteLine("\n6. Performing health check...");
            var healthCheckJson = await api.PerformHealthCheckAsync(serviceId);
            api.PrintJson(healthCheckJson);

            // 7. Get health history
            Console.WriteLine("\n7. Getting health history (last 7 days)...");
            var historyJson = await api.GetHealthHistoryAsync(serviceId, 7);
            api.PrintJson(historyJson);

            // 8. Get metrics
            Console.WriteLine("\n8. Getting service metrics...");
            var metricsJson = await api.GetMetricsAsync(serviceId);
            api.PrintJson(metricsJson);

            // 9. Get audit logs
            Console.WriteLine("\n9. Getting audit logs...");
            var auditJson = await api.GetAuditLogsAsync(userId, 30);
            api.PrintJson(auditJson);

            // 10. Disable service
            Console.WriteLine("\n10. Disabling service...");
            await api.DisableServiceAsync(serviceId);
            Console.WriteLine("    Service disabled\n");

            // 11. Enable service
            Console.WriteLine("11. Enabling service...");
            await api.EnableServiceAsync(serviceId);
            Console.WriteLine("    Service enabled\n");

            // 12. Change password
            Console.WriteLine("12. Changing password...");
            await api.ChangePasswordAsync(userId, "SecurePassword123!", "NewSecurePassword456!");
            Console.WriteLine("    Password changed\n");

            Console.WriteLine("=== Example completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
