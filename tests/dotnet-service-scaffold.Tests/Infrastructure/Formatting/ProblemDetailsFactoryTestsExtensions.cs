using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Extension methods that make it easier to work with <see cref="ProblemDetailsFactoryTests"/> in test suites.
/// </summary>
namespace dotnet_service_scaffold.Tests.Infrastructure.Formatting
{
    /// <summary>
    /// Provides helper methods for executing and inspecting the public test members of <see cref="ProblemDetailsFactoryTests"/>.
    /// </summary>
    public static class ProblemDetailsFactoryTestsExtensions
    {
        /// <summary>
        /// Executes all public test methods on the supplied <paramref name="tests"/> instance and returns a
        /// read‑only dictionary that maps each method name to a <c>true</c> value if the method completed without
        /// throwing, or <c>false</c> otherwise.
        /// </summary>
        /// <param name="tests">The <see cref="ProblemDetailsFactoryTests"/> instance whose test methods should be run.</param>
        /// <returns>A read‑only dictionary of method names and their pass/fail status.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<string, bool> RunAllProblemDetailsTests(this ProblemDetailsFactoryTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var results = new Dictionary<string, bool>(StringComparer.Ordinal);

            // Select only the public instance methods that correspond to the known test members.
            var testMethods = typeof(ProblemDetailsFactoryTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m =>
                    m.Name.StartsWith("CreateProblemDetails", StringComparison.Ordinal) ||
                    m.Name.StartsWith("ProblemDetails", StringComparison.Ordinal));

            foreach (var method in testMethods)
            {
                try
                {
                    var returnValue = method.Invoke(tests, Array.Empty<object>());

                    // Await any asynchronous test method.
                    if (returnValue is Task task)
                    {
                        task.GetAwaiter().GetResult();
                    }

                    results[method.Name] = true;
                }
                catch
                {
                    results[method.Name] = false;
                }
            }

            return results;
        }

        /// <summary>
        /// Returns the exact list of public test method names defined on <see cref="ProblemDetailsFactoryTests"/>.
        /// The list is based on the current public members and is returned as an immutable read‑only list.
        /// </summary>
        /// <param name="tests">The <see cref="ProblemDetailsFactoryTests"/> instance (only used for null checking).</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing the names of all known test methods.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetProblemDetailsTestNames(this ProblemDetailsFactoryTests tests) =>
            new[]
            {
                nameof(ProblemDetailsFactoryTests.CreateProblemDetails_ShouldCreateValidProblemDetails),
                nameof(ProblemDetailsFactoryTests.CreateProblemDetails_ShouldSetDefaultTypeToAboutBlank),
                nameof(ProblemDetailsFactoryTests.CreateProblemDetails_ShouldIncludeTraceIdFromActivity),
                nameof(ProblemDetailsFactoryTests.CreateProblemDetails_ShouldIncludeTraceIdFromHttpContext),
                nameof(ProblemDetailsFactoryTests.CreateProblemDetails_ShouldIncludeErrorCodeFromServiceScaffoldException),
                nameof(ProblemDetailsFactoryTests.CreateProblemDetails_ShouldIncludeCustomExtensions),
                nameof(ProblemDetailsFactoryTests.ProblemDetails_ShouldSerializeToJsonWithCamelCase),
                nameof(ProblemDetailsFactoryTests.ProblemDetails_ShouldIncludeAllRequiredRfc7807Fields),
                nameof(ProblemDetailsFactoryTests.ProblemDetails_ShouldHaveCorrectContentType)
            };
    }
}
