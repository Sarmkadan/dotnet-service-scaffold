using System.Collections.Generic;
using System.Threading.Tasks;

public interface IHealthCheckMonitorExample
{
    Task<(string status, int responseTime)> CheckServiceHealthAsync(string serviceId);
    Task<List<HealthCheckEntry>> GetHealthHistoryAsync(string serviceId, int days = 7);
    Task<List<HealthCheckEntry>> GetFailuresAsync(string serviceId, int limit = 50);
    void AnalyzeTrends(List<HealthCheckEntry> history);
    Task MonitorServiceAsync(string serviceName, string serviceId, int intervalSeconds = 60);
    Task<string> GenerateHealthReportAsync(string serviceName, string serviceId, int days = 7);
}
