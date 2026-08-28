#nullable enable
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

/// <summary>
/// Contains unit tests for cache and collection utilities.
/// </summary>
public interface ICacheAndCollectionTests
{
    void IsPasswordStrong_VariousPasswords_ReturnsExpectedStrengthAssessment(string password, bool expected);
    void ValidateRange_ValueAboveUpperBound_ThrowsArgumentExceptionWithParamName();
    void Batch_TenElementsWithBatchSizeThree_ProducesFourBatchesWithCorrectSizes();
    void Partition_IntegerCollection_SeparatesEvenAndOddNumbersCorrectly();
    Task InMemoryCacheService_SetThenGetAsync_ReturnsStoredValue();
    Task InMemoryCacheService_RemoveAsync_DeletesEntryFromCache();
}