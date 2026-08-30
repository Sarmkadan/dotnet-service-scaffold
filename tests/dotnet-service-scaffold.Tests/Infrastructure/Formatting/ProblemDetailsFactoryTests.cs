#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotnetServiceScaffold.Domain.Exceptions;
using DotnetServiceScaffold.Infrastructure.Formatting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

/// <summary>
/// Tests for the ProblemDetailsFactory class.
/// </summary>
public class ProblemDetailsFactoryTests : IProblemDetailsFactoryTests
{
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Creates a new HttpContext instance with the specified environment.
    /// </summary>
    private HttpContext CreateHttpContext(bool isDevelopment = false)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var webHostEnvironment = Substitute.For<IWebHostEnvironment>();
        webHostEnvironment.EnvironmentName.Returns(isDevelopment ? "Development" : "Production");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(webHostEnvironment);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        context.RequestServices = serviceProvider;

        return context;
    }

    /// <summary>
    /// Asynchronously creates a new HttpContext instance with the specified environment.
    /// </summary>
    private async Task<HttpContext> CreateHttpContextAsync(bool isDevelopment = false, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(CreateHttpContext(isDevelopment));
    }

    /// <summary>
    /// Retrieves the response body of the specified HttpContext instance.
    /// </summary>
    private async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Tests that CreateProblemDetails creates a valid problem details object with required fields.
    /// </summary>
    [Fact]
    public void CreateProblemDetails_ShouldCreateValidProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: ProblemDetailsFactoryTestsConstants.StatusCodeBadRequest,
            title: ProblemDetailsFactoryTestsConstants.BadRequestTitle,
            detail: ProblemDetailsFactoryTestsConstants.InvalidInputDataDetail,
            type: ProblemDetailsFactoryTestsConstants.BadRequestUrl
        );

        // Assert
        problemDetails.Should().NotBeNull();
        problemDetails.Type.Should().Be(ProblemDetailsFactoryTestsConstants.BadRequestUrl);
        problemDetails.Title.Should().Be(ProblemDetailsFactoryTestsConstants.BadRequestTitle);
        problemDetails.Status.Should().Be(ProblemDetailsFactoryTestsConstants.StatusCodeBadRequest);
        problemDetails.Detail.Should().Be("Invalid input data");
        problemDetails.Instance.Should().Be(context.Request.Path.ToString());
        problemDetails.Extensions.Should().ContainKey("timestamp");
    }

    /// <summary>
    /// Tests that CreateProblemDetails sets default type to "about:blank" when not provided.
    /// </summary>
    [Fact]
    public void CreateProblemDetails_ShouldSetDefaultTypeToAboutBlank()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: ProblemDetailsFactoryTestsConstants.StatusCodeInternalServerError
        );

        // Assert
        problemDetails.Type.Should().Be(ProblemDetailsFactoryTestsConstants.DefaultProblemType);
    }

    /// <summary>
    /// Tests that CreateProblemDetails includes traceId from Activity.Current.
    /// </summary>
    [Fact]
    public void CreateProblemDetails_ShouldIncludeTraceIdFromActivity()
    {
        // Arrange
        var context = CreateHttpContext();
        var activity = new Activity("TestActivity");
        activity.SetParentId(ActivityTraceId.CreateRandom().ToString());
        activity.Start();

        // Act
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: ProblemDetailsFactoryTestsConstants.StatusCodeNotFound
        );

        // Assert
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].Should().NotBeNull();

        activity.Stop();
    }

    /// <summary>
    /// Tests that CreateProblemDetails includes traceId from HttpContext.TraceIdentifier.
    /// </summary>
    [Fact]
    public void CreateProblemDetails_ShouldIncludeTraceIdFromHttpContext()
    {
        // Arrange
        var context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-id-123";

        // Act
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: ProblemDetailsFactoryTestsConstants.StatusCodeNotFound
        );

        // Assert
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].Should().Be("test-trace-id-123");
    }

    /// <summary>
    /// Tests that CreateProblemDetails includes errorCode from ServiceScaffoldException.
    /// </summary>
    [Fact]
    public void CreateProblemDetails_ShouldIncludeErrorCodeFromServiceScaffoldException()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ServiceScaffoldException("Test error", "TEST_ERROR_CODE");

        // Act
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            context,
            exception,
            statusCode: ProblemDetailsFactoryTestsConstants.StatusCodeBadRequest
        );

        // Assert
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"].Should().Be("TEST_ERROR_CODE");
    }

    /// <summary>
    /// Tests that CreateProblemDetails includes extensions provided in the parameters.
    /// </summary>
    [Fact]
    public void CreateProblemDetails_ShouldIncludeCustomExtensions()
    {
        // Arrange
        var context = CreateHttpContext();
        var extensions = new Dictionary<string, object?>
        {
            ["customField"] = "customValue",
            ["numericField"] = 42,
            ["nullField"] = null
        };

        // Act
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: ProblemDetailsFactoryTestsConstants.StatusCodeBadRequest,
            extensions: extensions
        );

        // Assert
        problemDetails.Extensions.Should().ContainKey("customField");
        problemDetails.Extensions["customField"].Should().Be("customValue");
        problemDetails.Extensions.Should().ContainKey("numericField");
        problemDetails.Extensions["numericField"].Should().Be(42);
        problemDetails.Extensions.Should().ContainKey("nullField");
        problemDetails.Extensions["nullField"].Should().BeNull();
    }

    /// <summary>
    /// Tests that ProblemDetails serializes correctly to JSON with camelCase naming.
    /// </summary>
    [Fact]
    public async Task ProblemDetails_ShouldSerializeToJsonWithCamelCase()
    {
        // Arrange
        var context = CreateHttpContext();
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: ProblemDetailsFactoryTestsConstants.StatusCodeBadRequest,
            title: ProblemDetailsFactoryTestsConstants.BadRequestTitle,
            detail: ProblemDetailsFactoryTestsConstants.InvalidInputDataDetail,
            type: ProblemDetailsFactoryTestsConstants.BadRequestUrl,
            errorCode: ProblemDetailsFactoryTestsConstants.ValidationErrorCode
        );

        // Act
        await context.Response.WriteAsJsonAsync(problemDetails);
        var responseBody = await GetResponseBody(context);

        // Assert
        responseBody.Should().NotBeNullOrEmpty();
        responseBody.Should().Contain("badRequest");
        responseBody.Should().Contain("invalidInputData");
        responseBody.Should().Contain("ProblemDetailsFactoryTestsConstants.BadRequestUrl");
        responseBody.Should().Contain("VALIDATION_ERROR");
    }

    /// <summary>
    /// Tests that ProblemDetails includes all required RFC 7807 fields.
    /// </summary>
    [Fact]
    public void ProblemDetails_ShouldIncludeAllRequiredRfc7807Fields()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: ProblemDetailsFactoryTestsConstants.StatusCodeUnprocessableEntity,
            title: "Unprocessable Entity",
            detail: "Validation failed",
            type: "https://example.com/errors/validation-failed"
        );

        // Assert - RFC 7807 requires: type, title, status, detail, instance
        problemDetails.Type.Should().NotBeNull();
        problemDetails.Title.Should().NotBeNull();
        problemDetails.Status.Should().NotBeNull();
        problemDetails.Detail.Should().NotBeNull();
        problemDetails.Instance.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ProblemDetails content type is correct.
    /// </summary>
    [Fact]
    public void ProblemDetails_ShouldHaveCorrectContentType()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: ProblemDetailsFactoryTestsConstants.StatusCodeBadRequest
        );

        // Assert
        context.Response.ContentType.Should().Be("application/problem+json");
    }
}
