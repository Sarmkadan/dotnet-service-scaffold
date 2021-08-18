#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Models;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Tests for the <see cref="Result"/> class.
/// </summary>
public class ResultTests
{
    [Fact]
    public void Success_NoArguments_ReturnsResultWithIsSuccessTrue()
    {
        /// <summary>
        /// Verifies that calling <see cref="Result.Success()"/> returns a <see cref="Result"/> with <see cref="IsSuccess"/> set to <c>true</c>.
        /// </summary>
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Failure_WithMessageAndCode_SetsAllErrorProperties()
    {
        /// <summary>
        /// Verifies that calling <see cref="Result.Failure(string, string)"/> sets all error properties.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="code">The error code.</param>
        var result = Result.Failure("record not found", "ERR_404");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("record not found");
        result.ErrorCode.Should().Be("ERR_404");
    }

    [Fact]
    public void Failure_FromException_CapturesMessageAndUsesTypeNameAsCode()
    {
        /// <summary>
        /// Verifies that calling <see cref="Result.Failure(Exception)"/> captures the exception message and uses the type name as the error code.
        /// </summary>
        /// <param name="exception">The exception to capture.</param>
        var exception = new InvalidOperationException("invalid state transition");

        var result = Result.Failure(exception);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("invalid state transition");
        result.ErrorCode.Should().Be("InvalidOperationException");
    }

    [Fact]
    public void Map_OnSuccessResult_TransformsValueToNewType()
    {
        /// <summary>
        /// Verifies that calling <see cref="Result{T}.Map(Func{T, TResult})"/> on a successful <see cref="Result{T}"/> transforms the value to the new type.
        /// </summary>
        /// <param name="source">The source <see cref="Result{T}"/>.</param>
        var source = Result<int>.Success(42);

        var mapped = source.Map(v => $"value:{v}");

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("value:42");
    }

    [Fact]
    public void Map_OnFailureResult_PropagatesErrorWithoutCallingMapper()
    {
        /// <summary>
        /// Verifies that calling <see cref="Result{T}.Map(Func{T, TResult})"/> on a failed <see cref="Result{T}"/> propagates the error without calling the mapper.
        /// </summary>
        /// <param name="source">The source <see cref="Result{T}"/>.</param>
        var source = Result<int>.Failure("upstream failure", "ERR_SRC");
        var mapperInvoked = false;

        var mapped = source.Map(v =>
        {
            mapperInvoked = true;
            return v.ToString()!;
        });

        mapped.IsSuccess.Should().BeFalse();
        mapped.ErrorMessage.Should().Be("upstream failure");
        mapped.ErrorCode.Should().Be("ERR_SRC");
        mapperInvoked.Should().BeFalse();
    }
}
