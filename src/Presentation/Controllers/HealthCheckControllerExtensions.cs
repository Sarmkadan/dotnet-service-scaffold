#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Extension methods for <see cref="HealthCheckController"/> that provide additional functionality
/// for health check management and service monitoring.
/// </summary>
public static class HealthCheckControllerExtensions
{
    /// <summary>
    /// Performs a health check with a timeout and returns a standardized response.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <param name="timeoutSeconds">Maximum time to wait for the health check in seconds.</param>
    /// <returns>An IActionResult with the health check result.</returns>
    public static async Task<IActionResult> CheckServiceHealthWithTimeout(
        this HealthCheckController controller,
        Guid serviceId,
        int timeoutSeconds = 30)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            var result = await controller.CheckServiceHealth(serviceId);

            if (result is OkObjectResult okResult)
            {
                return controller.Ok(new
                {
                    success = true,
                    timeout = timeoutSeconds,
                    data = okResult.Value
                });
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return controller.StatusCode(504, new
            {
                success = false,
                error = "Health check timed out",
                timeout = timeoutSeconds,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Retrieves health history with filtering capabilities.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <param name="count">Maximum number of records to return.</param>
    /// <param name="statusFilter">Optional status to filter results by.</param>
    /// <param name="fromDate">Optional start date for filtering.</param>
    /// <returns>An IActionResult with filtered health history.</returns>
    public static async Task<IActionResult> GetHealthHistoryFiltered(
        this HealthCheckController controller,
        Guid serviceId,
        int count = 20,
        string? statusFilter = null,
        DateTime? fromDate = null)
    {
        var result = await controller.GetHealthHistory(serviceId, count);

        if (result is OkObjectResult okResult)
        {
            var response = (dynamic)okResult.Value!;
            var history = ((IEnumerable<dynamic>)response.data!).ToList();

            var filteredHistory = history.AsEnumerable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                filteredHistory = filteredHistory.Where(h =>
                    string.Equals(h.Status?.ToString(), statusFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (fromDate.HasValue)
            {
                filteredHistory = filteredHistory.Where(h =>
                    (DateTime)h.CheckedAt >= fromDate.Value);
            }

            var filteredCount = filteredHistory.Count();

            return controller.Ok(new
            {
                success = true,
                originalCount = (int)response.count,
                filteredCount,
                filter = new
                {
                    status = statusFilter,
                    fromDate
                },
                data = filteredHistory.ToList()
            });
        }

        return result;
    }

    /// <summary>
    /// Gets comprehensive health status including metrics and recent history.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <param name="historyCount">Number of recent checks to include in metrics.</param>
    /// <returns>An IActionResult with comprehensive health metrics.</returns>
    public static async Task<IActionResult> GetComprehensiveHealthStatus(
        this HealthCheckController controller,
        Guid serviceId,
        int historyCount = 10)
    {
        var statusResult = await controller.GetHealthStatus(serviceId);
        var historyResult = await controller.GetHealthHistory(serviceId, historyCount);

        if (statusResult is OkObjectResult statusOk && historyResult is OkObjectResult historyOk)
        {
            var statusResponse = (dynamic)statusOk.Value;
            var historyResponse = (dynamic)historyOk.Value;
            var historyData = ((IEnumerable<dynamic>)historyResponse.data).ToList();

            var successChecks = historyData.Count(h =>
                h.Status?.ToString() == "Healthy");
            var failureChecks = historyData.Count(h =>
                h.Status?.ToString() != "Healthy");
            var avgResponseTime = historyData.Any()
                ? historyData.Average(h => (int)h.ResponseTimeMs)
                : 0;

            return controller.Ok(new
            {
                success = true,
                serviceId,
                timestamp = DateTime.UtcNow,
                status = statusResponse,
                metrics = new
                {
                    totalChecks = historyCount,
                    successfulChecks = successChecks,
                    failedChecks = failureChecks,
                    successRate = historyCount > 0
                        ? (successChecks * 100.0 / historyCount)
                        : 0.0,
                    averageResponseTimeMs = avgResponseTime,
                    recentChecks = historyData.Take(historyCount).ToList()
                }
            });
        }

        return statusResult;
    }

    /// <summary>
    /// Retrieves failed health checks grouped by error type with statistics.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="serviceId">The service identifier.</param>
    /// <param name="hoursBack">Time window in hours to look back.</param>
    /// <returns>An IActionResult with grouped failure statistics.</returns>
    public static async Task<IActionResult> GetFailedChecksGrouped(
        this HealthCheckController controller,
        Guid serviceId,
        int hoursBack = 24)
    {
        var result = await controller.GetFailedChecks(serviceId, hoursBack);

        if (result is OkObjectResult okResult)
        {
            var response = (dynamic)okResult.Value!;
            var failures = ((IEnumerable<dynamic>)response.data!).ToList();
            var failureCount = (int)response.count!;

            var errorGroups = failures
                .GroupBy(f => f.ErrorMessage?.ToString() ?? "Unknown error")
                .Select(g => new
                {
                    errorType = g.Key,
                    count = g.Count(),
                    percentage = failureCount > 0
                        ? (g.Count() * 100.0 / failureCount)
                        : 0.0,
                    firstOccurrence = g.Min(f => (DateTime)f.CheckedAt),
                    lastOccurrence = g.Max(f => (DateTime)f.CheckedAt),
                    sampleErrors = g.Select(f => new
                    {
                        timestamp = (DateTime)f.CheckedAt,
                        responseTime = (int)f.ResponseTimeMs,
                        error = f.ErrorMessage?.ToString()
                    })
                    .OrderByDescending(e => e.timestamp)
                    .Take(3)
                    .ToList()
                })
                .OrderByDescending(g => g.count)
                .ToList();

            return controller.Ok(new
            {
                success = true,
                serviceId,
                timeWindowHours = hoursBack,
                totalFailures = failureCount,
                errorGroups,
                summary = new
                {
                    mostCommonError = errorGroups.FirstOrDefault()?.errorType,
                    failureRate = failureCount > 0
                        ? $"{(failureCount * 100.0 / Math.Max(1, failureCount)):F2}%"
                        : "0.00%"
                }
            });
        }

        return result;
    }
}