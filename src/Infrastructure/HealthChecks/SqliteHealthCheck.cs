#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Health check that verifies the SQLite database file is accessible and writable,
/// and reports available disk space as a degraded warning when running low.
/// </summary>
public class SqliteHealthCheck : IHealthCheck
{
    private readonly string _databasePath;
    private readonly long _degradedDiskSpaceThresholdBytes;

    /// <param name="databasePath">Absolute or relative path to the SQLite database file.</param>
    /// <param name="degradedDiskSpaceThresholdBytes">
    /// Available disk space below which the check reports Degraded. Defaults to 512 MB.
    /// </param>
    public SqliteHealthCheck(string databasePath, long degradedDiskSpaceThresholdBytes = 512 * 1024 * 1024)
    {
        _databasePath = databasePath;
        _degradedDiskSpaceThresholdBytes = degradedDiskSpaceThresholdBytes;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        // Resolve the full path so diagnostics show an unambiguous location.
        var fullPath = Path.GetFullPath(_databasePath);
        data["databasePath"] = fullPath;

        // Check disk space on the volume that holds the database directory.
        var directory = Path.GetDirectoryName(fullPath) ?? ".";
        try
        {
            var driveInfo = new DriveInfo(directory);
            var availableBytes = driveInfo.AvailableFreeSpace;
            data["diskAvailableBytes"] = availableBytes;
            data["diskAvailableMB"] = availableBytes / (1024 * 1024);

            if (availableBytes < _degradedDiskSpaceThresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Low disk space: {availableBytes / (1024 * 1024)} MB available on {driveInfo.Name}",
                    data: data));
            }
        }
        catch (Exception ex)
        {
            data["diskCheckError"] = ex.Message;
        }

        // If the database file does not exist yet (first run before migration), the
        // directory must at least be writable so the runtime can create it.
        if (!File.Exists(fullPath))
        {
            try
            {
                Directory.CreateDirectory(directory);
                var probe = Path.Combine(directory, $".write-probe-{Guid.NewGuid():N}");
                File.WriteAllText(probe, string.Empty);
                File.Delete(probe);
                data["fileExists"] = false;
                return Task.FromResult(HealthCheckResult.Healthy(
                    "SQLite database file will be created on first write; directory is writable.",
                    data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"SQLite database directory is not writable: {ex.Message}",
                    ex, data));
            }
        }

        data["fileExists"] = true;

        // Verify the file is readable.
        try
        {
            using var fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"SQLite database file is not readable: {ex.Message}",
                ex, data));
        }

        // Verify the file is writable.
        try
        {
            using var fs = File.Open(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"SQLite database file is read-only: {ex.Message}",
                ex, data));
        }

        return Task.FromResult(HealthCheckResult.Healthy("SQLite database file is accessible and writable.", data));
    }
}
