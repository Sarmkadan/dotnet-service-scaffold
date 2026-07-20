using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Formatting;

namespace DotnetServiceScaffold.Tests;

public class CsvResponseFormatterTests
{
    private readonly CsvResponseFormatter _formatter;

    public CsvResponseFormatterTests()
    {
        _formatter = new CsvResponseFormatter();
    }

    [Fact]
    public async Task FormatAsync_SimpleObjectList_ReturnsCorrectCsv()
    {
        // Arrange
        var data = new List<TestDto>
        {
            new TestDto { Id = 1, Name = "Alice", Value = 100.50 },
            new TestDto { Id = 2, Name = "Bob", Value = 200.00 }
        };

        // Act
        var result = await _formatter.FormatAsync(data);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Id,Name,Value", result);
        Assert.Contains("1,Alice,100.5", result);
        Assert.Contains("2,Bob,200", result);
    }

    [Theory]
    [InlineData("Doe, John", "\"Doe, John\"")]
    [InlineData("He said \"Hi\"", "\"He said \"\"Hi\"\"\"")]
    [InlineData("Line1\nLine2", "\"Line1\nLine2\"")]
    [InlineData("Tab\there", "\"Tab\there\"")]
    public async Task FormatAsync_SpecialCharacters_AreEscaped(string input, string expectedOutput)
    {
        // Arrange
        var data = new List<TestDto>
        {
            new TestDto { Id = 1, Name = input, Value = 1 }
        };

        // Act
        var result = await _formatter.FormatAsync(data);

        // Assert
        Assert.Contains(expectedOutput, result);
    }

    [Fact]
    public async Task FormatAsync_EmptyCollection_ReturnsEmptyString()
    {
        // Arrange
        var data = new List<TestDto>();

        // Act
        var result = await _formatter.FormatAsync(data);

        // Assert
        // The current implementation returns string.Empty for empty collections
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task FormatAsync_NullPropertyValues_HandlesCorrectly()
    {
        // Arrange
        var data = new List<TestDto>
        {
            new TestDto { Id = 1, Name = null, Value = 100 }
        };

        // Act
        var result = await _formatter.FormatAsync(data);

        // Assert
        // Expected format: Id,Name,Value \n 1,,100
        Assert.Contains("1,,100", result);
    }

    [Fact]
    public async Task FormatAsync_NullInput_ReturnsEmptyString()
    {
        // Act
        var result = await _formatter.FormatAsync(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task FormatAsync_SingleObject_ReturnsHeaderAndOneRow()
    {
        // Arrange
        var data = new TestDto { Id = 1, Name = "Single", Value = 50 };

        // Act
        var result = await _formatter.FormatAsync(data);

        // Assert
        Assert.Contains("Id,Name,Value", result);
        Assert.Contains("1,Single,50", result);
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double Value { get; set; }
    }
}
