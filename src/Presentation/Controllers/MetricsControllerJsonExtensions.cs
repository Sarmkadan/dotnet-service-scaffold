#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Provides documentation and metadata for the <see cref="MetricsController"/> class.
/// This controller exposes application metrics and performance data through REST API endpoints.
/// </summary>
public static class MetricsControllerJsonExtensions
{
    // This class serves as a marker for JSON serialization metadata and documentation.
    // MetricsController instances should not be serialized directly as they contain
    // controller dependencies and ASP.NET Core infrastructure components.
    //
    // The actual API responses are returned as anonymous types or DTOs from controller methods,
    // not the controller instance itself.
}
