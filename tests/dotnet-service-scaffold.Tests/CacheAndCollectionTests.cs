// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Caching;
using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetServiceScaffold.Tests;

public class CacheAndCollectionTests
{
    [Theory]
    [InlineData("Passw0rd!", true)]
    [InlineData("password", false)]   // no uppercase, digit, or special char
    [InlineData("PASSWORD1!", false)] // no lowercase letter
    [InlineData("Pass1!", false)]     // fewer than 8 characters
    public void IsPasswordStrong_VariousPasswords_ReturnsExpectedStrengthAssessment(
        string password, bool expected)
    {
        ValidationUtility.IsPasswordStrong(password).Should().Be(expected);
    }

    [Fact]
    public void ValidateRange_ValueAboveUpperBound_ThrowsArgumentExceptionWithParamName()
    {
        var act = () => ValidationUtility.ValidateRange(15, 1, 10, "pageSize");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*pageSize*");
    }

    [Fact]
    public void Batch_TenElementsWithBatchSizeThree_ProducesFourBatchesWithCorrectSizes()
    {
        var source = Enumerable.Range(1, 10);

        var batches = source.Batch(3).ToList();

        batches.Should().HaveCount(4);
        batches[0].Should().HaveCount(3);
        batches[1].Should().HaveCount(3);
        batches[2].Should().HaveCount(3);
        batches[3].Should().HaveCount(1); // remainder
    }

    [Fact]
    public void Partition_IntegerCollection_SeparatesEvenAndOddNumbersCorrectly()
    {
        var numbers = new[] { 1, 2, 3, 4, 5, 6 };

        var (evens, odds) = numbers.Partition(n => n % 2 == 0);

        evens.Should().BeEquivalentTo(new[] { 2, 4, 6 });
        odds.Should().BeEquivalentTo(new[] { 1, 3, 5 });
    }

    [Fact]
    public async Task InMemoryCacheService_SetThenGetAsync_ReturnsStoredValue()
    {
        var loggerMock = new Mock<ILogger<InMemoryCacheService>>();
        var cache = new InMemoryCacheService(loggerMock.Object);

        await cache.SetAsync("greeting", "hello-world", TimeSpan.FromMinutes(5));
        var result = await cache.GetAsync<string>("greeting");

        result.Should().Be("hello-world");
    }

    [Fact]
    public async Task InMemoryCacheService_RemoveAsync_DeletesEntryFromCache()
    {
        var loggerMock = new Mock<ILogger<InMemoryCacheService>>();
        var cache = new InMemoryCacheService(loggerMock.Object);

        await cache.SetAsync("temp-key", "some-value");
        await cache.RemoveAsync("temp-key");
        var exists = await cache.ExistsAsync("temp-key");

        exists.Should().BeFalse();
    }
}
