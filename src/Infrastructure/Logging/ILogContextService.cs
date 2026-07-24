#nullable enable

using System.Diagnostics;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Provides scoped log context management for enriching structured log entries
/// with request-specific properties such as correlation IDs and user identity.
/// Uses <see cref="AsyncLocal{T}"/> to ensure proper flow across async boundaries.
/// </summary>
public interface ILogContextService
{
    /// <summary>
    /// Gets or sets the current activity ID from <see cref="Activity.Current"/>.
    /// </summary>
    string? ActivityId { get; set; }

    /// <summary>
    /// Gets or sets the W3C traceparent header value (trace-id:parent-id:span-id:flags).
    /// </summary>
    string? TraceParent { get; set; }

    /// <summary>Gets or sets the correlation ID for the current request.</summary>
    string? CorrelationId { get; set; }

    /// <summary>Gets or sets the authenticated user ID for the current request.</summary>
    string? UserId { get; set; }

    /// <summary>
    /// Ensures the correlation ID is initialized if not already set.
    /// Uses the current Activity's TraceId if available, otherwise generates a new one.
    /// </summary>
    /// <returns>The initialized correlation ID.</returns>
    string InitializeCorrelationId();

    /// <summary>
    /// Pushes all tracked properties onto the Serilog log context and returns
    /// an <see cref="IDisposable"/> that removes them when disposed.
    /// </summary>
    IDisposable PushProperties();

    /// <summary>
    /// Adds a custom property to the log context for the duration of the current scope.
    /// </summary>
    /// <param name="key">Property name.</param>
    /// <param name="value">Property value.</param>
    void AddProperty(string key, object? value);

    /// <summary>Returns a snapshot of all currently tracked properties.</summary>
    IReadOnlyDictionary<string, object?> GetProperties();
}
