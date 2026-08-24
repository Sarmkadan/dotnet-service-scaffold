namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

internal static class MemoryHealthCheckConstants
{
    public const double DefaultHealthyThresholdPercent = 70;
    public const double DefaultDegradedThresholdPercent = 85;
    public const double DefaultUnhealthyThresholdPercent = 95;
    public const double MinThresholdPercent = 1;
    public const double MaxThresholdPercent = 99;

    public const int TimeoutSeconds = 2;
    public const int BytesToMegabytesFactor = 1024 * 1024;
}
