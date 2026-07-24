#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Formatting;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class XmlResponseFormatterTests
{
    private readonly XmlResponseFormatter _formatter = new();

    // Simple POCO for happy‑path serialization
    private sealed class SimpleModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    // Model containing an empty collection
    private sealed class ModelWithEmptyList
    {
        public List<int> Numbers { get; set; } = new();
    }

    // A type that XmlSerializer cannot handle (e.g., contains a Stream)
    private sealed class NonSerializableModel
    {
        public Stream? Data { get; set; }
    }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var formatter = new XmlResponseFormatter();

        // Assert
        Assert.NotNull(formatter);
        Assert.Equal("application/xml", formatter.MediaType);
    }

    [Theory]
    [InlineData("application/xml", true)]
    [InlineData("text/xml", true)]
    [InlineData("application/atom+xml", true)]
    [InlineData("APPLICATION/XML", true)] // case‑insensitivity
    [InlineData("application/json", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void CanFormat_ShouldRespectMediaType(string? mediaType, bool expected)
    {
        // Act
        var result = _formatter.CanFormat(mediaType!);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task FormatAsync_NullInput_ShouldReturnEmptyString()
    {
        // Act
        var result = await _formatter.FormatAsync(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task FormatAsync_SimpleObject_ShouldSerializeCorrectly()
    {
        // Arrange
        var model = new SimpleModel { Id = 42, Name = "Test" };

        // Act
        var xml = await _formatter.FormatAsync(model);

        // Assert
        Assert.Contains("<SimpleModel>", xml);
        Assert.Contains("<Id>42</Id>", xml);
        Assert.Contains("<Name>Test</Name>", xml);
        Assert.Contains("</SimpleModel>", xml);
    }

    [Fact]
    public async Task FormatAsync_EmptyCollection_ShouldSerializeWithoutError()
    {
        // Arrange
        var model = new ModelWithEmptyList();

        // Act
        var xml = await _formatter.FormatAsync(model);

        // Assert
        // The XML should contain the root element and an empty <Numbers /> element
        Assert.Contains("<ModelWithEmptyList>", xml);
        Assert.Contains("<Numbers />", xml);
        Assert.Contains("</ModelWithEmptyList>", xml);
    }

    [Fact]
    public async Task FormatAsync_NonSerializableObject_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var model = new NonSerializableModel { Data = new MemoryStream() };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _formatter.FormatAsync(model);
        });
    }
}
