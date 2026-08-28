#nullable enable
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

public interface IProblemDetailsFactoryTests
{
    void CreateProblemDetails_ShouldCreateValidProblemDetails();
    void CreateProblemDetails_ShouldSetDefaultTypeToAboutBlank();
    void CreateProblemDetails_ShouldIncludeTraceIdFromActivity();
    void CreateProblemDetails_ShouldIncludeTraceIdFromHttpContext();
    void CreateProblemDetails_ShouldIncludeErrorCodeFromServiceScaffoldException();
    void CreateProblemDetails_ShouldIncludeCustomExtensions();
    Task ProblemDetails_ShouldSerializeToJsonWithCamelCase();
    void ProblemDetails_ShouldIncludeAllRequiredRfc7807Fields();
    void ProblemDetails_ShouldHaveCorrectContentType();
}