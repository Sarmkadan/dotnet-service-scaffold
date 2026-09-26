#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DotnetServiceScaffold.Infrastructure.HealthChecks;

/// <summary>
/// Health check that verifies the SQLite database file is accessible and writable,
/// and reports available disk space as a degraded warning when running low.
/// </summary>
public class SqliteHealthCheck : SafeHealthCheckBase
{
    private readonly string _databasePath;
    private readonly long _degradedDiskSpaceThresholdBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging exception details.</param>
    /// <param name="databasePath">Absolute or relative path to the SQLite database file.</param>
    /// <param name="degradedDiskSpaceThresholdBytes">
    /// Available disk space below which the check reports Degraded. Defaults to 512 MB.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="databasePath"/> is <see langword="null"/> or empty.
    /// </exception>
    public SqliteHealthCheck(
        ILogger<SqliteHealthCheck> logger,
        string databasePath,
        long degradedDiskSpaceThresholdBytes = SqliteHealthCheckConstants.DefaultDegradedDiskSpaceThresholdBytes)
        : base(logger)
    {
        ArgumentException.ThrowIfNullOrEmpty(databasePath);
        _databasePath = databasePath;
        _degradedDiskSpaceThresholdBytes = degradedDiskSpaceThresholdBytes;
    }

    /// <summary>
    /// Checks whether the SQLite database file is accessible and writable and whether its volume has sufficient free space.
    /// </summary>
    /// <param name="context">The context in which the health check is performed.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the health check.</param>
    /// <returns>A task containing the SQLite database health status and diagnostic details.</returns>
    protected override async Task<HealthCheckResult> CheckHealthInternalAsync(
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

        // If the database file does not exist yet (first run before migration), the
        // directory must at least be writable so the runtime can create it.
        if (!File.Exists(fullPath))
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

        data[SqliteHealthCheckConstants.FileExistsKey] = true;

        // Verify the file is readable.
        await using var fsRead = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: SqliteHealthCheckConstants.FileStreamBufferSize, useAsync: true);

        // Verify the file is writable.
        await using var fsWrite = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize: SqliteHealthCheckConstants.FileStreamBufferSize, useAsync: true);

        return HealthCheckResult.Healthy("SQLite database file is accessible and writable.", data);
    }
}