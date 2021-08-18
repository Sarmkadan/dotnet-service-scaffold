using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Benchmarks
{
    /// <summary>
    /// Provides extension methods for the MetricsBenchmarks class.
    /// </summary>
    public static class MetricsBenchmarksExtensions
    {
        /// <summary>
        /// Resets the metrics for the MetricsBenchmarks instance.
        /// </summary>
        /// <param name="benchmarks">The MetricsBenchmarks instance to reset.</param>
        public static void ResetMetrics(this MetricsBenchmarks benchmarks)
        {
            benchmarks.Setup();
        }

        /// <summary>
        /// Verifies that the metrics recorded by the MetricsBenchmarks instance are not empty.
        /// </summary>
        /// <param name="benchmarks">The MetricsBenchmarks instance to verify.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task VerifyMetricsNotEmptyAsync(this MetricsBenchmarks benchmarks)
        {
            var metrics = await benchmarks.GetMetrics();
            if (metrics.Count == 0)
            {
                throw new InvalidOperationException("No metrics recorded.");
            }
        }

        /// <summary>
        /// Asserts that a specific metric exists in the metrics recorded by the MetricsBenchmarks instance.
        /// </summary>
        /// <param name="benchmarks">The MetricsBenchmarks instance to verify.</param>
        /// <param name="metricName">The name of the metric to assert exists.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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
