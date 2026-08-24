namespace DotnetServiceScaffold.Benchmarks;

internal static class MetricsBenchmarksConstants
{
    public const string TagKeyService = "service";
    public const string TagValueUserService = "UserService";
    public const string TagKeyRegion = "region";
    public const string TagValueEuWest1 = "eu-west-1";
    public const string TagKeyEnv = "env";
    public const string TagValueProduction = "production";

    public const string CounterRequestsTotal = "requests.total";
    public const string TimingRequestDurationMs = "request.duration_ms";
    public const string GaugeMemoryMb = "memory.mb";

    public const int SetupLoopCount = 50;
    public const int SetupTimingBase = 10;
    public const int SetupTimingMod = 200;
    public const double SetupMemoryBase = 128.0;
    public const double SetupMemoryIncrement = 0.5;

    public const int TimingValue = 42;
    public const double GaugeValue = 256.5;
}
