namespace DotnetServiceScaffold.Application.Services;

internal static class IAuditServiceConstants
{
    public const int DefaultGetUserAuditLogsCount = 50;
    public const int DefaultGetRecentLogsCount = 100;
    public const int DefaultGetFailedActionsCount = 50;
    public const int DefaultCleanupOldLogsDaysToKeep = 90;
}
