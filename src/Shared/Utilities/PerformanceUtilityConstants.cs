#nullable enable

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Constants for PerformanceUtility class.
/// </summary>
internal static class PerformanceUtilityConstants
{
    /// <summary>
    /// Number of milliseconds in a second.
    /// </>
    public const double MillisecondsPerSecond = 1000.0;

    /// <summary>
    /// Number of milliseconds in a minute.
    /// </summary>
    public const double MillisecondsPerMinute = 60000.0;

    /// <summary>
    /// Number of milliseconds in an hour.
    /// </summary>
    public const double MillisecondsPerHour = 3600000.0;

    /// <summary>
    /// Number of bytes in a kilobyte.
    /// </summary>
    public const int BytesPerKilobyte = 1024;

    /// <summary>
    /// Number of bytes in a megabyte.
    /// </summary>
    public const double BytesPerMegabyte = 1048576.0;

    /// <summary>
    /// Default maximum number of retry attempts.
    /// </summary>
    public const int DefaultMaxRetryAttempts = 3;

    /// <summary>
    /// Initial delay in milliseconds for retry backoff.
    /// </summary>
    public const int DefaultInitialDelayMs = 100;

    /// <summary>
    /// Base multiplier for exponential backoff.
    /// </summary>
    public const int BackoffBaseMultiplier = 2;

    /// <summary>
    /// Percentage multiplier for CPU usage calculation.
    /// </summary>
    public const int PercentageMultiplier = 100;
}