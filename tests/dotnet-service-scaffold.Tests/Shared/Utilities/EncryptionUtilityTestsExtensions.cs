using System;
using FluentAssertions;
using DotnetServiceScaffold.Tests.Shared.Utilities;

namespace DotnetServiceScaffold.Tests.Shared.Utilities
{
    /// <summary>
    /// Extension methods for enhancing EncryptionUtility test assertions.
    /// </summary>
    public static class EncryptionUtilityTestsExtensions
    {
        /// <summary>
        /// Asserts that an action throws a specific exception with the given message.
        /// </summary>
        /// <typeparam name="T">Type of exception to expect</typeparam>
        /// <param name="act">The action to execute</param>
        /// <param name="expectedMessage">Expected exception message</param>
        public static void ShouldThrowWithMessage<T>(this Action act, string expectedMessage) where T : Exception
        {
            act.Should().Throw<T>()
                .WithMessage(expectedMessage);
        }

        /// <summary>
        /// Asserts that a string is a valid URL-safe Base64 string.
        /// </summary>
        /// <param name="value">The string to validate</param>
        public static void ShouldBeUrlSafeBase64(this string value)
        {
            value.Should().NotBeNull();
            value.Should().NotBeNullOrEmpty();
            value.Should().NotContain("+");
            value.Should().NotContain("/");
            value.Should().NotContain("=");
        }

        /// <summary>
        /// Asserts that a string is a valid hexadecimal representation.
        /// </summary>
        /// <param name="value">The string to validate</param>
        public static void ShouldBeHexadecimal(this string value)
        {
            value.Should().MatchRegex("^[0-9a-f]+$");
        }
    }
}
