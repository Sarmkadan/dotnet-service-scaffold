#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Formatting;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public sealed class JsonResponseFormatterTests
{
    private readonly JsonResponseFormatter _formatter = new();

    [Fact]
    public void Constructor_ShouldInitializeMediaType()
    {
        Assert.Equal("application/json", _formatter.MediaType);
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("application/json; charset=utf-8")]
    [InlineData("application/vnd.myapi+json")]
    [InlineData("application/json+custom")]
    public void CanFormat_ValidMediaTypes_ReturnsTrue(string mediaType)
    {
        Assert.True(_formatter.CanFormat(mediaType));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("text/plain")]
    [InlineData("application/xml")]
    public void CanFormat_InvalidMediaTypes_ReturnsFalse(string? mediaType)
    {
        Assert.False(_formatter.CanFormat(mediaType ?? string.Empty));
    }

    [Fact]
    public async Task FormatAsync_NullInput_ReturnsStringNull()
    {
        var result = await _formatter.FormatAsync(null);
        Assert.Equal("null", result);
    }

    [Fact]
    public async Task FormatAsync_SimpleObject_ReturnsJson()
    {
        var payload = new { Name = "Alice", Age = 30 };
        var json = await _formatter.FormatAsync(payload);
        Assert.Equal("{\"name\":\"Alice\",\"age\":30}", json);
    }

    [Fact]
    public async Task FormatAsync_EmptyCollection_ReturnsEmptyArray()
    {
        var emptyList = new List<int>();
        var json = await _formatter.FormatAsync(emptyList);
        Assert.Equal("[]", json);
    }

    [Fact]
    public async Task FormatAsync_ObjectWithDateTime_UsesUtcIso8601()
    {
        var local = new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Local);
        var payload = new { Timestamp = local };
        var json = await _formatter.FormatAsync(payload);

        var expected = $"{{\"timestamp\":\"{local.ToUniversalTime():o}\"}}";
        Assert.Equal(expected, json);
    }

    [Fact]
    public async Task FormatAsync_NonSerializableObject_ThrowsInvalidOperationException()
    {
        var payload = new { Action = (Action)(() => { }) };
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _formatter.FormatAsync(payload));
    }
}
