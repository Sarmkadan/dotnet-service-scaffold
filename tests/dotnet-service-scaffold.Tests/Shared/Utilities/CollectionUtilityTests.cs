#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests.Shared.Utilities;

public class CollectionUtilityTests
{
    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueForNullCollection()
    {
        IEnumerable<string>? collection = null;
        CollectionUtility.IsNullOrEmpty(collection).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueForEmptyCollection()
    {
        var collection = new List<string>();
        CollectionUtility.IsNullOrEmpty(collection).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnFalseForNonEmptyCollection()
    {
        var collection = new List<string> { "item1", "item2" };
        CollectionUtility.IsNullOrEmpty(collection).Should().BeFalse();
    }

    [Fact]
    public void GetOrDefault_ShouldReturnElementIfIndexIsValid()
    {
        var list = new List<int> { 1, 2, 3 };
        CollectionUtility.GetOrDefault(list, 1).Should().Be(2);
    }

    [Fact]
    public void GetOrDefault_ShouldReturnDefaultValueIfIndexIsNegative()
    {
        var list = new List<int> { 1, 2, 3 };
        CollectionUtility.GetOrDefault(list, -1).Should().Be(default(int));
    }

    [Fact]
    public void GetOrDefault_ShouldReturnDefaultValueIfIndexIsOutOfRange()
    {
        var list = new List<int> { 1, 2, 3 };
        CollectionUtility.GetOrDefault(list, 5).Should().Be(default(int));
    }

    [Fact]
    public void Paginate_ShouldReturnCorrectPage()
    {
        var items = Enumerable.Range(1, 100).ToList();
        var pageSize = 10;
        var pageNumber = 3; // Expecting items 21-30

        var paginated = CollectionUtility.Paginate(items, pageNumber, pageSize);

        paginated.Should().HaveCount(pageSize);
        paginated.First().Should().Be(21);
        paginated.Last().Should().Be(30);
    }

    [Fact]
    public void Paginate_ShouldReturnEmptyListForOutOfRangePageNumber()
    {
        var items = Enumerable.Range(1, 10).ToList();
        var pageSize = 5;
        var pageNumber = 3; // Page 3 would be items 11-15, which don't exist

        var paginated = CollectionUtility.Paginate(items, pageNumber, pageSize);

        paginated.Should().BeEmpty();
    }
}
