using System;
using DotnetServiceScaffold.Shared.Extensions;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class ExceptionExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidException_ReturnsNonEmptyJson()
    {
        // Arrange
        var ex = new InvalidOperationException("Test message");

        // Act
        var json = ex.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.Contains("test message", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("invalidoperationexception", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToJson_WithIndentation_ReturnsIndentedJson()
    {
        // Arrange
        var ex = new ArgumentException("Indented test");

        // Act
        var json = ex.ToJson(indented: true);

        // Assert
        Assert.Contains("\n", json); // Indented JSON contains line breaks
    }

    [Fact]
    public void ToJson_NullException_ThrowsArgumentNullException()
    {
        // Arrange
        Exception? nullEx = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullEx!.ToJson());
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsExceptionWithSameMessage()
    {
        // Arrange
        var original = new InvalidOperationException("Original message");
        var json = original.ToJson();

        // Act
        var deserialized = ExceptionExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Message, deserialized!.Message);
    }

    [Fact]
    public void FromJson_WithEmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExceptionExtensionsJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_WithMalformedJson_ReturnsNull()
    {
        // Arrange
        var malformedJson = "{ this is not valid json }";

        // Act
        var result = ExceptionExtensionsJsonExtensions.FromJson(malformedJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndException()
    {
        // Arrange
        var original = new ArgumentNullException("paramName", "Param is null");
        var json = original.ToJson();

        // Act
        var success = ExceptionExtensionsJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        Assert.True(success);
        Assert.NotNull(deserialized);
        Assert.Equal(original.Message, deserialized!.Message);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "not a json";

        // Act
        var success = ExceptionExtensionsJsonExtensions.TryFromJson(invalidJson, out var deserialized);

        // Assert
        Assert.False(success);
        Assert.Null(deserialized);
    }

    [Fact]
    public void TryFromJson_WithWhiteSpaceString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ExceptionExtensionsJsonExtensions.TryFromJson("   ", out _));
    }
}
