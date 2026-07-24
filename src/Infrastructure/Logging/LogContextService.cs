#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using Serilog.Context;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Scoped service that tracks per-request log properties using <see cref="AsyncLocal{T}"/>
/// and pushes them onto the Serilog <see cref="LogContext"/> stack for structured output enrichment.
/// Ensures proper flow across async boundaries and integrates with W3C trace context.
/// </summary>
public sealed class LogContextService : ILogContextService
{
    // AsyncLocal ensures values flow correctly across async/await boundaries
    private static readonly AsyncLocal<ContextState> _currentContext = new();

    // Thread-safe storage for custom properties within the current async context
    private readonly ConcurrentDictionary<string, object?> _properties = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the current activity ID from <see cref="Activity.Current"/>.
    /// </summary>
    public string? ActivityId
    {
        get => _currentContext.Value?.ActivityId;
        set
        {
            var context = _currentContext.Value ?? new ContextState();
            context.ActivityId = value;
            _currentContext.Value = context;
        }
    }

    /// <summary>
    /// Gets or sets the W3C traceparent header value (trace-id:parent-id:span-id:flags).
    /// </summary>
    public string? TraceParent
    {
        get => _currentContext.Value?.TraceParent;
        set
        {
            var context = _currentContext.Value ?? new ContextState();
            context.TraceParent = value;
            _currentContext.Value = context;
        }
    }

    /// <summary>
    /// Gets or sets the correlation ID for the current request.
    /// </summary>
    public string? CorrelationId
    {
        get => _currentContext.Value?.CorrelationId ?? _properties.GetValueOrDefault("CorrelationId")?.ToString();
        set
        {
            var context = _currentContext.Value ?? new ContextState();
            context.CorrelationId = value;
            _currentContext.Value = context;
            _properties["CorrelationId"] = value;
        }
    }

    /// <summary>
    /// Gets or sets the authenticated user ID for the current request.
    /// </summary>
    public string? UserId
    {
        get => _currentContext.Value?.UserId ?? _properties.GetValueOrDefault("UserId")?.ToString();
        set
        {
            var context = _currentContext.Value ?? new ContextState();
            context.UserId = value;
            _currentContext.Value = context;
            _properties["UserId"] = value;
        }
    }

    /// <summary>
    /// Ensures the correlation ID is initialized if not already set.
    /// Uses the current Activity's TraceId if available, otherwise generates a new one.
    /// </summary>
    /// <returns>The initialized correlation ID.</returns>
    public string InitializeCorrelationId()
    {
        if (CorrelationId is not null)
        {
            return CorrelationId;
        }

        // Try to get from Activity if available (W3C trace context)
        var activity = Activity.Current;
        if (activity?.TraceId != null && !activity.TraceId.Equals(default))
        {
            CorrelationId = activity.TraceId.ToHexString();
            TraceParent = activity.IdFormat switch
            {
                ActivityIdFormat.W3C => $"00-{activity.TraceId:D32}-{activity.SpanId:D16}-00",
                _ => $"00-{activity.TraceId:D32}-{activity.SpanId:D16}-01"
            };
            ActivityId = activity.Id;
            return CorrelationId;
        }

        // Generate a new correlation ID
        CorrelationId = Guid.NewGuid().ToString("N");
        return CorrelationId;
    }

    /// <summary>
    /// Adds a custom property to the log context for the duration of the current scope.
    /// </summary>
    /// <param name="key">Property name.</param>
    /// <param name="value">Property value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or whitespace.</exception>
    public void AddProperty(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _properties[key] = value;
    }

    /// <summary>
    /// Returns a snapshot of all currently tracked properties.
    /// </summary>
    /// <returns>A read-only dictionary of properties.</returns>
    public IReadOnlyDictionary<string, object?> GetProperties() => _properties.AsReadOnly();

    /// <summary>
    /// Pushes all tracked properties onto the Serilog log context and returns
    /// an <see cref="IDisposable"/> that removes them when disposed.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that pops the context when disposed.</returns>
    public IDisposable PushProperties()
    {
        // Get current context state
        var contextState = _currentContext.Value ?? new ContextState();

        // Create a snapshot of properties to push
        var propertiesToPush = new Dictionary<string, object?>(_properties, StringComparer.OrdinalIgnoreCase);

        // Add AsyncLocal context properties
        if (contextState.CorrelationId is not null)
        {
            propertiesToPush["CorrelationId"] = contextState.CorrelationId;
        }
        if (contextState.UserId is not null)
        {
            propertiesToPush["UserId"] = contextState.UserId;
        }
        if (contextState.ActivityId is not null)
        {
            propertiesToPush["ActivityId"] = contextState.ActivityId;
        }
        if (contextState.TraceParent is not null)
        {
            propertiesToPush["TraceParent"] = contextState.TraceParent;
        }

        var disposables = new List<IDisposable>(propertiesToPush.Count);
        foreach (var (key, value) in propertiesToPush)
        {
            disposables.Add(LogContext.PushProperty(key, value));
        }

        return new CompositeDisposable(disposables);
    }

    /// <summary>
    /// Context state that flows with AsyncLocal across async boundaries.
    /// </summary>
    private sealed class ContextState
    {
        public string? CorrelationId { get; set; }
        public string? UserId { get; set; }
        public string? ActivityId { get; set; }
        public string? TraceParent { get; set; }
    }

    /// <summary>
    /// Composite disposable that disposes all child disposables in reverse order.
    /// </summary>
    private sealed class CompositeDisposable : IDisposable
    {
        private readonly IReadOnlyList<IDisposable> _items;
        private bool _disposed;

        internal CompositeDisposable(IReadOnlyList<IDisposable> items)
        {
            _items = items;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            // Dispose in reverse order (LIFO) to properly nest contexts
            for (var index = _items.Count - 1; index >= 0; index--)
            {
                _items[index].Dispose();
            }
        }
    }
}
