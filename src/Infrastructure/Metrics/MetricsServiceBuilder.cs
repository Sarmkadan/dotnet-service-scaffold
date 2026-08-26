#nullable enable
using System;

namespace DotnetServiceScaffold.Infrastructure.Metrics
{
    /// <summary>
    /// Builder for creating <see cref="MetricValue"/> instances with fluent interface.
    /// </summary>
    public class MetricsServiceBuilder
    {
        private MetricType _type;
        private double _value;
        private long _count;
        private long _min;
        private long _max;
        private double[]? _buckets;
        private long[]? _bucketCounts;
        private long? _bucketSum;
        private bool _typeSet;
        private bool _valueSet;
        private bool _countSet;
        private bool _minSet;
        private bool _maxSet;
        private bool _bucketsSet;
        private bool _bucketCountsSet;
        private bool _bucketSumSet;

        /// <summary>
        /// Sets the metric type.
        /// </summary>
        /// <param name="type">The metric type.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentException">If <paramref name="type"/> is not a valid <see cref="MetricType"/> value.</exception>
        public MetricsServiceBuilder WithType(MetricType type)
        {
            if (!Enum.IsDefined(typeof(MetricType), type))
            {
                throw new ArgumentException($"Invalid metric type: {type}", nameof(type));
            }

            _type = type;
            _typeSet = true;
            return this;
        }

        /// <summary>
        /// Sets the metric value.
        /// </summary>
        /// <param name="value">The metric value.</param>
        /// <returns>This builder instance.</returns>
        public MetricsServiceBuilder WithValue(double value)
        {
            _value = value;
            _valueSet = true;
            return this;
        }

        /// <summary>
        /// Sets the metric count.
        /// </summary>
        /// <param name="count">The metric count.</param>
        /// <returns>This builder instance.</returns>
        public MetricsServiceBuilder WithCount(long count)
        {
            _count = count;
            _countSet = true;
            return this;
        }

        /// <summary>
        /// Sets the metric minimum value.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <returns>This builder instance.</returns>
        public MetricsServiceBuilder WithMin(long min)
        {
            _min = min;
            _minSet = true;
            return this;
        }

        /// <summary>
        /// Sets the metric maximum value.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <returns>This builder instance.</returns>
        public MetricsServiceBuilder WithMax(long max)
        {
            _max = max;
            _maxSet = true;
            return this;
        }

        /// <summary>
        /// Sets the histogram buckets.
        /// </summary>
        /// <param name="buckets">The bucket boundaries.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentException">If <paramref name="buckets"/> is null or empty.</exception>
        public MetricsServiceBuilder WithBuckets(double[] buckets)
        {
            if (buckets == null)
            {
                throw new ArgumentException("Buckets cannot be null.", nameof(buckets));
            }

            if (buckets.Length == 0)
            {
                throw new ArgumentException("Buckets cannot be empty.", nameof(buckets));
            }

            // Validate that buckets are sorted in ascending order
            for (int i = 1; i < buckets.Length; i++)
            {
                if (buckets[i] <= buckets[i - 1])
                {
                    throw new ArgumentException("Buckets must be sorted in ascending order.", nameof(buckets));
                }
            }

            _buckets = buckets;
            _bucketsSet = true;
            return this;
        }

        /// <summary>
        /// Sets the histogram bucket counts.
        /// </summary>
        /// <param name="bucketCounts">The counts for each bucket.</param>
        /// <returns>This builder instance.</returns>
        /// <exception cref="ArgumentException">If <paramref name="bucketCounts"/> is null or doesn't match buckets length.</exception>
        public MetricsServiceBuilder WithBucketCounts(long[] bucketCounts)
        {
            if (bucketCounts == null)
            {
                throw new ArgumentException("Bucket counts cannot be null.", nameof(bucketCounts));
            }

            if (_bucketsSet && bucketCounts.Length != _buckets.Length)
            {
                throw new ArgumentException("Bucket counts length must match buckets length.", nameof(bucketCounts));
            }

            _bucketCounts = bucketCounts;
            _bucketCountsSet = true;
            return this;
        }

        /// <summary>
        /// Sets the histogram bucket sum.
        /// </summary>
        /// <param name="bucketSum">The sum of values in buckets.</param>
        /// <returns>This builder instance.</returns>
        public MetricsServiceBuilder WithBucketSum(long? bucketSum)
        {
            _bucketSum = bucketSum;
            _bucketSumSet = true;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="MetricValue"/> instance with the configured properties.
        /// </summary>
        /// <returns>A configured <see cref="MetricValue"/> instance.</returns>
        /// <exception cref="ArgumentException">If required properties are missing for the specified metric type.</exception>
        public MetricValue Build()
        {
            if (!_typeSet)
            {
                throw new ArgumentException("Metric type must be set.", nameof(_type));
            }

            if (!_valueSet)
            {
                throw new ArgumentException("Metric value must be set.", nameof(_value));
            }

            var metric = new MetricValue
            {
                Type = _type,
                Value = _value
            };

            // Set properties based on metric type
            switch (_type)
            {
                case MetricType.Counter:
                    // Counter only uses Value; Count, Min, Max are not used
                    if (_countSet || _minSet || _maxSet || _bucketsSet || _bucketCountsSet || _bucketSumSet)
                    {
                        throw new ArgumentException("Counter metrics should not set Count, Min, Max, Buckets, BucketCounts, or BucketSum.");
                    }
                    break;
                case MetricType.Gauge:
                    // Gauge only uses Value; Count, Min, Max are not used
                    if (_countSet || _minSet || _maxSet || _bucketsSet || _bucketCountsSet || _bucketSumSet)
                    {
                        throw new ArgumentException("Gauge metrics should not set Count, Min, Max, Buckets, BucketCounts, or BucketSum.");
                    }
                    break;
                case MetricType.Timer:
                    // Timer requires Count, Min, Max
                    if (!_countSet)
                    {
                        throw new ArgumentException("Timer metric requires Count to be set.", nameof(_count));
                    }
                    if (!_minSet)
                    {
                        throw new ArgumentException("Timer metric requires Min to be set.", nameof(_min));
                    }
                    if (!_maxSet)
                    {
                        throw new ArgumentException("Timer metric requires Max to be set.", nameof(_max));
                    }
                    metric.Count = _count;
                    metric.Min = _min;
                    metric.Max = _max;
                    // Timer should not set Buckets, BucketCounts, BucketSum
                    if (_bucketsSet || _bucketCountsSet || _bucketSumSet)
                    {
                        throw new ArgumentException("Timer metrics should not set Buckets, BucketCounts, or BucketSum.");
                    }
                    break;
                case MetricType.Histogram:
                    // Histogram requires Buckets
                    if (!_bucketsSet)
                    {
                        throw new ArgumentException("Histogram metric requires Buckets to be set.", nameof(_buckets));
                    }
                    metric.Buckets = _buckets;
                    // If BucketCounts is set, use it; otherwise, we'll leave it null (will be set on first update)
                    if (_bucketCountsSet)
                    {
                        metric.BucketCounts = _bucketCounts;
                    }
                    // If BucketSum is set, use it
                    if (_bucketSumSet)
                    {
                        metric.BucketSum = _bucketSum;
                    }
                    // Histogram should not set Min, Max
                    if (_minSet || _maxSet)
                    {
                        throw new ArgumentException("Histogram metrics should not set Min or Max.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_type), _type, "Unsupported metric type.");
            }

            return metric;
        }

        /// <summary>
        /// Creates a builder pre-filled with properties from an existing <see cref="MetricValue"/> instance.
        /// </summary>
        /// <param name="template">The metric value to copy properties from.</param>
        /// <returns>A builder initialized with the template's values.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="template"/> is null.</exception>
        public static MetricsServiceBuilder From(MetricValue template)
        {
            ArgumentNullException.ThrowIfNull(template);

            return new MetricsServiceBuilder
            {
                _type = template.Type,
                _value = template.Value,
                _count = template.Count,
                _min = template.Min,
                _max = template.Max,
                _buckets = template.Buckets,
                _bucketCounts = template.BucketCounts,
                _bucketSum = template.BucketSum,
                _typeSet = true,
                _valueSet = true,
                _countSet = template.Count != 0 || template.Type == MetricType.Timer || template.Type == MetricType.Histogram, // Assume set if non-zero or type requires it
                _minSet = template.Type == MetricType.Timer, // Timer sets Min/Max
                _maxSet = template.Type == MetricType.Timer,
                _bucketsSet = template.Buckets != null,
                _bucketCountsSet = template.BucketCounts != null,
                _bucketSumSet = template.BucketSum.HasValue
            };
        }
    }
}