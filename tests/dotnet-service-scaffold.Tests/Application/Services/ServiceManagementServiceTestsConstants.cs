#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetServiceScaffold.Tests.Application.Services;

/// <summary>
/// Contains constant values used in ServiceManagementServiceTests.
/// </summary>
internal static class ServiceManagementServiceTestsConstants
{
    /// <summary>
    /// Test username used across multiple tests.
    /// </summary>
    public const string TestUserName = "testuser";

    /// <summary>
    /// Format string for audit log when a service is registered.
    /// </summary>
    public const string RegisteredServiceLogFormat = "Registered service {0}";

    /// <summary>
    /// Format string for audit log when a service is unregistered.
    /// </summary>
    public const string UnregisteredServiceLogFormat = "Unregistered service {0}";

    /// <summary>
    /// Format string for audit log when a service is disabled.
    /// </summary>
    public const string DisabledServiceLogFormat = "Disabled service: {0}";

    /// <summary>
    /// Audit log message when a service is re-enabled.
    /// </summary>
    public const string ReenabledServiceLogMessage = "Re-enabled service";

    /// <summary>
    /// Total requests for success rate test with metrics.
    /// </summary>
    public const int SuccessRateTest_TotalRequests = 100;

    /// <summary>
    /// Successful requests for success rate test with metrics.
    /// </summary>
    public const int SuccessRateTest_SuccessfulRequests = 90;

    /// <summary>
    /// Total requests for success rate test with no metrics.
    /// </summary>
    public const int NoMetricsTest_TotalRequests = 0;

    /// <summary>
    /// Successful requests for success rate test with no metrics.
    /// </summary>
    public const int NoMetricsTest_SuccessfulRequests = 0;
}