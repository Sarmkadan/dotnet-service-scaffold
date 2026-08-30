#nullable enable

namespace DotnetServiceScaffold.Application.Extensions;

/// <summary>
/// Constants for AuditServiceExtensions.
/// </summary>
internal static class AuditServiceExtensionsConstants
{
    /// <summary>
    /// The action name for logging a simple message.
    /// </summary>
    public const string MessageLoggedAction = "MessageLogged";

    /// <summary>
    /// The entity type for system-level actions.
    /// </summary>
    public const string SystemEntityType = "System";
}