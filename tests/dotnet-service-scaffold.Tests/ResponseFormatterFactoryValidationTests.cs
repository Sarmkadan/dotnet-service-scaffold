#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for ResponseFormatterFactoryValidation class
// =============================================================================

namespace DotnetServiceScaffold.Tests.Infrastructure.Formatting;

using Xunit;
using FluentAssertions;
using DotnetServiceScaffold.Infrastructure.Formatting;

/// <summary>
/// Tests for the <see cref="ResponseFormatterFactoryValidation"/> class.
/// </summary>
public class ResponseFormatterFactoryValidationTests
{
    [Fact]
    public void Validate_WithValidFactory_ReturnsEmptyList()
    {
        // Arrange
        var factory = new ResponseFormatterFactory();

        // Act
        var problems = factory.Validate();

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        ResponseFormatterFactory? factory = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory!.Validate());
    }

    [Fact]
    public void IsValid_WithValidFactory_ReturnsTrue()
    {
        // Arrange
        var factory = new ResponseFormatterFactory();

        // Act
        var isValid = factory.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        ResponseFormatterFactory? factory = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory!.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidFactory_ReturnsFalse()
    {
        // Arrange
        var factory = new ResponseFormatterFactory();

        // Make factory invalid by clearing formatters
        var field = typeof(ResponseFormatterFactory).GetField(
            "_formatters",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var formatters = (Dictionary<string, IResponseFormatter>)field!.GetValue(factory)!;
        formatters.Clear();

        // Act
        var isValid = factory.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_WithValidFactory_DoesNotThrow()
    {
        // Arrange
        var factory = new ResponseFormatterFactory();

        // Act
        Action act = () => factory.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange
        ResponseFormatterFactory? factory = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory!.EnsureValid());
    }

    [Fact]
    public void EnsureValid_WithInvalidFactory_ThrowsArgumentException()
    {
        // Arrange
        var factory = new ResponseFormatterFactory();

        // Make factory invalid by clearing formatters
        var field = typeof(ResponseFormatterFactory).GetField(
            "_formatters",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var formatters = (Dictionary<string, IResponseFormatter>)field!.GetValue(factory)!;
        formatters.Clear();

        // Act
        Action act = () => factory.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ResponseFormatterFactory is invalid. Problems:*");
    }


    private class ThrowingFormatter : IResponseFormatter
    {
        public string MediaType => "application/test";

        public Task<string> FormatAsync(object? data) => throw new InvalidOperationException("Test exception");

        public bool CanFormat(string mediaType)
        {
            if (mediaType == null) throw new ArgumentNullException(nameof(mediaType));
            return mediaType.Equals("application/test", StringComparison.OrdinalIgnoreCase);
        }
    }
}