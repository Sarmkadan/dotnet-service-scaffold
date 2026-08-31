using Xunit;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DotnetServiceScaffold.Infrastructure.Metrics;

namespace DotnetServiceScaffold.Tests
{
    public class MetricsServiceTests
    {
        private readonly Mock<ILogger<MetricsService>> _mockLogger;
        private readonly MetricsService _sut;

        public MetricsServiceTests()
        {
            _mockLogger = new Mock<ILogger<MetricsService>>();
            _sut = new MetricsService(_mockLogger.Object);
        }

        private ConcurrentDictionary<string, MetricValue> GetMetricsInternal()
        {
            var field = typeof(MetricsService).GetField("_metrics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (ConcurrentDictionary<string, MetricValue>)field.GetValue(_sut)!;
        }

        [Fact]
        public void IncrementCounter_WithValidParameters_IncrementsCounter()
        {
            // Arrange
            var metricName = "test_counter";
            var incrementValue = 5;

            // Act
            _sut.IncrementCounter(metricName, incrementValue);

            // Assert
            var metrics = GetMetricsInternal();
            Assert.True(metrics.TryGetValue(metricName, out var metric));
            Assert.Equal(MetricType.Counter, metric.Type);
            Assert.Equal(incrementValue, metric.Value);
        }

        [Fact]
        public void IncrementCounter_WithNullMetricName_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _sut.IncrementCounter(null!));
        }

        [Fact]
        public void RecordGauge_WithValidParameters_SetsGaugeValue()
        {
            // Arrange
            var metricName = "test_gauge";
            var gaugeValue = 42.5;

            // Act
            _sut.RecordGauge(metricName, gaugeValue);

            // Assert
            var metrics = GetMetricsInternal();
            Assert.True(metrics.TryGetValue(metricName, out var metric));
            Assert.Equal(MetricType.Gauge, metric.Type);
            Assert.Equal(gaugeValue, metric.Value);
        }

        [Fact]
        public void RecordGauge_WithNegativeValue_RecordsNegativeValue()
        {
            // Arrange
            var metricName = "negative_gauge";
            var gaugeValue = -10.0;

            // Act
            _sut.RecordGauge(metricName, gaugeValue);

            // Assert
            var metrics = GetMetricsInternal();
            Assert.True(metrics.TryGetValue(metricName, out var metric));
            Assert.Equal(MetricType.Gauge, metric.Type);
            Assert.Equal(gaugeValue, metric.Value);
        }

        [Fact]
        public void RecordTiming_WithValidParameters_RecordsTiming()
        {
            // Arrange
            var metricName = "test_timing";
            var elapsedMs = 150L;

            // Act
            _sut.RecordTiming(metricName, elapsedMs);

            // Assert
            var metrics = GetMetricsInternal();
            Assert.True(metrics.TryGetValue(metricName, out var metric));
            Assert.Equal(MetricType.Timer, metric.Type);
            Assert.Equal(elapsedMs, metric.Value);
            Assert.Equal(1, metric.Count);
            Assert.Equal(elapsedMs, metric.Min);
            Assert.Equal(elapsedMs, metric.Max);
        }

        [Fact]
        public async Task MeasureAsync_WithSuccessfulOperation_ReturnsResultAndRecordsTiming()
        {
            // Arrange
            var metricName = "test_measure";
            var expectedResult = 42;
            Func<Task<int>> operation = async () =>
            {
                await Task.Yield();
                return expectedResult;
            };

            // Act
            var result = await _sut.MeasureAsync(metricName, operation);

            // Assert
            Assert.Equal(expectedResult, result);
            var metrics = GetMetricsInternal();
            Assert.True(metrics.TryGetValue(metricName, out var metric));
            Assert.Equal(MetricType.Timer, metric.Type);
            Assert.True(metric.Count >= 1);
            Assert.True(metric.Value >= 0);
        }

        [Fact]
        public async Task MeasureAsync_WithThrowingOperation_PropagatesExceptionAndRecordsTiming()
        {
            // Arrange
            var metricName = "test_measure_error";
            Func<Task<int>> operation = async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Test error");
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.MeasureAsync(metricName, operation));
            var metrics = GetMetricsInternal();
            Assert.True(metrics.TryGetValue(metricName, out var metric));
            Assert.Equal(MetricType.Timer, metric.Type);
            Assert.True(metric.Count >= 1);
            Assert.True(metric.Value >= 0);
        }

        [Fact]
        public void RecordHistogram_WithValidParameters_RecordsHistogram()
        {
            // Arrange
            var metricName = "test_histogram";
            var value = 50.0;
            var buckets = new[] { 0.0, 10.0, 20.0, 100.0 };

            // Act
            _sut.RecordHistogram(metricName, value, buckets);

            // Assert
            var metrics = GetMetricsInternal();
            Assert.True(metrics.TryGetValue(metricName, out var metric));
            Assert.Equal(MetricType.Histogram, metric.Type);
            Assert.Equal(value, metric.Value);
            Assert.Equal(1, metric.Count);
            Assert.NotNull(metric.Buckets);
            Assert.Equal(buckets.Length, metric.Buckets.Length);
            for (int i = 0; i < buckets.Length; i++)
            {
                Assert.Equal(buckets[i], metric.Buckets[i]);
            }
        }

        [Fact]
        public void RecordHistogram_WithNullBuckets_ThrowsNullReferenceException()
        {
            // Act & Assert
            // Note: The implementation throws NullReferenceException when buckets is null
            // because it tries to access buckets.Length in the log statement.
            Assert.Throws<NullReferenceException>(() => _sut.RecordHistogram("test", 1.0, null!));
        }

        [Fact]
        public async Task GetMetricsAsync_WithNoMetrics_ReturnsEmptyDictionary()
        {
            // Act
            var metrics = await _sut.GetMetricsAsync();

            // Assert
            Assert.NotNull(metrics);
            Assert.Empty(metrics);
        }

        [Fact]
        public async Task ResetAsync_ResetsAllMetrics()
        {
            // Arrange
            _sut.IncrementCounter("counter1");
            _sut.RecordGauge("gauge1", 10.0);

            // Act
            await _sut.ResetAsync();

            // Assert
            var metrics = GetMetricsInternal();
            Assert.Empty(metrics);
        }
    }
}