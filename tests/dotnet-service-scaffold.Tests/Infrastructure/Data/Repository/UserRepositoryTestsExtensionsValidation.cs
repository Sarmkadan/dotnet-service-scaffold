using DotnetServiceScaffold.Domain.Models;
using DotnetServiceScaffold.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace DotnetServiceScaffold.Infrastructure.Data.Repository
{
    public static class UserRepositoryTestsExtensionsValidation
    {
        public static IReadOnlyList<string> Validate(this UserRepositoryTestsExtensions value)
        {
            var problems = new List<string>();
            if (value == null)
            {
                problems.Add("Value is null");
            }
            else
            {
                // Add validation logic here
            }
            return problems;
        }

        public static bool IsValid(this UserRepositoryTestsExtensions value)
        {
            return Validate(value).Count == 0;
        }

        public static void EnsureValid(this UserRepositoryTestsExtensions value)
        {
            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException("Invalid value: " + string.Join("; ", problems));
            }
        }
    }
}