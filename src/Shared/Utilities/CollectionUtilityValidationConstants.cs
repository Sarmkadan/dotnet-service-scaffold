#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Centralizes the magic values used by <see cref="CollectionUtilityValidation"/> so they have a single source of truth.
/// </summary>
internal static class CollectionUtilityValidationConstants
{
    /// <summary>
    /// Default batch size applied when a caller does not specify one.
    /// </summary>
    public const int DefaultBatchSize = 1;

    /// <summary>
    /// Maximum allowed batch or chunk size for collection operations.
    /// </summary>
    public const int MaxBatchOrChunkSize = 1_000_000;

    /// <summary>
    /// Message reported when the source collection contains no elements.
    /// </summary>
    public const string SourceCollectionEmpty = "Source collection is empty.";

    /// <summary>
    /// Message reported when the first collection of a pairwise operation contains no elements.
    /// </summary>
    public const string FirstCollectionEmpty = "First collection is empty.";

    /// <summary>
    /// Message reported when the second collection of a pairwise operation contains no elements.
    /// </summary>
    public const string SecondCollectionEmpty = "Second collection is empty.";

    /// <summary>
    /// Message reported when the batch size is not a positive integer.
    /// </summary>
    public const string BatchSizeMustBePositive = "Batch size must be a positive integer.";

    /// <summary>
    /// Message reported when the chunk size is not a positive integer.
    /// </summary>
    public const string ChunkSizeMustBePositive = "Chunk size must be a positive integer.";

    /// <summary>
    /// Message reported when the batch size exceeds <see cref="MaxBatchOrChunkSize"/>.
    /// </summary>
    public const string BatchSizeExceedsMaximum = "Batch size is excessively large (maximum 1,000,000).";

    /// <summary>
    /// Message reported when the chunk size exceeds <see cref="MaxBatchOrChunkSize"/>.
    /// </summary>
    public const string ChunkSizeExceedsMaximum = "Chunk size is excessively large (maximum 1,000,000).";

    /// <summary>
    /// Message reported when the predicate function is <see langword="null"/>.
    /// </summary>
    public const string PredicateCannotBeNull = "Predicate function cannot be null.";

    /// <summary>
    /// Message reported when the key selector function is <see langword="null"/>.
    /// </summary>
    public const string KeySelectorCannotBeNull = "Key selector function cannot be null.";

    /// <summary>
    /// Header line of the aggregate validation failure message.
    /// </summary>
    public const string ValidationFailedHeader = "Collection operation validation failed:";

    /// <summary>
    /// Bullet prefix placed before each problem in the aggregate validation failure message.
    /// </summary>
    public const string ProblemBullet = "- ";
}
