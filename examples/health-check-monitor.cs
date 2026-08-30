#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
// Example: Health Check Monitoring
// This example demonstrates continuous monitoring of service health,
// tracking failures, and alerting on issues.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class HealthCheckMonitorExample : IHealthCheckMonitorExample, IEquatable<HealthCheckMonitorExample>
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;
    private Dictionary<string, int> _failureCount = new();
    private Dictionary<string, DateTime> _lastFailure = new();

    public string Id { get; set; }
    public string Status { get; set; }
    public int ResponseTime { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public DateTime CheckedAt { get; set; }

    public HealthCheckMonitorExample(string apiKey, string baseUrl = HealthCheckMonitorExampleConstants.DefaultBaseUrl)
    {
        _apiKey = apiKey;
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }

    public bool Equals(HealthCheckMonitorExample? other)
    {
        if (other is null) return false;
        return Id == other.Id &&
               Status == other.Status &&
               ResponseTime == other.ResponseTime &&
               StatusCode == other.StatusCode &&
               Message == other.Message &&
               CheckedAt == other.CheckedAt;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as HealthCheckMonitorExample);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Status, ResponseTime, StatusCode, Message, CheckedAt);
    }

    public static bool operator ==(HealthCheckMonitorExample? left, HealthCheckMonitorExample? right)
    {
        return EqualityComparer<HealthCheckMonitorExample>.Default.Equals(left, right);
    }

    public static bool operator !=(HealthCheckMonitorExample? left, HealthCheckMonitorExample? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Perform a health check on a service
    /// </summary>
    public async Task<(string status, int responseTime)> CheckServiceHealthAsync(string serviceId)
    {
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}{string.Format(HealthCheckMonitorExampleConstants.HealthCheckEndpoint, serviceId)}");
        httpRequest.Headers.Add(HealthCheckMonitorExampleConstants.ApiKeyHeader, _apiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                string.Format(HealthCheckMonitorExampleConstants.ErrorHealthCheckFailed, response.StatusCode));
        }

        using (var doc = JsonDocument.Parse(responseJson))
        {
            var data = doc.RootElement.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyData);
            var status = data.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyStatus).GetString();
            var responseTime = data.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyResponseTime).GetInt32();
            return (status, responseTime);
        }
    }

    /// <summary>
    /// Get health check history for a service
    /// </summary>
    public async Task<List<HealthCheckEntry>> GetHealthHistoryAsync(string serviceId, int days = HealthCheckMonitorExampleConstants.DefaultHistoryDays, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}{string.Format(HealthCheckMonitorExampleConstants.HealthHistoryEndpoint, serviceId, days, HealthCheckMonitorExampleConstants.DefaultHistoryLimit)}");
        httpRequest.Headers.Add(HealthCheckMonitorExampleConstants.ApiKeyHeader, _apiKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                string.Format(HealthCheckMonitorExampleConstants.ErrorFailedToGetHistory, response.StatusCode));
        }

        var entries = new List<HealthCheckEntry>();

        using (var doc = JsonDocument.Parse(responseJson))
        {
            var data = doc.RootElement.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyData).EnumerateArray();

            foreach (var item in data)
            {
                entries.Add(new HealthCheckEntry
                {
                    Id = item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyId).GetString(),
                    Status = item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyStatus).GetString(),
                    ResponseTime = item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyResponseTime).GetInt32(),
                    StatusCode = item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyStatusCode).GetInt32(),
                    CheckedAt = DateTime.Parse(item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyCheckedAt).GetString())
                });
            }
        }

        return entries.OrderByDescending(e => e.CheckedAt).ToList();
    }

    /// <summary>
    /// Get failed health checks
    /// </summary>
    public async Task<List<HealthCheckEntry>> GetFailuresAsync(string serviceId, int limit = HealthCheckMonitorExampleConstants.DefaultFailuresLimit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}{string.Format(HealthCheckMonitorExampleConstants.HealthFailuresEndpoint, serviceId, limit)}");
        httpRequest.Headers.Add(HealthCheckMonitorExampleConstants.ApiKeyHeader, _apiKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                string.Format(HealthCheckMonitorExampleConstants.ErrorFailedToGetFailures, response.StatusCode));
        }

        var entries = new List<HealthCheckEntry>();

        using (var doc = JsonDocument.Parse(responseJson))
        {
            var data = doc.RootElement.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyData).EnumerateArray();

            foreach (var item in data)
            {
                entries.Add(new HealthCheckEntry
                {
                    Id = item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyId).GetString(),
                    Status = item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyStatus).GetString(),
                    ResponseTime = item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyResponseTime).GetInt32(),
                    StatusCode = item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyStatusCode).GetInt32(),
                    Message = item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyMessage).GetString(),
                    CheckedAt = DateTime.Parse(item.GetProperty(HealthCheckMonitorExampleConstants.JsonPropertyCheckedAt).GetString())
                });
            }
        }

        return entries;
    }

    /// <summary>
    /// Analyze health trends
    /// </summary>
    public void AnalyzeTrends(List<HealthCheckEntry> history)
    {
        if (history.Count == 0)
        {
            Console.WriteLine("No history available");
            return;
        }

        var totalChecks = history.Count;
        var healthyChecks = history.Count(h => h.Status == HealthCheckMonitorExampleConstants.StatusHealthy);
        var degradedChecks = history.Count(h => h.Status == HealthCheckMonitorExampleConstants.StatusDegraded);
        var unhealthyChecks = history.Count(h => h.Status == HealthCheckMonitorExampleConstants.StatusUnhealthy);

        var successRate = (double)healthyChecks / totalChecks * 100;
        var avgResponseTime = history.Average(h => h.ResponseTime);
        var maxResponseTime = history.Max(h => h.ResponseTime);
        var minResponseTime = history.Min(h => h.ResponseTime);

        Console.WriteLine("=== Health Trends ===");
        Console.WriteLine($"Total Checks: {totalChecks}");
        Console.WriteLine($"Healthy: {healthyChecks} ({successRate:F1}%)");
        Console.WriteLine($"Degraded: {degradedChecks}");
        Console.WriteLine($"Unhealthy: {unhealthyChecks}");
        Console.WriteLine($"Average Response Time: {avgResponseTime:F0}ms");
        Console.WriteLine($"Max Response Time: {maxResponseTime}ms");
        Console.WriteLine($"Min Response Time: {minResponseTime}ms");
    }

    /// <summary>
    /// Alert if threshold exceeded
    /// </summary>
    private void CheckAlertThresholds(string serviceName, string status, int responseTime)
    {
        if (status == HealthCheckMonitorExampleConstants.StatusUnhealthy)
        {
            OnAlert(string.Format(HealthCheckMonitorExampleConstants.AlertCriticalFormat, serviceName));

            if (!_failureCount.ContainsKey(serviceName))
                _failureCount[serviceName] = 0;

            _failureCount[serviceName]++;
            _lastFailure[serviceName] = DateTime.Now;
        }
        else if (status == HealthCheckMonitorExampleConstants.StatusDegraded)
        {
            OnAlert(string.Format(HealthCheckMonitorExampleConstants.AlertWarningFormat, serviceName, responseTime));
        }
        else if (_failureCount.ContainsKey(serviceName) && _failureCount[serviceName] > 0)
        {
            OnAlert(string.Format(HealthCheckMonitorExampleConstants.AlertRecoveryFormat, serviceName));
            _failureCount[serviceName] = 0;
        }
    }

    /// <summary>
    /// Alert callback - override to implement custom alerting
    /// </summary>
    protected virtual void OnAlert(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ALERT] {DateTime.Now:{HealthCheckMonitorExampleConstants.DateTimeFormatFull}} - {message}");
        Console.ResetColor();
        // Could send email, Slack, PagerDuty, etc.
    }

    /// <summary>
    /// Monitor service continuously
    /// </summary>
    public async Task MonitorServiceAsync(string serviceName, string serviceId, int intervalSeconds = HealthCheckMonitorExampleConstants.DefaultMonitorIntervalSeconds)
    {
        Console.WriteLine($"Starting monitoring of {serviceName} (every {intervalSeconds}s)");
        Console.WriteLine("Press Ctrl+C to stop\n");

        while (true)
        {
            try
            {
                var (status, responseTime) = await CheckServiceHealthAsync(serviceId);

                var timestamp = DateTime.Now.ToString(HealthCheckMonitorExampleConstants.DateTimeFormatTimeOnly);
                Console.WriteLine($"[{timestamp}] {serviceName}: {status} ({responseTime}ms)");

                CheckAlertThresholds(serviceName, status, responseTime);

                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[ERROR] {DateTime.Now:{HealthCheckMonitorExampleConstants.DateTimeFormatTimeOnly}} - {serviceName}: {ex.Message}");
                Console.ResetColor();

                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds));
            }
        }
    }

    /// <summary>
    /// Generate health report
    /// </summary>
    public async Task<string> GenerateHealthReportAsync(string serviceName, string serviceId, int days = HealthCheckMonitorExampleConstants.DefaultHistoryDays, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var history = await GetHealthHistoryAsync(serviceId, days, cancellationToken);
        var failures = await GetFailuresAsync(serviceId, HealthCheckMonitorExampleConstants.DefaultFailuresLimit, cancellationToken);

        var report = new System.Text.StringBuilder();
        report.AppendLine($"=== Health Report: {serviceName} ===");
        report.AppendLine($"Report Generated: {DateTime.Now:HealthCheckMonitorExampleConstants.DateTimeFormatReport}");
        report.AppendLine($"Period: Last {days} days\n");

        // Statistics
        var totalChecks = history.Count;
        var healthyChecks = history.Count(h => h.Status == HealthCheckMonitorExampleConstants.StatusHealthy);
        var degradedChecks = history.Count(h => h.Status == HealthCheckMonitorExampleConstants.StatusDegraded);
        var unhealthyChecks = history.Count(h => h.Status == HealthCheckMonitorExampleConstants.StatusUnhealthy);
        var successRate = totalChecks > 0 ? (double)healthyChecks / totalChecks * 100 : 0;

        report.AppendLine("Statistics:");
        report.AppendLine($"  Total Checks: {totalChecks}");
        report.AppendLine($"  Success Rate: {successRate:F1}%");
        report.AppendLine($"  Healthy: {healthyChecks}");
        report.AppendLine($"  Degraded: {degradedChecks}");
        report.AppendLine($"  Unhealthy: {unhealthyChecks}\n");

        // Recent failures
        if (failures.Count > 0)
        {
            report.AppendLine("Recent Failures:");
            foreach (var failure in failures.Take(10))
            {
                report.AppendLine($"  {failure.CheckedAt:HealthCheckMonitorExampleConstants.DateTimeFormatReport} - {failure.Status} ({failure.StatusCode})");
            }
        }

        return report.ToString();
    }

    /// <summary>
    /// Example usage
    /// </summary>
    public static async Task Main(string[] args)
    {
        const string apiKey = HealthCheckMonitorExampleConstants.ExampleApiKey;
        const string serviceName = HealthCheckMonitorExampleConstants.ExampleServiceName;
        const string serviceId = HealthCheckMonitorExampleConstants.ExampleServiceId;

        var monitor = new HealthCheckMonitorExample(apiKey);

        try
        {
            // Single health check
            Console.WriteLine("=== Single Health Check ===\n");
            var (status, responseTime) = await monitor.CheckServiceHealthAsync(serviceId);
            Console.WriteLine($"Status: {status}, Response Time: {responseTime}ms\n");

            // Get recent history
            Console.WriteLine("=== Health History (Last 7 days) ===\n");
            var history = await monitor.GetHealthHistoryAsync(serviceId, HealthCheckMonitorExampleConstants.DefaultHistoryDays, CancellationToken.None);
            monitor.AnalyzeTrends(history);

            // Get failures
            Console.WriteLine("\n=== Recent Failures ===\n");
            var failures = await monitor.GetFailuresAsync(serviceId, HealthCheckMonitorExampleConstants.ExampleFailuresLimit, CancellationToken.None);
            foreach (var failure in failures)
            {
                Console.WriteLine($"{failure.CheckedAt:HealthCheckMonitorExampleConstants.DateTimeFormatReport} - {failure.Status} ({failure.StatusCode})");
            }

            // Generate report
            Console.WriteLine("\n=== Health Report ===\n");
            var report = await monitor.GenerateHealthReportAsync(serviceName, serviceId, HealthCheckMonitorExampleConstants.DefaultHistoryDays, CancellationToken.None);
            Console.WriteLine(report);

            // Monitor continuously (uncomment to run)
            // await monitor.MonitorServiceAsync(serviceName, serviceId, intervalSeconds: HealthCheckMonitorExampleConstants.ExampleMonitorIntervalSeconds);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}

public class HealthCheckEntry
{
    public string Id { get; set; }
    public string Status { get; set; }
    public int ResponseTime { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public DateTime CheckedAt { get; set; }
}
