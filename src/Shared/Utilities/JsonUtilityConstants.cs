#nullable enable

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Constants for JsonUtility class.
/// </summary>
internal static class JsonUtilityConstants
{
    /// <summary>
    /// Error message prefix for deserialization failures.
    /// </summary>
    public const string DeserializeErrorMessage = "Failed to deserialize JSON: ";

    /// <summary>
    /// Error message prefix for merge failures.
    /// </summary>
    public const string MergeErrorMessage = "Failed to merge JSON: ";
}