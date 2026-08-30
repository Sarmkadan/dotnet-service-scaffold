#nullable enable

using System;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Contains constant values used in <see cref="PerformanceUtilityValidation"/>.
/// </summary>
internal static class PerformanceUtilityValidationConstants
{
    public const string MustBeNonNegativeGot = " must be non-negative, got ";
    public const string MbSuffix = " MB";
    public const string BytesSuffix = " bytes";
    public const string CannotBeLessThan = " cannot be less than ";
    public static readonly string ValidationHeaderPrefix = " instance is not valid. Validation errors:" + Environment.NewLine + "- ";
    public static readonly string ValidationErrorsSeparator = Environment.NewLine + "- ";
}