using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Benchmarks
{
    /// <summary>
    /// Provides extension methods for the <see cref="MetricsBenchmarks"/> class to simplify benchmark setup, validation, and metric assertions.
    /// </summary>
    public static class MetricsBenchmarksExtensions
    {
        /// <summary>
        /// Resets the metrics benchmarks instance by re-running the setup.
        /// </summary>
        /// <param name="benchmarks">The <see cref="MetricsBenchmarks"/> instance to reset.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        public static void ResetMetrics(this MetricsBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
        }

        /// <summary>
        /// Verifies that the metrics recorded by the <see cref="MetricsBenchmarks"/> instance are not empty.
        /// </summary>
        /// <param name="benchmarks">The <see cref="MetricsBenchmarks"/> instance to verify.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no metrics are recorded.</exception>
        public static async Task VerifyMetricsNotEmptyAsync(this MetricsBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            var metrics = await benchmarks.GetMetrics();
            if (metrics.Count == 0)
            {
                throw new InvalidOperationException("No metrics recorded.");
            }
        }

        /// <summary>
        /// Asserts that a specific metric exists in the metrics recorded by the <see cref="MetricsBenchmarks"/> instance.
        /// </summary>
        /// <param name="benchmarks">The <see cref="MetricsBenchmarks"/> instance to verify.</param>
        /// <param name="metricName">The name of the metric to assert exists.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="metricName"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the metric does not exist.</exception>
        public static async Task AssertMetricExistsAsync(this MetricsBenchmarks benchmarks, string metricName)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentNullException.ThrowIfNull(metricName);

            var metrics = await benchmarks.GetMetrics();
            if (!metrics.ContainsKey(metricName))
            {
                throw new KeyNotFoundException($"Metric '{metricName}' not found.");
            }
        }
    }
}
