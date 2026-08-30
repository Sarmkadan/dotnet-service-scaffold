#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

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
    public SqliteHealthCheck(string databasePath, long degradedDiskSpaceThresholdBytes = SqliteHealthCheckConstants.DefaultDegradedDiskSpaceThresholdBytes)
    {
        ArgumentException.ThrowIfNullOrEmpty(databasePath);
        _databasePath = databasePath;
        _degradedDiskSpaceThresholdBytes = degradedDiskSpaceThresholdBytes;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Enforce timeout to prevent hung SQLite operations from stalling health checks
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(SqliteHealthCheckConstants.TimeoutSeconds));

        var data = new Dictionary<string, object>();

        // Resolve the full path so diagnostics show an unambiguous location.
        var fullPath = Path.GetFullPath(_databasePath);
        data["databasePath"] = fullPath;

        // Check disk space on the volume that holds the database directory.
        var directory = Path.GetDirectoryName(fullPath) ?? SqliteHealthCheckConstants.CurrentDirectoryIndicator;
        try
        {
            var driveInfo = new DriveInfo(directory);
            var availableBytes = driveInfo.AvailableFreeSpace;
            data["diskAvailableBytes"] = availableBytes;
            data["diskAvailableMB"] = availableBytes / SqliteHealthCheckConstants.BytesPerMebibyte;

            if (availableBytes < _degradedDiskSpaceThresholdBytes)
            {
                return HealthCheckResult.Degraded(
                    $"Low disk space: {availableBytes / SqliteHealthCheckConstants.BytesPerMebibyte} MB available on {driveInfo.Name}",
                    data: data);
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
                await File.WriteAllTextAsync(probe, string.Empty, timeoutCts.Token);
                File.Delete(probe);
                data[SqliteHealthCheckConstants.FileExistsKey] = false;
                return HealthCheckResult.Healthy(
                    "SQLite database file will be created on first write; directory is writable.",
                    data);
            }
            catch (OperationCanceledException)
            {
                return HealthCheckResult.Degraded(
                    "Timeout while checking SQLite database directory writability.",
                    data: data);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    $"SQLite database directory is not writable: {ex.Message}",
                    ex, data);
            }
        }

        data[SqliteHealthCheckConstants.FileExistsKey] = true;

        // Verify the file is readable.
        try
        {
            await using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: SqliteHealthCheckConstants.FileStreamBufferSize, useAsync: true);
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded(
                "Timeout while checking SQLite database file readability.",
                data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"SQLite database file is not readable: {ex.Message}",
                ex, data);
        }

        // Verify the file is writable.
        try
        {
            await using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize: SqliteHealthCheckConstants.FileStreamBufferSize, useAsync: true);
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded(
                "Timeout while checking SQLite database file writability.",
                data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(
                $"SQLite database file is read-only: {ex.Message}",
                ex, data);
        }

        return HealthCheckResult.Healthy("SQLite database file is accessible and writable.", data);
    }
}
