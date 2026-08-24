using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetServiceScaffold.Infrastructure.Formatting;

namespace DotnetServiceScaffold.Tests
{
    public static class CsvResponseFormatterTestsValidation
    {
        public static IReadOnlyList<string> Validate(CsvResponseFormatterTests value)
        {
            var problems = new List<string>();
            if (value == null)
            {
                problems.Add("Value cannot be null");
            }
            else
            {
                // Add more validation logic here
            }
            return problems;
        }

        public static bool IsValid(CsvResponseFormatterTests value)
        {
            return Validate(value).Count == 0;
        }

        public static void EnsureValid(CsvResponseFormatterTests value)
        {
            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException("The following problems were found: " + string.Join(" ", problems));
            }
        }
    }
}