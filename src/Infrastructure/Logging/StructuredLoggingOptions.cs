#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Configuration options for the structured logging pipeline.
/// Bound from the "StructuredLogging" section in appsettings.json.
/// </summary>
public sealed class StructuredLoggingOptions
{
    /// <summary>Name of the application included in every log entry.</summary>
    public string ApplicationName { get; set; } = "DotnetServiceScaffold";

    /// <summary>Whether to include the machine/host name in every log entry.</summary>
    public bool EnrichWithMachineName { get; set; } = true;

    /// <summary>Whether to include the current environment name (Development/Production).</summary>
    public bool EnrichWithEnvironment { get; set; } = true;

    /// <summary>Whether to attach a correlation ID to each incoming HTTP request.</summary>
    public bool EnableCorrelationId { get; set; } = true;

    /// <summary>HTTP header name used to read or propagate the correlation ID.</summary>
    public string CorrelationIdHeader { get; set; } = "X-Correlation-Id";

    /// <summary>Whether to log enriched request context (method, path, status, duration).</summary>
    public bool EnrichWithRequestContext { get; set; } = true;

    /// <summary>Minimum log level for the structured logging pipeline override.</summary>
    public string MinimumLevel { get; set; } = "Information";
}
