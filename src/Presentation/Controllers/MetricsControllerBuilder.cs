#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotnetServiceScaffold.Presentation.Controllers
{
    /// <summary>
    /// Builder for <see cref="MetricsSummary"/> objects using fluent syntax.
    /// </summary>
    public class MetricsControllerBuilder
    {
        private DateTime? _timestamp;
        private int? _totalMetrics;
        private int? _counters;
        private int? _gauges;
        private int? _timers;
        private List<string> _categories = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsControllerBuilder"/> class.
        /// </summary>
        public MetricsControllerBuilder()
        {
        }

        /// <summary>
        /// Sets the timestamp for the metrics summary.
        /// </summary>
        /// <param name="timestamp">The timestamp value.</param>
        /// <returns>The same builder instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="timestamp"/> is <see cref="DateTime.MinValue"/>.</exception>
        public MetricsControllerBuilder WithTimestamp(DateTime timestamp)
        {
            if (timestamp == DateTime.MinValue)
            {
                throw new ArgumentException("Timestamp must not be DateTime.MinValue.", nameof(timestamp));
            }

            _timestamp = timestamp;
            return this;
        }

        /// <summary>
        /// Sets the total metrics count.
        /// </summary>
        /// <param name="totalMetrics">The total metrics count.</param>
        /// <returns>The same builder instance for fluent chaining.</returns>
        /// <exception cref="ArgumentException">If <paramref name="totalMetrics"/> is negative.</exception>
        public MetricsControllerBuilder WithTotalMetrics(int totalMetrics)
        {
            if (totalMetrics < 0)
            {
                throw new ArgumentException("TotalMetrics must be non-negative.", nameof(totalMetrics));
            }

            _totalMetrics = totalMetrics;
            return this;
        }

        /// <summary>
        /// Sets the counters count.
        /// </summary>
        /// <param name="counters">The counters count.</param>
        /// <returns>The same builder instance for fluent chaining.</returns>
        /// <exception cref="ArgumentException">If <paramref name="counters"/> is negative.</exception>
        public MetricsControllerBuilder WithCounters(int counters)
        {
            if (counters < 0)
            {
                throw new ArgumentException("Counters must be non-negative.", nameof(counters));
            }

            _counters = counters;
            return this;
        }

        /// <summary>
        /// Sets the gauges count.
        /// </summary>
        /// <param name="gauges">The gauges count.</param>
        /// <returns>The same builder instance for fluent chaining.</returns>
        /// <exception cref="ArgumentException">If <paramref name="gauges"/> is negative.</exception>
        public MetricsControllerBuilder WithGauges(int gauges)
        {
            if (gauges < 0)
            {
                throw new ArgumentException("Gauges must be non-negative.", nameof(gauges));
            }

            _gauges = gauges;
            return this;
        }

        /// <summary>
        /// Sets the timers count.
        /// </summary>
        /// <param name="timers">The timers count.</param>
        /// <returns>The same builder instance for fluent chaining.</returns>
        /// <exception cref="ArgumentException">If <paramref name="timers"/> is negative.</exception>
        public MetricsControllerBuilder WithTimers(int timers)
        {
            if (timers < 0)
            {
                throw new ArgumentException("Timers must be non-negative.", nameof(timers));
            }

            _timers = timers;
            return this;
        }

        /// <summary>
        /// Sets the categories list.
        /// </summary>
        /// <param name="categories">The categories list.</param>
        /// <returns>The same builder instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="categories"/> is <see langword="null"/>.</exception>
        public MetricsControllerBuilder WithCategories(IEnumerable<string> categories)
        {
            ArgumentNullException.ThrowIfNull(categories);

            _categories = new List<string>(categories);
            return this;
        }

        /// <summary>
        /// Builds the <see cref="MetricsSummary"/> instance with the current property values.
        /// </summary>
        /// <returns>A configured <see cref="MetricsSummary"/> instance.</returns>
        /// <exception cref="ArgumentException">If any required property is missing.</exception>
        public MetricsSummary Build()
        {
            if (!_timestamp.HasValue)
            {
                throw new ArgumentException("Timestamp is required.", nameof(_timestamp));
            }

            if (!_totalMetrics.HasValue)
            {
                throw new ArgumentException("TotalMetrics is required.", nameof(_totalMetrics));
            }

            if (!_counters.HasValue)
            {
                throw new ArgumentException("Counters is required.", nameof(_counters));
            }

            if (!_gauges.HasValue)
            {
                throw new ArgumentException("Gauges is required.", nameof(_gauges));
            }

            if (!_timers.HasValue)
            {
                throw new ArgumentException("Timers is required.", nameof(_timers));
            }

            return new MetricsSummary
            {
                Timestamp = _timestamp.Value,
                TotalMetrics = _totalMetrics.Value,
                Counters = _counters.Value,
                Gauges = _gauges.Value,
                Timers = _timers.Value,
                Categories = _categories
            };
        }

        /// <summary>
        /// Creates a new builder pre-filled with values from an existing <see cref="MetricsSummary"/> instance.
        /// </summary>
        /// <param name="template">The template instance to copy values from.</param>
        /// <returns>A new <see cref="MetricsControllerBuilder"/> initialized with the template's values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="template"/> is <see langword="null"/>.</exception>
        public static MetricsControllerBuilder From(MetricsSummary template)
        {
            ArgumentNullException.ThrowIfNull(template);

            return new MetricsControllerBuilder
            {
                _timestamp = template.Timestamp,
                _totalMetrics = template.TotalMetrics,
                _counters = template.Counters,
                _gauges = template.Gauges,
                _timers = template.Timers,
                _categories = new List<string>(template.Categories)
            };
        }
    }
}