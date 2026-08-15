using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotnetServiceScaffold.Tests
{
    /// <summary>
    /// Extension methods that add utility functionality to <see cref="ExternalApiClientExtensionsTests"/>.
    /// </summary>
    public static class ExternalApiClientExtensionsTestsExtensions
    {
        /// <summary>
        /// Retrieves the names of all public test methods defined on <see cref="ExternalApiClientExtensionsTests"/>.
        /// </summary>
        /// <param name="testClass">The test class instance.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing the method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="testClass"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetTestMethodNames(this ExternalApiClientExtensionsTests testClass)
        {
            ArgumentNullException.ThrowIfNull(testClass);

            var methodNames = typeof(ExternalApiClientExtensionsTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(Task) && m.GetParameters().Length == 0 && !m.IsSpecialName)
                .Select(m => m.Name)
                .ToArray();

            return methodNames;
        }

        /// <summary>
        /// Executes every public test method on the supplied <see cref="ExternalApiClientExtensionsTests"/> instance
        /// sequentially, awaiting each <see cref="Task"/>. If any test throws, the first exception is re‑thrown.
        /// </summary>
        /// <param name="testClass">The test class instance.</param>
        /// <returns>A <see cref="Task"/> that completes when all tests have run.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="testClass"/> is <c>null</c>.</exception>
        /// <exception cref="Exception">The first exception thrown by any test method.</exception>
        public static async Task RunAllTestsAsync(this ExternalApiClientExtensionsTests testClass)
        {
            ArgumentNullException.ThrowIfNull(testClass);

            var methods = typeof(ExternalApiClientExtensionsTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(Task) && m.GetParameters().Length == 0 && !m.IsSpecialName);

            foreach (var method in methods)
            {
                var task = (Task)method.Invoke(testClass, null)!;
                await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a specific test delegate against the supplied <see cref="ExternalApiClientExtensionsTests"/>
        /// instance, ensuring the instance is not <c>null</c>.
        /// </summary>
        /// <typeparam name="TResult">The result type returned by the test delegate.</typeparam>
        /// <param name="testClass">The test class instance.</param>
        /// <param name="testFunc">A delegate that receives the test class and returns a <see cref="Task{TResult}"/>.</param>
        /// <returns>The result produced by <paramref name="testFunc"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="testClass"/> or <paramref name="testFunc"/> is <c>null</c>.
        /// </exception>
        public static async Task<TResult> ExecuteTestAsync<TResult>(this ExternalApiClientExtensionsTests testClass, Func<ExternalApiClientExtensionsTests, Task<TResult>> testFunc)
        {
            ArgumentNullException.ThrowIfNull(testClass);
            ArgumentNullException.ThrowIfNull(testFunc);

            return await testFunc(testClass).ConfigureAwait(false);
        }
    }
}
