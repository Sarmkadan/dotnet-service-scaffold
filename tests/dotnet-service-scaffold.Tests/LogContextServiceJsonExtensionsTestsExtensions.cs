using System;
using System.Collections.Generic;
using DotnetServiceScaffold.Infrastructure.Logging;
using Xunit;

namespace DotnetServiceScaffold.Tests.Logging
{
    /// <summary>
    /// Provides extension methods for <see cref="LogContextServiceJsonExtensionsTests"/> to facilitate testing.
    /// </summary>
    public static class LogContextServiceJsonExtensionsTestsExtensions
    {
        /// <summary>
        /// Asserts that two <see cref="LogContextService"/> instances have identical properties.
        /// </summary>
        /// <param name="_">The test class instance.</param>
        /// <param name="expected">The expected <see cref="LogContextService"/>.</param>
        /// <param name="actual">The actual <see cref="LogContextService"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="expected"/> or <paramref name="actual"/> is null.</exception>
        public static void AssertPropertiesMatch(this LogContextServiceJsonExtensionsTests _, LogContextService expected, LogContextService actual)
        {
            ArgumentNullException.ThrowIfNull(expected);
            ArgumentNullException.ThrowIfNull(actual);

            var expectedProps = expected.GetProperties();
            var actualProps = actual.GetProperties();

            Assert.Equal(expectedProps.Count, actualProps.Count);
            foreach (var kvp in expectedProps)
            {
                Assert.True(actualProps.ContainsKey(kvp.Key), $"Actual service missing expected key: {kvp.Key}");
                Assert.Equal(kvp.Value, actualProps[kvp.Key]);
            }
        }

        /// <summary>
        /// Creates a <see cref="LogContextService"/> populated with the specified properties.
        /// </summary>
        /// <param name="_">The test class instance.</param>
        /// <param name="properties">The properties to add.</param>
        /// <returns>A populated <see cref="LogContextService"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="properties"/> is null.</exception>
        public static LogContextService CreateServiceWithProperties(this LogContextServiceJsonExtensionsTests _, Dictionary<string, object?> properties)
        {
            ArgumentNullException.ThrowIfNull(properties);

            var service = new LogContextService();
            foreach (var kvp in properties)
            {
                service.AddProperty(kvp.Key, kvp.Value);
            }
            return service;
        }
    }
}
