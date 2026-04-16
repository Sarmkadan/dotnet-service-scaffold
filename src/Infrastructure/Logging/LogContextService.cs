#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Serilog.Context;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Scoped service that tracks per-request log properties and pushes them onto
/// the Serilog <see cref="LogContext"/> stack for structured output enrichment.
/// </summary>
public sealed class LogContextService : ILogContextService
{
    private readonly Dictionary<string, object?> _properties = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public string? CorrelationId
    {
        get => _properties.TryGetValue("CorrelationId", out var value) ? value?.ToString() : null;
        set => _properties["CorrelationId"] = value;
    }

    /// <inheritdoc/>
    public string? UserId
    {
        get => _properties.TryGetValue("UserId", out var value) ? value?.ToString() : null;
        set => _properties["UserId"] = value;
    }

    /// <inheritdoc/>
    public void AddProperty(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _properties[key] = value;
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> GetProperties() => _properties.AsReadOnly();

    /// <inheritdoc/>
    public IDisposable PushProperties()
    {
        var disposables = new List<IDisposable>(_properties.Count);
        foreach (var (key, value) in _properties)
        {
            disposables.Add(LogContext.PushProperty(key, value));
        }

        return new CompositeDisposable(disposables);
    }

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
            for (var index = _items.Count - 1; index >= 0; index--)
            {
                _items[index].Dispose();
            }
        }
    }
}
