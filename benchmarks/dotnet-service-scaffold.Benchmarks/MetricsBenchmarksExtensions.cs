using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Benchmarks
{
    public static class MetricsBenchmarksExtensions
    {
        public static void ResetMetrics(this MetricsBenchmarks benchmarks)
        {
            benchmarks.Setup();
        }

        public static async Task VerifyMetricsNotEmptyAsync(this MetricsBenchmarks benchmarks)
        {
            var metrics = await benchmarks.GetMetrics();
            if (metrics.Count == 0)
            {
                throw new InvalidOperationException("No metrics recorded.");
            }
        }

        public static async Task AssertMetricExistsAsync(this MetricsBenchmarks benchmarks, string metricName)
        {
            var metrics = await benchmarks.GetMetrics();
            if (!metrics.ContainsKey(metricName))
            {
                throw new KeyNotFoundException($"Metric '{metricName}' not found.");
            }
        }
    }
}
