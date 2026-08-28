#nullable enable
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

public interface IMetricsController
{
    Task<IActionResult> GetMetrics();
    Task<IActionResult> GetMetricsByCategory(string category);
    Task<IActionResult> ResetMetrics();
    Task<IActionResult> GetMetricsSummary();
}