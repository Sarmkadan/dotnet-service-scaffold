#nullable enable

namespace DotnetServiceScaffold.Shared.Models;

/// <summary>
/// Constants for <see cref="ResultValidation"/>.
/// </summary>
internal static class ResultValidationConstants
{
    public const string FailedResultMustHaveNonEmptyErrorMessage = "Failed result must have a non-empty error message.";
    public const string FailedResultMustHaveNonNullErrorCode = "Failed result must have a non-null error code.";
    public const string SuccessfulResultStringMustNotBeEmptyOrWhitespace = "Successful result with string value must not be empty or whitespace.";
    public const string SuccessfulResultMustNotContainDefaultValueFormat = "Successful result must not contain default value of type {0}.";
    public static readonly string NewLine = System.Environment.NewLine;
    public const string ResultValidationFailedPrefix = "Result validation failed:";
    public const string ResultTValidationFailedPrefix = "Result<T> validation failed:";
    public const string ValidationErrorPrefix = "- ";
}