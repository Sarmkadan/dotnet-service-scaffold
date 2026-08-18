using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DotnetServiceScaffold.Presentation.Middleware;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class RateLimitingMiddlewareTests
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddlewareTests()
    {
        _next = _ => Task.CompletedTask;
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<RateLimitingMiddleware>.Instance;
    }

    [Fact]
    public async Task InvokeAsync_SkipsRateLimitingForHealthPath()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/health";
        var options = new RateLimitOptions();
        bool nextInvoked = false;
        RequestDelegate next = _ =>
        {
            nextInvoked = true;
            return Task.CompletedTask;
        };
        var middleware = new RateLimitingMiddleware(next, options, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextInvoked);
        Assert.DoesNotContain("X-RateLimit-Limit", context.Response.Headers);
        Assert.DoesNotContain("X-RateLimit-Remaining", context.Response.Headers);
    }

    [Fact]
    public async Task InvokeAsync_AllowsRequestWhenTokensAvailable_Anonymous()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        var options = new RateLimitOptions { AnonymousRequestsPerMinute = 60 };
        bool nextInvoked = false;
        RequestDelegate next = _ =>
        {
            nextInvoked = true;
            return Task.CompletedTask;
        };
        var middleware = new RateLimitingMiddleware(next, options, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextInvoked);
        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal("60", context.Response.Headers["X-RateLimit-Limit"]);
        Assert.Equal("59", context.Response.Headers["X-RateLimit-Remaining"]);
    }

    [Fact]
    public async Task InvokeAsync_Returns429WhenTokensExhausted_Anonymous()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        var options = new RateLimitOptions { AnonymousRequestsPerMinute = 1 };
        bool firstNextInvoked = false;
        bool secondNextInvoked = false;
        RequestDelegate next = _ =>
        {
            // This tracks if the delegate was called for each request
            // We'll use a closure to track per invocation, but simpler: use a counter
            // Instead, we'll create a new delegate for each test method.
            // We'll handle by resetting a flag before each call.
            // We'll use a class-level flag? Better to use a local variable via closure.
            // We'll restructure: create a new middleware for each request.
            // But we need to share the bucket state? No, the bucket is per clientId.
            // We'll make two separate requests with the same clientId.
            // We'll use a single middleware instance and call InvokeAsync twice.
            // We'll track if next was called for each invocation by checking a flag before and after.
            // We'll do: set a flag to false, call InvokeAsync, then check the flag.
            // We'll do that in the Act section.
            return Task.CompletedTask;
        };
        // We'll use a different approach: create a middleware and call InvokeAsync twice.
        // We'll reset the response between calls.
        var middleware = new RateLimitingMiddleware(_next, options, _logger);

        // Act - first request
        await middleware.InvokeAsync(context);
        // Reset response for second request
        context.Response.Headers.Clear();
        context.Response.StatusCode = 0;

        // Act - second request
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(429, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.Headers.ContentType);
        Assert.Equal("60", context.Response.Headers["Retry-After"]);
    }

    [Fact]
    public async Task InvokeAsync_AllowsRequestWhenTokensAvailable_Authenticated()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        // Set up authenticated user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;
        var options = new RateLimitOptions { AuthenticatedRequestsPerMinute = 300 };
        bool nextInvoked = false;
        RequestDelegate next = _ =>
        {
            nextInvoked = true;
            return Task.CompletedTask;
        };
        var middleware = new RateLimitingMiddleware(next, options, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextInvoked);
        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal("300", context.Response.Headers["X-RateLimit-Limit"]);
        Assert.Equal("299", context.Response.Headers["X-RateLimit-Remaining"]);
    }

    [Fact]
    public async Task InvokeAsync_Returns429WhenTokensExhausted_Authenticated()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        // Set up authenticated user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;
        var options = new RateLimitOptions { AuthenticatedRequestsPerMinute = 1 };
        var middleware = new RateLimitingMiddleware(_next, options, _logger);

        // Act - first request
        await middleware.InvokeAsync(context);
        // Reset response
        context.Response.Headers.Clear();
        context.Response.StatusCode = 0;

        // Act - second request
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(429, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.Headers.ContentType);
        Assert.Equal("60", context.Response.Headers["Retry-After"]);
    }

    [Fact]
    public void GetClientIdentifier_ReturnsUserId_WhenAuthenticated()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;
        var options = new RateLimitOptions();
        var middleware = new RateLimitingMiddleware(_next, options, _logger);

        // Act
        var identifier = GetPrivateMethod<string>(middleware, "GetClientIdentifier", context);

        // Assert
        Assert.Equal("user:user123", identifier);
    }

    [Fact]
    public void GetClientIdentifier_ReturnsIpAddress_WhenNotAuthenticated()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
        var options = new RateLimitOptions();
        var middleware = new RateLimitingMiddleware(_next, options, _logger);

        // Act
        var identifier = GetPrivateMethod<string>(middleware, "GetClientIdentifier", context);

        // Assert
        Assert.Equal("ip:192.168.1.1", identifier);
    }

    // Helper to invoke private methods
    private TResult GetPrivateMethod<TResult>(object obj, string methodName, params object[] parameters)
    {
        var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method == null)
            throw new MissingMethodException($"Method {methodName} not found on {obj.GetType()}");
        return (TResult)method.Invoke(obj, parameters);
    }
}