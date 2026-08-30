using System;

namespace DotnetServiceScaffold.Infrastructure.Data;

/// <summary>
/// Constants for ServiceScaffoldDbContextExtensions.
/// </summary>
internal static class ServiceScaffoldDbContextExtensionsConstants
{
    /// <summary>
    /// The comparison used when matching user email addresses.
    /// </summary>
    public const StringComparison UserEmailComparison = StringComparison.OrdinalIgnoreCase;
}
