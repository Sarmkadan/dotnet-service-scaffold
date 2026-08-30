#nullable enable

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Constants for DateTimeUtilityValidation.
/// </summary>
internal static class DateTimeUtilityValidationConstants
{
    public const string DateTimeCannotBeDefault = "DateTime value cannot be default (DateTime.MinValue).";
    public const string DateTimeMustHaveSpecifiedKind = "DateTime value must have a specified kind (UTC or Local).";
    public const string DurationStringCannotBeNullOrWhitespace = "Duration string cannot be null or whitespace.";
    public const string InvalidIsoDurationFormat = "Duration string '{0}' is not a valid ISO 8601 duration format. Expected format: PnYnMnDTnHnMnS (e.g., P3DT4H5M6S).";
    public const string DurationValuesTooLarge = "Duration string '{0}' contains values that are too large.";
    public const string BirthDateCannotBeInFuture = "Birth date cannot be in the future.";
    public const string BirthDateResultsInNegativeAge = "Birth date results in a negative age, which is not valid.";
}