using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICompleteApiUsageExample
{
    Task<string> RegisterUserAsync(string username, string email, string password, CancellationToken cancellationToken = default);
    Task LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<string> CreateApiKeyAsync(string name, List<string> scopes, List<string> ipWhitelist = null, CancellationToken cancellationToken = default);
    Task<string> RegisterServiceAsync(string name, string healthCheckUrl, string ownerId, CancellationToken cancellationToken = default);
    Task<string> GetServicesAsync(CancellationToken cancellationToken = default);
    Task<string> PerformHealthCheckAsync(string serviceId, CancellationToken cancellationToken = default);
    Task<string> GetHealthHistoryAsync(string serviceId, int days = CompleteApiUsageExampleConstants.DefaultHistoryDays, CancellationToken cancellationToken = default);
    Task<string> GetMetricsAsync(string serviceId = null, CancellationToken cancellationToken = default);
    Task<string> GetAuditLogsAsync(string userId = null, int days = CompleteApiUsageExampleConstants.DefaultAuditLogDays, CancellationToken cancellationToken = default);
    Task EnableServiceAsync(string serviceId, CancellationToken cancellationToken = default);
    Task DisableServiceAsync(string serviceId, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(string userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
}