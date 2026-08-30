#nullable enable
namespace DotnetServiceScaffold.Domain.Models
{
    /// <summary>
    /// Constants for ServiceEventExtensions.
    /// </summary>
    internal static class ServiceEventExtensionsConstants
    {
        public const string DefaultSeverityWhenNull = "Info";
        public const string DefaultMessageWhenNull = "No message";
        public const string CriticalSeverity = "critical";
        public const string ErrorSeverity = "error";
        public const string WarningSeverity = "warning";
        public const string InfoSeverity = "info";

        public const int DefaultSeverityPriority = 2;
        public const int CriticalSeverityPriority = 5;
        public const int ErrorSeverityPriority = 4;
        public const int WarningSeverityPriority = 3;
        public const int InfoSeverityPriority = 1;

        public const int ServiceDownPriority = 3;
        public const int HealthCheckFailedPriority = 4;
        public const int ErrorOccurredPriority = 3;
        public const int ServiceRestartedPriority = 2;
        public const int DeploymentStartedPriority = 2;
        public const int DeploymentCompletedPriority = 1;
        public const int ConfigurationChangedPriority = 1;
        public const int ServiceUpPriority = 1;
        public const int HealthCheckPassedPriority = 0;
        public const int ServiceDisabledPriority = 2;
        public const int ServiceEnabledPriority = 1;
        public const int UnknownEventTypePriority = 1;

        public const int MinPriorityLevel = 0;
        public const int MaxPriorityLevel = 5;
    }
}