#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Presentation.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetServiceScaffold.Tests.Presentation.Middleware;

public class ErrorHandlingMiddlewareTests
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ErrorHandlingMiddleware _middleware;

    public ErrorHandlingMiddlewareTests()
    {
        _next = Substitute.For<RequestDelegate>();
        _logger = Substitute.For<ILogger<ErrorHandlingMiddleware>>();
        _webHostEnvironment = Substitute.For<IWebHostEnvironment>();
        _middleware = new ErrorHandlingMiddleware(_next, _logger);
    }

    private HttpContext CreateHttpContext(bool isDevelopment = false)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        _webHostEnvironment.EnvironmentName.Returns(isDevelopment ? "Development" : "Production");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_webHostEnvironment);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        context.RequestServices = serviceProvider;

        return context;
    }

    private async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCatchGenericExceptionAndReturn500()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new Exception("Test exception"));
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("An error occurred processing your request.");
        _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldCatchServiceScaffoldExceptionAndReturn400()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new ServiceScaffoldException("Bad request error"));
        var context = CreateHttpContext(true); // Development environment to see full message

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Bad request error");
        _logger.Received(1).LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldCatchArgumentNullExceptionAndReturn400()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new ArgumentNullException("param", "Argument null error"));
        var context = CreateHttpContext(true);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Argument null error");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCatchArgumentExceptionAndReturn400()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new ArgumentException("Invalid argument"));
        var context = CreateHttpContext(true);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Invalid argument");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCatchInvalidOperationExceptionAndReturn409()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new InvalidOperationException("Operation not allowed"));
        var context = CreateHttpContext(true);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Operation not allowed");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCatchKeyNotFoundExceptionAndReturn404()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new KeyNotFoundException("Resource not found"));
        var context = CreateHttpContext(true);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Resource not found");
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnGenericMessageInProduction()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new Exception("Sensitive error details"));
        var context = CreateHttpContext(false); // Production environment

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("An error occurred processing your request. Please contact support with the error ID.");
        responseBody.Should().NotContain("Sensitive error details");
    }
}
