#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Interface for structured logging configuration options.
/// </summary>
public interface IStructuredLoggingOptions
{
    /// <summary>Name of the application included in every log entry.</summary>
    string ApplicationName { get; set; }

    /// <summary>Whether to include the machine/host name in every log entry.</summary>
    bool EnrichWithMachineName { get; set; }

    /// <summary>Whether to include the current environment name (Development/Production).</summary>
    bool EnrichWithEnvironment { get; set; }

    /// <summary>Whether to attach a correlation ID to each incoming HTTP request.</summary>
    bool EnableCorrelationId { get; set; }

    /// <summary>HTTP header name used to read or propagate the correlation ID.</summary>
    string CorrelationIdHeader { get; set; }

    /// <summary>Whether to log enriched request context (method, path, status, duration).</summary>
    bool EnrichWithRequestContext { get; set; }

    /// <summary>Minimum log level for the structured logging pipeline override.</summary>
    string MinimumLevel { get; set; }
}