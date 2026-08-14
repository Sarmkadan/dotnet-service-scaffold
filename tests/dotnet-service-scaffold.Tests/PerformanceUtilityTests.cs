using System;
using System.Threading.Tasks;
using DotnetServiceScaffold.Shared.Utilities;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class PerformanceUtilityTests
{
    [Fact]
    public void MeasureMs_ShouldReturnNonNegativeValue()
    {
        long elapsed = PerformanceUtility.MeasureMs(() => { /* no work */ });
        Assert.True(elapsed >= 0);
    }

    [Fact]
    public void MeasureMs_NullAction_Throws()
    {
        Assert.Throws<NullReferenceException>(() => PerformanceUtility.MeasureMs(null!));
    }

    [Fact]
    public void MeasureMsT_ShouldReturnResultAndNonNegativeElapsed()
    {
        var (result, elapsed) = PerformanceUtility.MeasureMs(() => 42);
        Assert.Equal(42, result);
        Assert.True(elapsed >= 0);
    }

    [Fact]
    public void MeasureMsT_NullFunc_Throws()
    {
        Assert.Throws<NullReferenceException>(() => PerformanceUtility.MeasureMs<int>(null!));
    }

    [Fact]
    public async Task MeasureMsAsync_ShouldReturnNonNegativeValue()
    {
        long elapsed = await PerformanceUtility.MeasureMsAsync(async () => await Task.CompletedTask);
        Assert.True(elapsed >= 0);
    }

    [Fact]
    public async Task MeasureMsAsync_NullFunc_Throws()
    {
        await Assert.ThrowsAsync<NullReferenceException>(async () => await PerformanceUtility.MeasureMsAsync(null!));
    }

    [Fact]
    public async Task MeasureMsAsyncT_ShouldReturnResultAndNonNegativeElapsed()
    {
        var (result, elapsed) = await PerformanceUtility.MeasureMsAsync(async () => await Task.FromResult("test"));
        Assert.Equal("test", result);
        Assert.True(elapsed >= 0);
    }

    [Fact]
    public async Task MeasureMsAsyncT_NullFunc_Throws()
    {
        await Assert.ThrowsAsync<NullReferenceException>(async () => await PerformanceUtility.MeasureMsAsync<string>(null!));
    }

    [Fact]
    public void GetMemoryUsageMb_ReturnsPositive()
    {
        double usage = PerformanceUtility.GetMemoryUsageMb();
        Assert.True(usage > 0);
    }

    [Fact]
    public void GetMemoryStats_ReturnsNonNegativeValues()
    {
        MemoryStats stats = PerformanceUtility.GetMemoryStats();
        Assert.True(stats.WorkingSetMb >= 0);
        Assert.True(stats.PrivateMemoryMb >= 0);
        Assert.True(stats.PeakWorkingSetMb >= 0);
    }

    [Fact]
    public void GetCpuUsagePercent_ReturnsNonNegative()
    {
        double cpu = PerformanceUtility.GetCpuUsagePercent();
        Assert.True(cpu >= 0);
    }

    [Fact]
    public void GetGcStats_ReturnsNonNegativeCounts()
    {
        GarbageCollectionStats gc = PerformanceUtility.GetGcStats();
        Assert.True(gc.Gen0Collections >= 0);
        Assert.True(gc.Gen1Collections >= 0);
        Assert.True(gc.Gen2Collections >= 0);
        Assert.True(gc.TotalMemoryBytes >= 0);
    }

    [Theory]
    [InlineData(0, "0ms")]
    [InlineData(999, "999ms")]
    [InlineData(1500, "1.5s")]
    [InlineData(59000, "59.0s")]
    [InlineData(120000, "2.0m")]
    [InlineData(7200000, "2.0h")]
    public void FormatElapsedTime_FormatsCorrectly(long ms, string expected)
    {
        string formatted = PerformanceUtility.FormatElapsedTime(ms);
        Assert.Equal(expected, formatted);
    }

    [Theory]
    [InlineData(500, "500.00 B")]
    [InlineData(1024, "1.00 KB")]
    [InlineData(1536, "1.50 KB")]
    [InlineData(5 * 1024 * 1024, "5.00 MB")]
    [InlineData(3L * 1024 * 1024 * 1024, "3.00 GB")]
    public void FormatBytes_FormatsCorrectly(long bytes, string expected)
    {
        string formatted = PerformanceUtility.FormatBytes(bytes);
        Assert.Equal(expected, formatted);
    }
}
