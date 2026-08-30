#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Utilities;

namespace DotnetServiceScaffold.Tests
{
    /// <summary>
    /// Contains constant values used in StringUtilityTests.
    /// </summary>
    internal static class StringUtilityTestsConstants
    {
        /// <summary>
        /// The truncate length used in the HelloWorld test.
        /// </summary>
        public const int HelloWorldTruncateLength = 8;

        /// <summary>
        /// The truncate length used in the null or empty input test.
        /// </summary>
        public const int NullOrEmptyTruncateLength = 10;

        /// <summary>
        /// The number of visible characters to keep on each edge when masking sensitive data.
        /// </summary>
        public const int MaskSensitiveVisibleChars = 2;
    }
}