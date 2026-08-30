#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetServiceScaffold.Tests
{
    /// <summary>
    /// Constants for CacheAndCollectionTests to avoid magic values.
    /// </summary>
    internal static class CacheAndCollectionTestsConstants
    {
        public const string TestCacheKeyGreeting = "greeting";
        public const string TestCacheValueGreeting = "hello-world";
        public const string TestCacheKeyTemp = "temp-key";
        public const string TestValidateRangeParamName = "pageSize";
        public const int DefaultCacheExpirationMinutes = 5;
        public const int DefaultBatchSize = 3;
    }
}