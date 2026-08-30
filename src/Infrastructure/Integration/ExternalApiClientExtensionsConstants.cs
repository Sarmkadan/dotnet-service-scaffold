#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Constants for ExternalApiClientExtensions.
/// </summary>
internal static class ExternalApiClientExtensionsConstants
{
    /// <summary>
    /// Default maximum number of retry attempts.
    /// </summary>
    public const int DefaultMaxRetries = 3;

    /// <summary>
    /// Default request timeout in seconds.
    /// </summary>
    public const int DefaultTimeoutSeconds = 30;
}