#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        get => _currentContext.Value?.CorrelationId ??
               CurrentContext.CustomProperties.GetValueOrDefault(LogContextServiceConstants.CorrelationIdKey)?.ToString();

        set
        {
            var context = _currentContext.Value ?? new ContextState();
            context.CorrelationId = value;
            _currentContext.Value = context;
            CurrentContext.CustomProperties[LogContextServiceConstants.CorrelationIdKey] = value;
        }
    }

    /// <summary>
    /// Gets or sets the authenticated user ID for the current request.
    /// </summary>
    public string? UserId
    {
        get => _currentContext.Value?.UserId ??
               CurrentContext.CustomProperties.GetValueOrDefault(LogContextServiceConstants.UserIdKey)?.ToString();

        set
        {
            var context = _currentContext.Value ?? new ContextState();
            context.UserId = value;
            _currentContext.Value = context;
            CurrentContext.CustomProperties[LogContextServiceConstants.UserIdKey] = value;
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
                ActivityIdFormat.W3C => string.Format(LogContextServiceConstants.TraceParentFormatW3C, activity.TraceId, activity.SpanId),
                _ => string.Format(LogContextServiceConstants.TraceParentFormatLegacy, activity.TraceId, activity.SpanId)
            };
            ActivityId = activity.Id;
            return CorrelationId;
        }

        // Generate a new correlation ID
        CorrelationId = Guid.NewGuid().ToString(LogContextServiceConstants.GuidFormatN);
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
        CurrentContext.CustomProperties[key] = value;
    }

    /// <summary>
    /// Returns a snapshot of all currently tracked properties.
    /// </summary>
    /// <returns>A read‑only dictionary of properties.</returns>
    public IReadOnlyDictionary<string, object?> GetProperties()
        => new ReadOnlyDictionary<string, object?>(CurrentContext.CustomProperties);

    /// <summary>
    /// Pushes all tracked properties onto the Serilog log context and returns
    /// an <see cref="IDisposable"/> that removes them when disposed.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that pops the context when disposed.</returns>
    public IDisposable PushProperties()
    {
        // Ensure a ContextState exists for the current async flow
        var contextState = _currentContext.Value ?? new ContextState();

        // Snapshot of custom properties before pushing
        var snapshot = new Dictionary<string, object?>(CurrentContext.CustomProperties,
            StringComparer.OrdinalIgnoreCase);

        // Add AsyncLocal context properties to the snapshot
        if (contextState.CorrelationId is not null)
        {
            snapshot[LogContextServiceConstants.CorrelationIdKey] = contextState.CorrelationId;
        }

        if (contextState.UserId is not null)
        {
            snapshot[LogContextServiceConstants.UserIdKey] = contextState.UserId;
        }

        if (contextState.ActivityId is not null)
        {
            snapshot[LogContextServiceConstants.ActivityIdKey] = contextState.ActivityId;
        }

        if (contextState.TraceParent is not null)
        {
            snapshot[LogContextServiceConstants.TraceParentKey] = contextState.TraceParent;
        }

        // Push each property onto Serilog's LogContext
        var disposables = new List<IDisposable>(snapshot.Count);
        foreach (var (key, value) in snapshot)
        {
            disposables.Add(LogContext.PushProperty(key, value));
        }

        // Return a disposable that restores the custom‑property dictionary
        // to its previous state when the scope ends.
        return new ScopeDisposable(this, disposables, snapshot);
    }

    /// <summary>
    /// Gets the current <see cref="ContextState"/> instance, creating one if necessary.
    /// </summary>
    private ContextState CurrentContext => _currentContext.Value ??= new ContextState();

    /// <summary>
    /// Context state that flows with <see cref="AsyncLocal{T}"/> across async boundaries.
    /// </summary>
    private sealed class ContextState
    {
        public string? CorrelationId { get; set; }
        public string? UserId { get; set; }
        public string? ActivityId { get; set; }
        public string? TraceParent { get; set; }

        // Holds custom properties that are scoped to the async flow.
        public ConcurrentDictionary<string, object?> CustomProperties { get; } =
            new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Disposable that restores the custom‑property dictionary to a previous snapshot
    /// and disposes the Serilog <see cref="LogContext"/> pushes.
    /// </summary>
    private sealed class ScopeDisposable : IDisposable
    {
        private readonly LogContextService _service;
        private readonly IReadOnlyList<IDisposable> _logDisposables;
        private readonly Dictionary<string, object?> _previousProperties;
        private bool _disposed;

        internal ScopeDisposable(
            LogContextService service,
            IReadOnlyList<IDisposable> logDisposables,
            Dictionary<string, object?> previousProperties)
        {
            _service = service;
            _logDisposables = logDisposables;
            _previousProperties = previousProperties;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Restore the custom‑property dictionary to its previous snapshot.
            var current = _service.CurrentContext.CustomProperties;
            current.Clear();
            foreach (var kvp in _previousProperties)
            {
                current[kvp.Key] = kvp.Value;
            }

            // Dispose the Serilog pushes in reverse order (LIFO).
            for (var i = _logDisposables.Count - 1; i >= 0; i--)
            {
                _logDisposables[i].Dispose();
            }
        }
    }
}
