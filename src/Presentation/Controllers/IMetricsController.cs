#nullable enable
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

public interface IMetricsController
{
    Task<IActionResult> GetMetrics(CancellationToken cancellationToken = default);
    Task<IActionResult> GetMetricsByCategory(string category, CancellationToken cancellationToken = default);
    Task<IActionResult> ResetMetrics(CancellationToken cancellationToken = default);
    Task<IActionResult> GetMetricsSummary(CancellationToken cancellationToken = default);
}