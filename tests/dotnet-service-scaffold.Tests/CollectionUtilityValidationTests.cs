using System;
using System.Collections.Generic;
using System.Linq;
using DotnetServiceScaffold.Shared.Utilities;
using Xunit;

namespace DotnetServiceScaffold.Tests;

public class CollectionUtilityValidationTests
{
    private static readonly List<int> SampleList = Enumerable.Range(1, 5).ToList();

    [Fact]
    public void ValidateBatchSize_HappyPath_ReturnsEmptyList()
    {
        var result = CollectionUtilityValidation.Validate(SampleList, batchSize: 10);
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateBatchSize_InvalidBatchSize_ReturnsErrorMessages()
    {
        // batchSize <= 0
        var resultLow = CollectionUtilityValidation.Validate(SampleList, batchSize: 0);
        Assert.Contains("Batch size must be a positive integer.", resultLow);

        // batchSize > 1_000_000
        var resultHigh = CollectionUtilityValidation.Validate(SampleList, batchSize: 1_000_001);
        Assert.Contains("Batch size is excessively large (maximum 1,000,000).", resultHigh);
    }

    [Fact]
    public void ValidateBatchSize_NullSource_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CollectionUtilityValidation.Validate<int>(null, batchSize: 10));
    }

    [Fact]
    public void ValidateFirstSecond_EmptyCollections_ReturnsErrorMessages()
    {
        var first = Enumerable.Empty<int>();
        var second = Enumerable.Empty<int>();

        var result = CollectionUtilityValidation.Validate(first, second);
        Assert.Contains("First collection is empty.", result);
        Assert.Contains("Second collection is empty.", result);
    }

    [Fact]
    public void ValidateFirstSecond_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<int> nullList = null!;
        var second = SampleList;

        Assert.Throws<ArgumentNullException>(() => CollectionUtilityValidation.Validate(nullList, second));
        Assert.Throws<ArgumentNullException>(() => CollectionUtilityValidation.Validate(SampleList, nullList));
    }

    [Fact]
    public void ValidatePredicate_NullPredicate_ReturnsErrorMessage()
    {
        var result = CollectionUtilityValidation.Validate(SampleList, (Func<int, bool>)null!);
        Assert.Contains("Predicate function cannot be null.", result);
    }

    [Fact]
    public void ValidateKeySelector_NullKeySelector_ReturnsErrorMessage()
    {
        var result = CollectionUtilityValidation.Validate(SampleList, (Func<int, string>)null!);
        Assert.Contains("Key selector function cannot be null.", result);
    }

    [Fact]
    public void IsValidBatchSize_Invalid_ReturnsFalse()
    {
        var isValid = CollectionUtilityValidation.IsValid(SampleList, batchSize: 0);
        Assert.False(isValid);
    }

    [Fact]
    public void IsValidFirstSecond_Valid_ReturnsTrue()
    {
        var first = SampleList;
        var second = SampleList.Select(x => x * 2);
        var isValid = CollectionUtilityValidation.IsValid(first, second);
        Assert.True(isValid);
    }
}
