#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Contract for tests of the CollectionUtility class.
/// </summary>
public interface ICollectionUtilityTests
{
    void IsNullOrEmpty_ShouldReturnTrueForNullCollection();

    void IsNullOrEmpty_ShouldReturnTrueForEmptyCollection();

    void IsNullOrEmpty_ShouldReturnFalseForNonEmptyCollection();

    void GetOrDefault_ShouldReturnElementIfIndexIsValid();

    void GetOrDefault_ShouldReturnDefaultValueIfIndexIsNegative();

    void GetOrDefault_ShouldReturnDefaultValueIfIndexIsOutOfRange();

    void Paginate_ShouldReturnCorrectPage();

    void Paginate_ShouldReturnEmptyListForOutOfRangePageNumber();
}