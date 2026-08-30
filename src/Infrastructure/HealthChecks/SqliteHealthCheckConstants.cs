namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Constants for the SQLite health check.
/// </summary>
internal static class SqliteHealthCheckConstants
{
    public const long DefaultDegradedDiskSpaceThresholdBytes = 512 * 1024 * 1024;
    public const int TimeoutSeconds = 2;
    public const int BytesPerMebibyte = 1024 * 1024;
    public const int FileStreamBufferSize = 4096;
    public const string CurrentDirectoryIndicator = ".";
    public const string FileExistsKey = "fileExists";
}
