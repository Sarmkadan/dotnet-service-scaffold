using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Shared.Models;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ResultValidationTests
{
    #region Validate (non‑generic)

    [Fact]
    public void Validate_SuccessResult_ReturnsEmptyList()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var problems = result.Validate();

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_FailureResult_WithMessageAndCode_ReturnsEmptyList()
    {
        // Arrange
        var result = Result.Failure("Something went wrong", "ERR001");

        // Act
        var problems = result.Validate();

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_FailureResult_MissingMessageOrCode_ReturnsProblems()
    {
        // Arrange
        var result = Result.Failure(string.Empty, null!);

        // Act
        var problems = result.Validate();

        // Assert
        Assert.Contains("Failed result must have a non-empty ErrorMessage.", problems);
        Assert.Contains("Failed result must have a non-null ErrorCode.", problems);
        Assert.Equal(2, problems.Count);
    }

    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        // Arrange
        Result? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.Validate());
    }

    #endregion

    #region Validate<T> (generic)

    [Fact]
    public void ValidateT_SuccessResult_WithValidString_ReturnsEmptyList()
    {
        var result = Result<string>.Success("hello");
        var problems = result.Validate<string>();
        Assert.Empty(problems);
    }

    [Fact]
    public void ValidateT_SuccessResult_WithEmptyString_ReturnsProblem()
    {
        var result = Result<string>.Success(string.Empty);
        var problems = result.Validate<string>();
        Assert.Contains("Successful result with string value must not be empty or whitespace.", problems);
        Assert.Single(problems);
    }

    [Fact]
    public void ValidateT_SuccessResult_WithDefaultFormattableReference_ReturnsProblem()
    {
        // Using a nullable DateTime (reference type) that is default (null)
        Result<DateTime?> result = Result<DateTime?>.Success(default);
        var problems = result.Validate<DateTime?>();
        // Since DateTime? is a reference type and implements IFormattable, the default check fires
        Assert.Contains("Successful result must not contain default value of type Nullable`1.", problems);
    }

    [Fact]
    public void ValidateT_FailureResult_MissingMessageOrCode_ReturnsProblems()
    {
        var result = Result<int>.Failure(string.Empty, null!);
        var problems = result.Validate<int>();
        Assert.Contains("Failed result must have a non-empty ErrorMessage.", problems);
        Assert.Contains("Failed result must have a non-null ErrorCode.", problems);
        Assert.Equal(2, problems.Count);
    }

    [Fact]
    public void ValidateT_Null_ThrowsArgumentNullException()
    {
        Result<int>? result = null;
        Assert.Throws<ArgumentNullException>(() => result!.Validate<int>());
    }

    #endregion

    #region IsValid

    [Fact]
    public void IsValid_SuccessResult_ReturnsTrue()
    {
        var result = Result.Success();
        Assert.True(result.IsValid());
    }

    [Fact]
    public void IsValid_FailureResult_WithProblems_ReturnsFalse()
    {
        var result = Result.Failure(string.Empty, null!);
        Assert.False(result.IsValid());
    }

    [Fact]
    public void IsValidT_SuccessResult_ReturnsTrue()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsValid<int>());
    }

    [Fact]
    public void IsValidT_FailureResult_WithProblems_ReturnsFalse()
    {
        var result = Result<string>.Failure(string.Empty, null!);
        Assert.False(result.IsValid<string>());
    }

    #endregion

    #region EnsureValid

    [Fact]
    public void EnsureValid_SuccessResult_DoesNotThrow()
    {
        var result = Result.Success();
        var exception = Record.Exception(() => result.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_FailureResult_WithProblems_ThrowsArgumentException()
    {
        var result = Result.Failure(string.Empty, null!);
        var ex = Assert.Throws<ArgumentException>(() => result.EnsureValid());
        Assert.Contains("Failed result must have a non-empty ErrorMessage.", ex.Message);
        Assert.Contains("Failed result must have a non-null ErrorCode.", ex.Message);
    }

    [Fact]
    public void EnsureValid_Null_ThrowsArgumentNullException()
    {
        Result? result = null;
        Assert.Throws<ArgumentNullException>(() => result!.EnsureValid());
    }

    [Fact]
    public void EnsureValidT_SuccessResult_DoesNotThrow()
    {
        var result = Result<string>.Success("valid");
        var exception = Record.Exception(() => result.EnsureValid<string>());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValidT_FailureResult_WithProblems_ThrowsArgumentException()
    {
        var result = Result<int>.Failure(string.Empty, null!);
        var ex = Assert.Throws<ArgumentException>(() => result.EnsureValid<int>());
        Assert.Contains("Failed result must have a non-empty ErrorMessage.", ex.Message);
        Assert.Contains("Failed result must have a non-null ErrorCode.", ex.Message);
    }

    #endregion
}
