using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Formatting;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Verifies CSV formatting behavior and provides value-based equality for the test fixture's public data properties.
/// </summary>
public class CsvResponseFormatterTests : ICsvResponseFormatterTests, IEquatable<CsvResponseFormatterTests>
{
    private readonly CsvResponseFormatter _formatter;

    /// <summary>
    /// Gets or sets the identifier used when comparing test fixture instances.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the name used when comparing test fixture instances.
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// Gets or sets the numeric value used when comparing test fixture instances.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Initializes a test fixture with a new CSV response formatter.
    /// </summary>
    public CsvResponseFormatterTests()
    {
        _formatter = new CsvResponseFormatter();
    }

    /// <summary>
    /// Determines whether another test fixture has the same identifier, name, and numeric value.
    /// </summary>
    /// <param name="other">The test fixture to compare with this instance.</param>
    /// <returns><see langword="true"/> when all three public data properties are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(CsvResponseFormatterTests? other)
    {
        if (other is null)
            return false;

        return Id == other.Id &&
               Name == other.Name &&
               Value == other.Value;
    }

    /// <summary>
    /// Determines whether an object is a test fixture with the same identifier, name, and numeric value.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns><see langword="true"/> when the object is an equal test fixture; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as CsvResponseFormatterTests);
    }

    /// <summary>
    /// Computes a hash code from the identifier, name, and numeric value.
    /// </summary>
    /// <returns>A hash code for the fixture's public data properties.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Value);
    }

    /// <summary>
    /// Determines whether two test fixtures are equal using their value-based equality implementation.
    /// </summary>
    /// <param name="left">The first test fixture to compare.</param>
    /// <param name="right">The second test fixture to compare.</param>
    /// <returns><see langword="true"/> when the fixtures are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(CsvResponseFormatterTests? left, CsvResponseFormatterTests? right)
    {
        return EqualityComparer<CsvResponseFormatterTests>.Default.Equals(left, right);
    }

    /// <summary>
    /// Determines whether two test fixtures differ using their value-based equality implementation.
    /// </summary>
    /// <param name="left">The first test fixture to compare.</param>
    /// <param name="right">The second test fixture to compare.</param>
    /// <returns><see langword="true"/> when the fixtures are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(CsvResponseFormatterTests? left, CsvResponseFormatterTests? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Verifies that formatting a list of simple objects produces a header and one CSV row per object.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
        Assert.Contains(CsvResponseFormatterTestsConstants.CsvHeader, result);
        Assert.Contains("1,Alice,100.5", result);
        Assert.Contains("2,Bob,200", result);
    }

    /// <summary>
    /// Verifies that commas, quotes, line breaks, and tabs in string values are escaped in the CSV output.
    /// </summary>
    /// <param name="input">The string value containing a character that requires CSV escaping.</param>
    /// <param name="expectedOutput">The escaped representation expected in the formatted CSV.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that formatting an empty collection produces an empty string.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task FormatAsync_EmptyCollection_ReturnsEmptyString()
    {
        // Arrange
        var data = new List<TestDto>();

        // Act
        var result = await _formatter.FormatAsync(data);

        // Assert
        // The current implementation returns string.Empty for empty collections
        Assert.Equal(CsvResponseFormatterTestsConstants.EmptyString, result);
    }

    /// <summary>
    /// Verifies that a null property value is represented by an empty CSV field.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that formatting a null input produces an empty string.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task FormatAsync_NullInput_ReturnsEmptyString()
    {
        // Act
        var result = await _formatter.FormatAsync(null);

        // Assert
        Assert.Equal(CsvResponseFormatterTestsConstants.EmptyString, result);
    }

    /// <summary>
    /// Verifies that formatting a single object produces a header and one CSV row.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task FormatAsync_SingleObject_ReturnsHeaderAndOneRow()
    {
        // Arrange
        var data = new TestDto { Id = 1, Name = "Single", Value = 50 };

        // Act
        var result = await _formatter.FormatAsync(data);

        // Assert
        Assert.Contains(CsvResponseFormatterTestsConstants.CsvHeader, result);
        Assert.Contains("1,Single,50", result);
    }

    /// <summary>
    /// Creates a concise representation containing the fixture's identifier, name, and numeric value.
    /// </summary>
    /// <returns>A string containing the type name and current public data property values.</returns>
    public override string ToString()
    {
        return $"CsvResponseFormatterTests {{ Id = {Id}, Name = {Name}, Value = {Value} }}";
    }

    private class TestDto
    {
        /// <summary>
        /// Gets or sets the identifier written to the first CSV column.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the optional name written to the second CSV column.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the numeric value written to the third CSV column.
        /// </summary>
        public double Value { get; set; }
    }
}
