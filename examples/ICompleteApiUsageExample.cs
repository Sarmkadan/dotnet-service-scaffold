using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICompleteApiUsageExample
{
    Task<string> RegisterUserAsync(string username, string email, string password);
    Task LoginAsync(string username, string password);
    Task<string> CreateApiKeyAsync(string name, List<string> scopes, List<string> ipWhitelist = null);
    Task<string> RegisterServiceAsync(string name, string healthCheckUrl, string ownerId);
    Task<string> GetServicesAsync();
    Task<string> PerformHealthCheckAsync(string serviceId);
    Task<string> GetHealthHistoryAsync(string serviceId, int days = CompleteApiUsageExampleConstants.DefaultHistoryDays);
    Task<string> GetMetricsAsync(string serviceId = null);
    Task<string> GetAuditLogsAsync(string userId = null, int days = CompleteApiUsageExampleConstants.DefaultAuditLogDays);
    Task EnableServiceAsync(string serviceId);
    Task DisableServiceAsync(string serviceId);
    Task ChangePasswordAsync(string userId, string oldPassword, string newPassword);
}