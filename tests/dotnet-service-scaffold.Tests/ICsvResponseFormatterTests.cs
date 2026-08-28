namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Interface for CSV response formatter tests.
/// </summary>
public interface ICsvResponseFormatterTests
{
    Task FormatAsync_SimpleObjectList_ReturnsCorrectCsv();
    Task FormatAsync_SpecialCharacters_AreEscaped(string input, string expectedOutput);
    Task FormatAsync_EmptyCollection_ReturnsEmptyString();
    Task FormatAsync_NullPropertyValues_HandlesCorrectly();
    Task FormatAsync_NullInput_ReturnsEmptyString();
    Task FormatAsync_SingleObject_ReturnsHeaderAndOneRow();
}