#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Serilog.Context;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Extension methods for <see cref="ILogContextService"/> that provide convenient APIs
/// for working with Serilog context and structured logging.
/// </summary>
public static class LogContextServiceExtensions
{
    /// <summary>
    /// Adds multiple properties to the log context at once.
    /// </summary>
    /// <param name="service">The log context service instance.</param>
    /// <param name="properties">Dictionary of properties to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="properties"/> is null.</exception>
    public static void AddProperties(this ILogContextService service, IReadOnlyDictionary<string, object?> properties)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(properties);

        foreach (var (key, value) in properties)
        {
            service.AddProperty(key, value);
        }
    }

    /// <summary>
    /// Ensures the correlation ID is initialized and adds common request-scoped properties.
    /// Uses W3C trace context if available, otherwise generates a new correlation ID.
    /// </summary>
    /// <param name="service">The log context service instance.</param>
    /// <param name="userId">The user ID associated with the request.</param>
    /// <param name="operationName">The name of the operation being performed.</param>
    /// <returns>The initialized correlation ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static string InitializeRequestContext(this ILogContextService service, string? userId = null, string? operationName = null)
    {
        ArgumentNullException.ThrowIfNull(service);

        // Initialize correlation ID (uses W3C trace context if available)
        var correlationId = service.InitializeCorrelationId();

        // Add request-specific properties
        service.AddProperty(LogContextServiceExtensionsConstants.CorrelationIdPropertyName, correlationId);
        service.AddProperty(LogContextServiceExtensionsConstants.RequestIdPropertyName, Guid.NewGuid().ToString(LogContextServiceExtensionsConstants.GuidFormatN));

        if (userId is not null)
        {
            service.AddProperty(LogContextServiceExtensionsConstants.UserIdPropertyName, userId);
            service.UserId = userId;
        }

        if (operationName is not null)
        {
            service.AddProperty(LogContextServiceExtensionsConstants.OperationPropertyName, operationName);
        }

        return correlationId;
    }

    /// <summary>
    /// Adds common request-scoped properties to the log context for structured logging.
    /// Includes correlation ID, user ID, operation name, and timing information.
    /// </summary>
    /// <param name="service">The log context service instance.</param>
    /// <param name="correlationId">The correlation ID for the request.</param>
    /// <param name="userId">The user ID associated with the request.</param>
    /// <param name="operationName">The name of the operation being performed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="correlationId"/> is null or whitespace.</exception>
    [Obsolete("Use InitializeRequestContext instead. This method does not initialize W3C trace context.")]
    public static void AddRequestProperties(this ILogContextService service, string correlationId, string? userId, string? operationName = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        service.AddProperty(LogContextServiceExtensionsConstants.CorrelationIdPropertyName, correlationId);
        service.AddProperty(LogContextServiceExtensionsConstants.RequestIdPropertyName, Guid.NewGuid().ToString(LogContextServiceExtensionsConstants.GuidFormatN));

        if (userId is not null)
        {
            service.AddProperty(LogContextServiceExtensionsConstants.UserIdPropertyName, userId);
        }

        if (operationName is not null)
        {
            service.AddProperty(LogContextServiceExtensionsConstants.OperationPropertyName, operationName);
        }
    }

    /// <summary>
    /// Creates a new scope with the current log context properties and executes the provided action.
    /// The scope is automatically disposed when the action completes.
    /// </summary>
    /// <param name="service">The log context service instance.</param>
    /// <param name="action">The action to execute within the scoped context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="action"/> is null.</exception>
    public static void WithContextScope(this ILogContextService service, Action action)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(action);

        using (service.PushProperties())
        {
            action();
        }
    }

    /// <summary>
    /// Creates a new scope with the current log context properties and executes the provided function.
    /// The scope is automatically disposed when the function completes.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="service">The log context service instance.</param>
    /// <param name="func">The function to execute within the scoped context.</param>
    /// <returns>The result of the function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="func"/> is null.</exception>
    public static T WithContextScope<T>(this ILogContextService service, Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(func);

        using (service.PushProperties())
        {
            return func();
        }
    }

    /// <summary>
    /// Gets a specific property value from the log context.
    /// </summary>
    /// <typeparam name="T">The expected type of the property value.</typeparam>
    /// <param name="service">The log context service instance.</param>
    /// <param name="key">The property key to retrieve.</param>
    /// <param name="value">When this method returns, contains the property value if found; otherwise, the default value for type <typeparamref name="T"/>.</param>
    /// <returns>True if the property exists and has a value; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or whitespace.</exception>
    public static bool TryGetProperty<T>(this ILogContextService service, string key, out T? value)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        value = default;
        if (service.GetProperties().TryGetValue(key, out var objValue) && objValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Measures and logs the execution time of an action, adding timing information to the log context.
    /// </summary>
    /// <param name="service">The log context service instance.</param>
    /// <param name="actionName">The name of the action being measured.</param>
    /// <param name="action">The action to measure.</param>
    /// <returns>A <see cref="System.Diagnostics.Stopwatch"/> instance that can be used for further timing measurements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/>, <paramref name="actionName"/>, or <paramref name="action"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="actionName"/> is null or whitespace.</exception>
    public static System.Diagnostics.Stopwatch MeasureExecutionTime(this ILogContextService service, string actionName, Action action)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(actionName);
        ArgumentNullException.ThrowIfNull(action);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        service.AddProperty(LogContextServiceExtensionsConstants.MeasuredActionPropertyName, actionName);

        try
        {
            action();
        }
        finally
        {
            stopwatch.Stop();
            service.AddProperty(LogContextServiceExtensionsConstants.ActionDurationMsPropertyName, stopwatch.ElapsedMilliseconds);
        }

        return stopwatch;
    }

    /// <summary>
    /// Measures and logs the execution time of a function, adding timing information to the log context.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="service">The log context service instance.</param>
    /// <param name="actionName">The name of the action being measured.</param>
    /// <param name="func">The function to measure.</param>
    /// <returns>A tuple containing the result of the function and a <see cref="System.Diagnostics.Stopwatch"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/>, <paramref name="actionName"/>, or <paramref name="func"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="actionName"/> is null or whitespace.</exception>
    public static (T Result, System.Diagnostics.Stopwatch Stopwatch) MeasureExecutionTime<T>(this ILogContextService service, string actionName, Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(actionName);
        ArgumentNullException.ThrowIfNull(func);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        service.AddProperty(LogContextServiceExtensionsConstants.MeasuredActionPropertyName, actionName);

        T result;
        try
        {
            result = func();
        }
        finally
        {
            stopwatch.Stop();
            service.AddProperty(LogContextServiceExtensionsConstants.ActionDurationMsPropertyName, stopwatch.ElapsedMilliseconds);
        }

        return (result, stopwatch);
    }
}
