#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Utility class for performance measurement and optimization.
/// Provides helpers for timing operations, memory monitoring, and performance analysis.
/// </summary>
public static class PerformanceUtility
{
    /// <summary>
    /// Measures the execution time of an action.
    /// Returns the elapsed time in milliseconds.
    /// </summary>
    public static long MeasureMs(Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// Measures the execution time of a function and returns its result.
    /// </summary>
    public static (T Result, long ElapsedMs) MeasureMs<T>(Func<T> func)
    {
        var sw = Stopwatch.StartNew();
        var result = func();
        sw.Stop();
        return (result, sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Async version: measures the execution time of an async operation.
    /// </summary>
    public static async Task<long> MeasureMsAsync(Func<Task> func)
    {
        var sw = Stopwatch.StartNew();
        await func();
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// Async version: measures the execution time and returns the result.
    /// </summary>
    public static async Task<(T Result, long ElapsedMs)> MeasureMsAsync<T>(Func<Task<T>> func)
    {
        var sw = Stopwatch.StartNew();
        var result = await func();
        sw.Stop();
        return (result, sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Gets the current process memory usage in MB.
    /// </summary>
    public static double GetMemoryUsageMb()
    {
        using (var process = Process.GetCurrentProcess())
        {
            return process.WorkingSet64 / PerformanceUtilityConstants.BytesPerMegabyte;
        }
    }

    /// <summary>
    /// Gets memory statistics for the current process.
    /// </summary>
    public static MemoryStats GetMemoryStats()
    {
        using (var process = Process.GetCurrentProcess())
        {
            return new MemoryStats
            {
                WorkingSetMb = process.WorkingSet64 / PerformanceUtilityConstants.BytesPerMegabyte,
                PrivateMemoryMb = process.PrivateMemorySize64 / PerformanceUtilityConstants.BytesPerMegabyte,
                PeakWorkingSetMb = process.PeakWorkingSet64 / PerformanceUtilityConstants.BytesPerMegabyte
            };
        }
    }

    /// <summary>
    /// Gets CPU usage percentage for the current process.
    /// Requires a baseline measurement due to how Windows reports CPU usage.
    /// </summary>
    public static double GetCpuUsagePercent()
    {
        using (var process = Process.GetCurrentProcess())
        {
            var cpuUsage = process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount;
            return (cpuUsage / Environment.TickCount) * PerformanceUtilityConstants.PercentageMultiplier;
        }
    }

    /// <summary>
    /// Gets the garbage collection statistics.
    /// </summary>
    public static GarbageCollectionStats GetGcStats()
    {
        return new GarbageCollectionStats
        {
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            TotalMemoryBytes = GC.GetTotalMemory(false)
        };
    }

    /// <summary>
    /// Formats elapsed time in a human-readable format.
    /// Example: "1.5s", "123ms", "2.1h"
    /// </summary>
    public static string FormatElapsedTime(long milliseconds)
    {
        if (milliseconds < PerformanceUtilityConstants.MillisecondsPerSecond)
            return $"{milliseconds}ms";

        if (milliseconds < PerformanceUtilityConstants.MillisecondsPerMinute)
            return $"{milliseconds / PerformanceUtilityConstants.MillisecondsPerSecond:F1}s";

        if (milliseconds < PerformanceUtilityConstants.MillisecondsPerHour)
            return $"{milliseconds / PerformanceUtilityConstants.MillisecondsPerMinute:F1}m";

        return $"{milliseconds / PerformanceUtilityConstants.MillisecondsPerHour:F1}h";
    }

    /// <summary>
    /// Formats size in bytes to human-readable format.
    /// Example: "1.5 MB", "256 KB", "1.2 GB"
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }

    /// <summary>
    /// Retries an action with exponential backoff.
    /// </summary>
    public static async Task<T> RetryWithBackoffAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts = PerformanceUtilityConstants.DefaultMaxRetryAttempts,
        int initialDelayMs = PerformanceUtilityConstants.DefaultInitialDelayMs)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                var delayMs = initialDelayMs * (int)Math.Pow(PerformanceUtilityConstants.BackoffBaseMultiplier, attempt - 1);
                await Task.Delay(delayMs);
            }
        }

        // Final attempt - let exception propagate
        return await operation();
    }
}

/// <summary>
/// Memory usage statistics.
/// </summary>
public class MemoryStats
{
    public double WorkingSetMb { get; set; }
    public double PrivateMemoryMb { get; set; }
    public double PeakWorkingSetMb { get; set; }
}

/// <summary>
/// Garbage collection statistics.
/// </summary>
public class GarbageCollectionStats
{
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public long TotalMemoryBytes { get; set; }
}
