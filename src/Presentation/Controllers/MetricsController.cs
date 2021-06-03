// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Metrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// API controller for exposing application metrics and performance data.
/// Provides endpoints for monitoring application health and performance characteristics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(
        IMetricsService metricsService,
        ILogger<MetricsController> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all recorded metrics.
    /// </summary>
    /// <response code="200">Returns all metrics</response>
    /// <response code="401">If not authenticated</response>
    [HttpGet]
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            var metrics = await _metricsService.GetMetricsAsync();

            _logger.LogDebug("Retrieved metrics ({MetricCount} metrics)", metrics.Count);

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                metricCount = metrics.Count,
                metrics = metrics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics");
            return StatusCode(500, new { error = "Failed to retrieve metrics" });
        }
    }

    /// <summary>
    /// Gets metrics for a specific category.
    /// </summary>
    /// <param name="category">The metric category to filter by (e.g., "http", "database", "cache")</param>
    /// <response code="200">Returns filtered metrics</response>
    /// <response code="401">If not authenticated</response>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetMetricsByCategory(string category)
    {
        try
        {
            var allMetrics = await _metricsService.GetMetricsAsync();

            // Filter metrics by category
            var filtered = allMetrics
                .Where(kvp => kvp.Key.StartsWith(category, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            _logger.LogDebug(
                "Retrieved metrics for category '{Category}' ({MetricCount} metrics)",
                category, filtered.Count);

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                category = category,
                metricCount = filtered.Count,
                metrics = filtered
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics for category '{Category}'", category);
            return StatusCode(500, new { error = "Failed to retrieve metrics" });
        }
    }

    /// <summary>
    /// Resets all metrics to zero.
    /// </summary>
    /// <response code="204">If successfully reset</response>
    /// <response code="401">If not authenticated</response>
    [HttpPost("reset")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetMetrics()
    {
        try
        {
            await _metricsService.ResetAsync();

            _logger.LogInformation("Metrics reset by user {UserId}",
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting metrics");
            return StatusCode(500, new { error = "Failed to reset metrics" });
        }
    }

    /// <summary>
    /// Gets summary statistics of all metrics.
    /// </summary>
    /// <response code="200">Returns metrics summary</response>
    /// <response code="401">If not authenticated</response>
    [HttpGet("summary")]
    public async Task<IActionResult> GetMetricsSummary()
    {
        try
        {
            var metrics = await _metricsService.GetMetricsAsync();

            var summary = new MetricsSummary
            {
                Timestamp = DateTime.UtcNow,
                TotalMetrics = metrics.Count,
                Counters = CountMetricsOfType(metrics, "counter"),
                Gauges = CountMetricsOfType(metrics, "gauge"),
                Timers = CountMetricsOfType(metrics, "timer"),
                Categories = ExtractCategories(metrics)
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics summary");
            return StatusCode(500, new { error = "Failed to retrieve metrics summary" });
        }
    }

    /// <summary>
    /// Counts metrics of a specific type.
    /// </summary>
    private int CountMetricsOfType(Dictionary<string, object> metrics, string type)
    {
        return metrics.Count(kvp =>
        {
            var value = kvp.Value as System.Collections.IDictionary;
            return value?["type"]?.ToString() == type;
        });
    }

    /// <summary>
    /// Extracts unique metric categories.
    /// </summary>
    private List<string> ExtractCategories(Dictionary<string, object> metrics)
    {
        return metrics.Keys
            .Select(key => key.Split('[')[0].Split('.')[0])
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }
}

/// <summary>
/// Summary of metrics statistics.
/// </summary>
public class MetricsSummary
{
    public DateTime Timestamp { get; set; }
    public int TotalMetrics { get; set; }
    public int Counters { get; set; }
    public int Gauges { get; set; }
    public int Timers { get; set; }
    public List<string> Categories { get; set; } = new();
}
