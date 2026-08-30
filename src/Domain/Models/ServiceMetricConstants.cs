#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Domain.Models;

internal static class ServiceMetricConstants
{
    public const int MaximumNotesLength = 500;
    public const decimal AnomalousCpuUsagePercent = 85;
    public const decimal AnomalousMemoryUsagePercent = 80;
    public const decimal AnomalousDiskUsagePercent = 90;
    public const decimal AnomalousAverageResponseTimeMs = 5000;
    public const long NoRequests = 0;
    public const decimal ZeroErrorRate = 0;
    public const decimal PercentageMultiplier = 100;
    public const decimal CriticalCpuUsagePercent = 90;
    public const decimal CriticalMemoryUsagePercent = 85;
    public const decimal WarningCpuUsagePercent = 75;
    public const decimal WarningMemoryUsagePercent = 70;
    public const decimal WarningDiskUsagePercent = 85;
    public const string CriticalSeverity = "Critical";
    public const string WarningSeverity = "Warning";
    public const string NormalSeverity = "Normal";
    public const long BytesPerMegabyte = 1024 * 1024;
    public const string MetricsDisplayFormat =
        "CPU: {0:F1}% | Memory: {1:F1}% ({2}MB) | Disk: {3:F1}% | RPS: {4} | " +
        "Avg Response: {5:F0}ms | Errors: {6}/{7} | Uptime: {8:F2}%";
}
