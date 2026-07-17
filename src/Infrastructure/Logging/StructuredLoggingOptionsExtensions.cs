#nullable enable

using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Extension methods for <see cref="StructuredLoggingOptions"/> to provide additional functionality
/// for configuring structured logging behavior.
/// </summary>
public static class StructuredLoggingOptionsExtensions
{
    /// <summary>
    /// Configures the application name used in all log entries.
    /// </summary>
    /// <param name="options">The logging options to configure.</param>
    /// <param name="applicationName">The name of the application to include in structured logs.</param>
    /// <returns>The configured <see cref="StructuredLoggingOptions"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="applicationName"/> is null or empty.</exception>
    public static StructuredLoggingOptions WithApplicationName(
        this StructuredLoggingOptions options,
        string applicationName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(applicationName);

        options.ApplicationName = applicationName;
        return options;
    }

    /// <summary>
    /// Configures the minimum log level using a <see cref="LogLevel"/> enum value.
    /// </summary>
    /// <param name="options">The logging options to configure.</param>
    /// <param name="level">The minimum log level to set.</param>
    /// <returns>The configured <see cref="StructuredLoggingOptions"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static StructuredLoggingOptions WithMinimumLevel(
        this StructuredLoggingOptions options,
        LogLevel level)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.MinimumLevel = level.ToString();
        return options;
    }

    /// <summary>
    /// Configures the HTTP header name used for correlation ID propagation.
    /// </summary>
    /// <param name="options">The logging options to configure.</param>
    /// <param name="headerName">The HTTP header name to use for correlation ID.</param>
    /// <returns>The configured <see cref="StructuredLoggingOptions"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="headerName"/> is null or empty.</exception>
    public static StructuredLoggingOptions WithCorrelationIdHeader(
        this StructuredLoggingOptions options,
        string headerName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(headerName);

        options.CorrelationIdHeader = headerName;
        return options;
    }

    /// <summary>
    /// Disables automatic inclusion of machine name in log entries.
    /// </summary>
    /// <param name="options">The logging options to configure.</param>
    /// <returns>The configured <see cref="StructuredLoggingOptions"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static StructuredLoggingOptions WithoutMachineNameEnrichment(
        this StructuredLoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.EnrichWithMachineName = false;
        return options;
    }
}