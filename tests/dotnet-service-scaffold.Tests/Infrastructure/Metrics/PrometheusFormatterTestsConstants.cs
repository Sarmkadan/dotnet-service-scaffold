#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetServiceScaffold.Tests.Infrastructure.Metrics
{
    /// <summary>
    /// Constants for PrometheusFormatterTests to avoid magic values.
    /// </summary>
    internal static class PrometheusFormatterTestsConstants
    {
        public const string AppPrefix = "app";
        public const string CounterType = "counter";
        public const string GaugeType = "gauge";
        public const string TimerType = "timer";
        public const string HttpRequestsKey = "http.requests";
        public const string MemoryUsedKey = "memory.used";
        public const string DbQueryKey = "db.query";
        public const string SomeMetricPathKey = "some-metric.path";
        public const string TaggedHttpRequestsKey = "http.requests[method=GET,status=200]";
        public const string MethodGetTag = "method=\"GET\"";
        public const string Status200Tag = "status=\"200\"";
        public const string SvcPrefix = "svc";
    }
}