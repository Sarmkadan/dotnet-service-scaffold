// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Models;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

public class ResultTests
{
    [Fact]
    public void Success_NoArguments_ReturnsResultWithIsSuccessTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Failure_WithMessageAndCode_SetsAllErrorProperties()
    {
        var result = Result.Failure("record not found", "ERR_404");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("record not found");
        result.ErrorCode.Should().Be("ERR_404");
    }

    [Fact]
    public void Failure_FromException_CapturesMessageAndUsesTypeNameAsCode()
    {
        var exception = new InvalidOperationException("invalid state transition");

        var result = Result.Failure(exception);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("invalid state transition");
        result.ErrorCode.Should().Be("InvalidOperationException");
    }

    [Fact]
    public void Map_OnSuccessResult_TransformsValueToNewType()
    {
        var source = Result<int>.Success(42);

        var mapped = source.Map(v => $"value:{v}");

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("value:42");
    }

    [Fact]
    public void Map_OnFailureResult_PropagatesErrorWithoutCallingMapper()
    {
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
