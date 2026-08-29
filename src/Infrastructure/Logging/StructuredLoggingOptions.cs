#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Configuration options for the structured logging pipeline.
/// Bound from the "StructuredLogging" section in appsettings.json.
/// </summary>
public sealed class StructuredLoggingOptions : IStructuredLoggingOptions, IEquatable<StructuredLoggingOptions>
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

    /// <summary>
    /// Validates the configuration options ensuring required fields are set and have valid values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when any required property is null, empty, or has an invalid value.
    /// </exception>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrEmpty(ApplicationName, nameof(ApplicationName));
        ArgumentException.ThrowIfNullOrEmpty(CorrelationIdHeader, nameof(CorrelationIdHeader));
        ArgumentException.ThrowIfNullOrEmpty(MinimumLevel, nameof(MinimumLevel));

        if (!Enum.TryParse<LogLevel>(MinimumLevel, true, out _))
        {
            throw new ArgumentException($"'{MinimumLevel}' is not a valid log level.", nameof(MinimumLevel));
        }
    }

    public bool Equals(StructuredLoggingOptions? other)
    {
        if (other is null) return false;
        return ApplicationName == other.ApplicationName
            && EnrichWithMachineName == other.EnrichWithMachineName
            && EnrichWithEnvironment == other.EnrichWithEnvironment
            && EnableCorrelationId == other.EnableCorrelationId
            && CorrelationIdHeader == other.CorrelationIdHeader
            && EnrichWithRequestContext == other.EnrichWithRequestContext
            && MinimumLevel == other.MinimumLevel;
    }

    public override bool Equals(object? obj) => Equals(obj as StructuredLoggingOptions);

    public override int GetHashCode()
    {
        return HashCode.Combine(ApplicationName, EnrichWithMachineName, EnrichWithEnvironment, EnableCorrelationId, CorrelationIdHeader, EnrichWithRequestContext, MinimumLevel);
    }

    public static bool operator ==(StructuredLoggingOptions? left, StructuredLoggingOptions? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(StructuredLoggingOptions? left, StructuredLoggingOptions? right)
    {
        return !(left == right);
    }
}
