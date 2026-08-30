#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;

namespace DotnetServiceScaffold.Benchmarks
{
    /// <summary>
    /// Constants for <see cref="CacheBenchmarksExtensions"/> to avoid magic values.
    /// </summary>
    internal static class CacheBenchmarksExtensionsConstants
    {
        /// <summary>
        /// Factor used to calculate percentage (100%).
        /// </summary>
        public const double PercentageFactor = 100.0;

        /// <summary>
        /// Number of decimal places for percentage rounding.
        /// </summary>
        public const int PercentageDecimalPlaces = 2;
    }
}