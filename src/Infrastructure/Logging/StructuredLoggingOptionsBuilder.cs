#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Builder for <see cref="StructuredLoggingOptions"/> that provides a fluent interface for configuration.
/// </summary>
public class StructuredLoggingOptionsBuilder
{
    // Default values match those in StructuredLoggingOptions
    private string _applicationName = "DotnetServiceScaffold";
    private bool _enrichWithMachineName = true;
    private bool _enrichWithEnvironment = true;
    private bool _enableCorrelationId = true;
    private string _correlationIdHeader = "X-Correlation-Id";
    private bool _enrichWithRequestContext = true;
    private string _minimumLevel = "Information";

    /// <summary>
    /// Sets the application name.
    /// </summary>
    /// <param name="applicationName">The application name to set.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="applicationName"/> is null or empty.</exception>
    public StructuredLoggingOptionsBuilder WithApplicationName(string applicationName)
    {
        ArgumentException.ThrowIfNullOrEmpty(applicationName, nameof(applicationName));
        _applicationName = applicationName;
        return this;
    }

    /// <summary>
    /// Sets whether to enrich log entries with the machine/host name.
    /// </summary>
    /// <param name="enrich">True to enrich with machine name; otherwise, false.</param>
    /// <returns>The builder instance for chaining.</returns>
    public StructuredLoggingOptionsBuilder WithEnrichWithMachineName(bool enrich)
    {
        _enrichWithMachineName = enrich;
        return this;
    }

    /// <summary>
    /// Sets whether to enrich log entries with the environment name.
    /// </summary>
    /// <param name="enrich">True to enrich with environment name; otherwise, false.</param>
    /// <returns>The builder instance for chaining.</returns>
    public StructuredLoggingOptionsBuilder WithEnrichWithEnvironment(bool enrich)
    {
        _enrichWithEnvironment = enrich;
        return this;
    }

    /// <summary>
    /// Sets whether to enable correlation ID attachment to HTTP requests.
    /// </summary>
    /// <param name="enable">True to enable correlation ID; otherwise, false.</param>
    /// <returns>The builder instance for chaining.</returns>
    public StructuredLoggingOptionsBuilder WithEnableCorrelationId(bool enable)
    {
        _enableCorrelationId = enable;
        return this;
    }

    /// <summary>
    /// Sets the HTTP header name used to read or propagate the correlation ID.
    /// </summary>
    /// <param name="header">The correlation ID header name.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="header"/> is null or empty.</exception>
    public StructuredLoggingOptionsBuilder WithCorrelationIdHeader(string header)
    {
        ArgumentException.ThrowIfNullOrEmpty(header, nameof(header));
        _correlationIdHeader = header;
        return this;
    }

    /// <summary>
    /// Sets whether to enrich log entries with request context (method, path, status, duration).
    /// </summary>
    /// <param name="enrich">True to enrich with request context; otherwise, false.</param>
    /// <returns>The builder instance for chaining.</returns>
    public StructuredLoggingOptionsBuilder WithEnrichWithRequestContext(bool enrich)
    {
        _enrichWithRequestContext = enrich;
        return this;
    }

    /// <summary>
    /// Sets the minimum log level for the structured logging pipeline.
    /// </summary>
    /// <param name="minimumLevel">The minimum log level (e.g., "Information", "Debug").</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="minimumLevel"/> is null or empty.</exception>
    public StructuredLoggingOptionsBuilder WithMinimumLevel(string minimumLevel)
    {
        ArgumentException.ThrowIfNullOrEmpty(minimumLevel, nameof(minimumLevel));
        _minimumLevel = minimumLevel;
        return this;
    }

    /// <summary>
    /// Creates a new builder pre-filled with values from an existing <see cref="StructuredLoggingOptions"/> instance.
    /// </summary>
    /// <param name="template">The template options to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static StructuredLoggingOptionsBuilder From(StructuredLoggingOptions template)
    {
        ArgumentNullException.ThrowIfNull(template);
        return new StructuredLoggingOptionsBuilder
        {
            _applicationName = template.ApplicationName,
            _enrichWithMachineName = template.EnrichWithMachineName,
            _enrichWithEnvironment = template.EnrichWithEnvironment,
            _enableCorrelationId = template.EnableCorrelationId,
            _correlationIdHeader = template.CorrelationIdHeader,
            _enrichWithRequestContext = template.EnrichWithRequestContext,
            _minimumLevel = template.MinimumLevel
        };
    }

    /// <summary>
    /// Builds a <see cref="StructuredLoggingOptions"/> instance with the current configuration.
    /// </summary>
    /// <returns>A configured <see cref="StructuredLoggingOptions"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when any required property is null, empty, or has an invalid value.
    /// </exception>
    public StructuredLoggingOptions Build()
    {
        var options = new StructuredLoggingOptions
        {
            ApplicationName = _applicationName,
            EnrichWithMachineName = _enrichWithMachineName,
            EnrichWithEnvironment = _enrichWithEnvironment,
            EnableCorrelationId = _enableCorrelationId,
            CorrelationIdHeader = _correlationIdHeader,
            EnrichWithRequestContext = _enrichWithRequestContext,
            MinimumLevel = _minimumLevel
        };

        options.Validate();
        return options;
    }
}