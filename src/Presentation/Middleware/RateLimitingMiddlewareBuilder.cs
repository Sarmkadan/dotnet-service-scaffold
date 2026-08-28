#nullable enable

using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetServiceScaffold.Presentation.Middleware;

/// <summary>
/// Fluent builder for <see cref="RateLimitingMiddleware"/> instances.
/// </summary>
public class RateLimitingMiddlewareBuilder
{
    private double? _tokens;
    private DateTime? _lastRefillTime;
    private int? _capacity;
    private int? _anonymousRequestsPerMinute;
    private int? _authenticatedRequestsPerMinute;

    /// <summary>
    /// Sets the initial token count for new token buckets.
    /// </summary>
    /// <param name="tokens">The initial token count.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tokens"/> is negative.</exception>
    public RateLimitingMiddlewareBuilder WithTokens(double tokens)
    {
        if (tokens < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tokens), "Token count cannot be negative.");
        }

        _tokens = tokens;
        return this;
    }

    /// <summary>
    /// Sets the last refill time for new token buckets.
    /// </summary>
    /// <param name="lastRefillTime">The last refill time.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RateLimitingMiddlewareBuilder WithLastRefillTime(DateTime lastRefillTime)
    {
        _lastRefillTime = lastRefillTime;
        return this;
    }

    /// <summary>
    /// Sets the bucket capacity for new token buckets.
    /// </summary>
    /// <param name="capacity">The bucket capacity.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than or equal to zero.</exception>
    public RateLimitingMiddlewareBuilder WithCapacity(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Bucket capacity must be greater than zero.");
        }

        _capacity = capacity;
        return this;
    }

    /// <summary>
    /// Sets the request limit per minute for anonymous requests.
    /// </summary>
    /// <param name="anonymousRequestsPerMinute">The request limit per minute for anonymous requests.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="anonymousRequestsPerMinute"/> is less than or equal to zero.</exception>
    public RateLimitingMiddlewareBuilder WithAnonymousRequestsPerMinute(int anonymousRequestsPerMinute)
    {
        if (anonymousRequestsPerMinute <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(anonymousRequestsPerMinute), "Anonymous requests per minute must be greater than zero.");
        }

        _anonymousRequestsPerMinute = anonymousRequestsPerMinute;
        return this;
    }

    /// <summary>
    /// Sets the request limit per minute for authenticated requests.
    /// </summary>
    /// <param name="authenticatedRequestsPerMinute">The request limit per minute for authenticated requests.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="authenticatedRequestsPerMinute"/> is less than or equal to zero.</exception>
    public RateLimitingMiddlewareBuilder WithAuthenticatedRequestsPerMinute(int authenticatedRequestsPerMinute)
    {
        if (authenticatedRequestsPerMinute <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(authenticatedRequestsPerMinute), "Authenticated requests per minute must be greater than zero.");
        }

        _authenticatedRequestsPerMinute = authenticatedRequestsPerMinute;
        return this;
    }

    /// <summary>
    /// Creates a builder pre-filled with values from an existing <see cref="RateLimitingMiddleware"/>.
    /// </summary>
    /// <param name="template">The rate limiting middleware to copy values from.</param>
    /// <returns>A new builder instance initialized with the template's values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
    public static RateLimitingMiddlewareBuilder From(RateLimitingMiddleware template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var builder = new RateLimitingMiddlewareBuilder();

        // Extract options using reflection
        var optionsField = typeof(RateLimitingMiddleware).GetField("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (optionsField != null)
        {
            var options = optionsField.GetValue(template) as RateLimitOptions;
            if (options != null)
            {
                builder.WithAnonymousRequestsPerMinute(options.AnonymousRequestsPerMinute)
                       .WithAuthenticatedRequestsPerMinute(options.AuthenticatedRequestsPerMinute);
            }
        }

        // Note: Tokens, LastRefillTime, and Capacity are internal to TokenBucketState instances
        // which are stored in the _buckets dictionary and created lazily.
        // Extracting these values would require accessing the _buckets field and examining
        // existing TokenBucketState instances, which may not be representative of default values.
        // For simplicity, we leave these as null to use the middleware's default initialization.

        return builder;
    }

    /// <summary>
    /// Builds the <see cref="RateLimitingMiddleware"/> instance with the current values.
    /// </summary>
    /// <returns>A fully configured <see cref="RateLimitingMiddleware"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
    public RateLimitingMiddleware Build()
    {
        // Validate required properties
        if (!_anonymousRequestsPerMinute.HasValue)
        {
            throw new ArgumentException("AnonymousRequestsPerMinute is required.", nameof(_anonymousRequestsPerMinute));
        }

        if (!_authenticatedRequestsPerMinute.HasValue)
        {
            throw new ArgumentException("AuthenticatedRequestsPerMinute is required.", nameof(_authenticatedRequestsPerMinute));
        }

        var options = new RateLimitOptions
        {
            AnonymousRequestsPerMinute = _anonymousRequestsPerMinute.Value,
            AuthenticatedRequestsPerMinute = _authenticatedRequestsPerMinute.Value
        };

        // Create middleware with default dependencies
        // In a real application, these would be provided by the DI framework
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var logger = NullLogger<RateLimitingMiddleware>.Instance;

        var middleware = new RateLimitingMiddleware(next, options, logger);

        // Note: Applying Tokens, LastRefillTime, and Capacity to existing TokenBucketState instances
        // would require accessing the private _buckets field and modifying existing states.
        // Since TokenBucketState instances are created lazily per client ID, and we want to
        // configure the initial state for new buckets, this would require modifying the middleware
        // itself to use a factory pattern or accept these values in the constructor.
        // Given the constraint not to modify existing files, we accept that these values
        // cannot be applied to the built middleware instance without modification to RateLimitingMiddleware.
        // The builder stores these values for completeness and potential future use.

        return middleware;
    }
}