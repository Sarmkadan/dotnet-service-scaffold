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

/// <summary>
/// Tests for the ErrorHandlingMiddleware class.
/// </summary>
public class ErrorHandlingMiddlewareTests : IErrorHandlingMiddlewareTests
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ErrorHandlingMiddleware _middleware;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlingMiddlewareTests"/> class.
    /// </summary>
    public ErrorHandlingMiddlewareTests()
    {
        _next = Substitute.For<RequestDelegate>();
        _logger = Substitute.For<ILogger<ErrorHandlingMiddleware>>();
        _webHostEnvironment = Substitute.For<IWebHostEnvironment>();
        _middleware = new ErrorHandlingMiddleware(_next, _logger);
    }

    /// <summary>
    /// Creates a new HttpContext instance with the specified environment.
    /// </summary>
    /// <param name="isDevelopment">Whether the environment is development or not.</param>
    /// <returns>A new HttpContext instance.</returns>
    private HttpContext CreateHttpContext(bool isDevelopment = false)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        _webHostEnvironment.EnvironmentName.Returns(isDevelopment ? ErrorHandlingMiddlewareTestsConstants.DevelopmentEnvironment : ErrorHandlingMiddlewareTestsConstants.ProductionEnvironment);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_webHostEnvironment);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        context.RequestServices = serviceProvider;

        return context;
    }

    /// <summary>
    /// Retrieves the response body of the specified HttpContext instance.
    /// </summary>
    /// <param name="context">The HttpContext instance.</param>
    /// <returns>The response body as a string.</returns>
    private async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Tests that the InvokeAsync method catches a generic exception and returns a 500 status code.
    /// </summary>
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
        context.Response.ContentType.Should().StartWith("application/json");
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain(ErrorHandlingMiddlewareTestsConstants.GenericErrorMessage);
    }

    /// <summary>
    /// Tests that the InvokeAsync method catches a ServiceScaffoldException and returns a 400 status code.
    /// </summary>
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
        responseBody.Should().Contain(ErrorHandlingMiddlewareTestsConstants.BadRequestMessage);
    }

    /// <summary>
    /// Tests that the InvokeAsync method catches an ArgumentNullException and returns a 400 status code.
    /// </summary>
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
        responseBody.Should().Contain(ErrorHandlingMiddlewareTestsConstants.ArgumentNullMessage);
    }

    /// <summary>
    /// Tests that the InvokeAsync method catches an ArgumentException and returns a 400 status code.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_ShouldCatchArgumentExceptionAndReturn400()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new ArgumentException(ErrorHandlingMiddlewareTestsConstants.ArgumentMessage));
        var context = CreateHttpContext(true);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain(ErrorHandlingMiddlewareTestsConstants.ArgumentMessage);
    }

    /// <summary>
    /// Tests that the InvokeAsync method catches an InvalidOperationException and returns a 409 status code.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_ShouldCatchInvalidOperationExceptionAndReturn409()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new InvalidOperationException(ErrorHandlingMiddlewareTestsConstants.InvalidOperationMessage));
        var context = CreateHttpContext(true);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain(ErrorHandlingMiddlewareTestsConstants.InvalidOperationMessage);
    }

    /// <summary>
    /// Tests that the InvokeAsync method catches a KeyNotFoundException and returns a 404 status code.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_ShouldCatchKeyNotFoundExceptionAndReturn404()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new KeyNotFoundException(ErrorHandlingMiddlewareTestsConstants.KeyNotFoundMessage));
        var context = CreateHttpContext(true);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Resource not found");
    }

    /// <summary>
    /// Tests that the InvokeAsync method returns a generic message in production environment.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_ShouldReturnGenericMessageInProduction()
    {
        // Arrange
        _next.When(x => x(Arg.Any<HttpContext>())).Throw(new Exception(ErrorHandlingMiddlewareTestsConstants.SensitiveErrorDetails));
        var context = CreateHttpContext(false); // Production environment

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain(ErrorHandlingMiddlewareTestsConstants.ProductionErrorMessage);
        responseBody.Should().NotContain(ErrorHandlingMiddlewareTestsConstants.SensitiveErrorDetails);
    }
}
