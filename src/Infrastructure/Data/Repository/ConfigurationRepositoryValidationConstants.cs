#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository
{
    /// <summary>
    /// Constants for <see cref="ConfigurationRepositoryValidation"/>.
    /// </summary>
    internal static class ConfigurationRepositoryValidationConstants
    {
        public const string KeyNullOrWhitespace = "Configuration Key cannot be null or whitespace.";
        public const string KeyExceedsMaxLength = "Configuration Key cannot exceed 255 characters.";
        public const string ValueNullOrWhitespace = "Configuration Value cannot be null or whitespace.";
        public const string ValueExceedsMaxLength = "Configuration Value cannot exceed 4000 characters.";
        public const string ConfigTypeExceedsMaxLength = "Configuration Type cannot exceed 50 characters.";
        public const string DescriptionExceedsMaxLength = "Configuration Description cannot exceed 1000 characters.";
        public const string CreatedAtMustBeSet = "Configuration CreatedAt must be set to a valid date.";
        public const string CreatedAtCannotBeFuture = "Configuration CreatedAt cannot be in the future.";
        public const string UpdatedAtMustBeSet = "Configuration UpdatedAt must be set to a valid date.";
        public const string UpdatedAtCannotBeFuture = "Configuration UpdatedAt cannot be in the future.";
        public const string ServiceIdEmptyGuid = "Configuration ServiceId cannot be an empty GUID.";
        public const string ValidationFailedPrefix = "Configuration validation failed:";

        public const int KeyMaxLength = 255;
        public const int ValueMaxLength = 4000;
        public const int ConfigTypeMaxLength = 50;
        public const int DescriptionMaxLength = 1000;
        public const int TimestampFutureMinutes = 5;
    }
}