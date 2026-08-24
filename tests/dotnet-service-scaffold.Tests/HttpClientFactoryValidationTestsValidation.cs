using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace DotnetServiceScaffold.Tests
{
    /// <summary>
    /// Provides structural validation helpers for <see cref="HttpClientFactoryValidationTests"/> instances,
    /// checking that every declared test method satisfies the constraints required for discovery and execution.
    /// </summary>
    public static class HttpClientFactoryValidationTestsValidation
    {
        /// <summary>
        /// Validates every test method declared on <paramref name="value"/>'s type hierarchy and returns the problems found.
        /// </summary>
        /// <param name="value">The test-class instance to validate.</param>
        /// <returns>A read-only list of human-readable problems; empty when the instance is valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this HttpClientFactoryValidationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            List<string> problems = new();

            for (Type? type = value.GetType();
                 type is not null && typeof(HttpClientFactoryValidationTests).IsAssignableFrom(type);
                 type = type.BaseType)
            {
                CollectProblems(type, problems);
            }

            return problems;
        }

        /// <summary>
        /// Determines whether every test method declared on <paramref name="value"/>'s type hierarchy is structurally valid.
        /// </summary>
        /// <param name="value">The test-class instance to validate.</param>
        /// <returns><see langword="true"/> when no problems were found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this HttpClientFactoryValidationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures every test method declared on <paramref name="value"/>'s type hierarchy is structurally valid.
        /// </summary>
        /// <param name="value">The test-class instance to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">At least one structural problem was found; the message lists every problem.</exception>
        public static void EnsureValid(this HttpClientFactoryValidationTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            IReadOnlyList<string> problems = value.Validate();
            if (problems.Count == 0)
            {
                return;
            }

            string details = string.Join(Environment.NewLine, problems.Select(static problem => string.Concat(" - ", problem)));
            throw new ArgumentException(
                FormattableString.Invariant($"HttpClientFactoryValidationTests instance is invalid ({problems.Count} problem(s)):{Environment.NewLine}{details}"),
                nameof(value));
        }

        /// <summary>
        /// Appends the structural problems of all public instance methods declared directly on <paramref name="type"/>.
        /// </summary>
        private static void CollectProblems(Type type, List<string> problems)
        {
            foreach (MethodInfo method in type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .OrderBy(static candidate => candidate.Name, StringComparer.Ordinal))
            {
                if (method.IsSpecialName)
                {
                    continue;
                }

                string? problem = DescribeProblem(method);
                if (problem is not null)
                {
                    problems.Add(problem);
                }
            }
        }

        /// <summary>
        /// Returns a human-readable description of the first structural defect of <paramref name="method"/>,
        /// or <see langword="null"/> when the method satisfies all test-runner constraints.
        /// </summary>
        private static string? DescribeProblem(MethodInfo method)
        {
            if (method.IsAbstract)
            {
                return FormattableString.Invariant($"Test method '{method.Name}' is abstract and can never be executed by a test runner.");
            }

            if (method.IsGenericMethodDefinition)
            {
                return FormattableString.Invariant($"Test method '{method.Name}' is a generic method definition and cannot be discovered or executed by a test runner.");
            }

            if (!IsSupportedReturnType(method.ReturnType))
            {
                return FormattableString.Invariant($"Test method '{method.Name}' returns '{method.ReturnType}', which is not a supported test return type (void, Task, ValueTask or ValueTask<T>).");
            }

            int parameterCount = method.GetParameters().Length;
            if (parameterCount > 0 && !HasDataSource(method))
            {
                return FormattableString.Invariant($"Test method '{method.Name}' declares {parameterCount} parameter(s) but carries no data-supplying attribute (InlineData, MemberData or ClassData), so the runner cannot provide arguments.");
            }

            return null;
        }

        /// <summary>
        /// Determines whether <paramref name="returnType"/> is a return type a test runner can execute.
        /// </summary>
        private static bool IsSupportedReturnType(Type returnType) =>
            returnType == typeof(void)
            || returnType == typeof(Task)
            || returnType == typeof(ValueTask)
            || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>));

        /// <summary>
        /// Determines whether <paramref name="method"/> carries a data-supplying attribute such as InlineData, MemberData or ClassData.
        /// </summary>
        private static bool HasDataSource(MethodInfo method) =>
            method.IsDefined(typeof(DataAttribute), inherit: true);
    }
}
