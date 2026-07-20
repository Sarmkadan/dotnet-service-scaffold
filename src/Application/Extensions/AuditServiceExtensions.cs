#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotnetServiceScaffold.Application.Services;

namespace DotnetServiceScaffold.Application.Extensions;

/// <summary>
/// Extension methods for <see cref="IAuditService"/> to provide async logging
/// capabilities without requiring changes to the original interface definition.
/// </summary>
public static class AuditServiceExtensions
{
    /// <summary>
    /// Logs a message asynchronously. If the underlying <see cref="IAuditService"/>
    /// implementation provides a synchronous <c>Log(string)</c> method, it will be
    /// invoked via reflection. If it already provides an async <c>LogAsync(string)</c>
    /// method, that method will be called directly. Otherwise, this method completes
    /// immediately.
    /// </summary>
    /// <param name="auditService">The audit service instance.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="cancellationToken">Optional cancellation token (currently unused).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task LogAsync(this IAuditService auditService, string message, CancellationToken cancellationToken = default)
    {
        // Try to find an existing async LogAsync method (in case the interface was later extended)
        var asyncMethod = auditService.GetType().GetMethod("LogAsync", new[] { typeof(string), typeof(CancellationToken) });
        if (asyncMethod != null && asyncMethod.ReturnType == typeof(Task))
        {
            // Invoke the existing async method
            var result = asyncMethod.Invoke(auditService, new object[] { message, cancellationToken });
            return (Task)result!;
        }

        // Fallback to a synchronous Log(string) method if it exists
        var syncMethod = auditService.GetType().GetMethod("Log", new[] { typeof(string) });
        if (syncMethod != null)
        {
            var result = syncMethod.Invoke(auditService, new object[] { message });
            // If the synchronous method returns a Task, return it; otherwise, wrap in a completed task.
            return result is Task task ? task : Task.CompletedTask;
        }

        // No known logging method – complete immediately.
        return Task.CompletedTask;
    }
}
