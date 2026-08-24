using System;
using System.Collections.Generic;
using System.Text.Json;
using DotnetServiceScaffold.Infrastructure.Logging;
using Xunit;
using System.Globalization;

namespace DotnetServiceScaffold.Tests.Logging
{
    public class LogContextServiceJsonExtensionsTestsValidation
    {
        public IReadOnlyList<string> Validate(LogContextServiceJsonExtensionsTests value)
        {
            var errors = new List<string>();
            if (value == null)
            {
                errors.Add("Value is null");
            }
            else
            {
                // Add validation logic here
            }
            return errors;
        }

        public bool IsValid(LogContextServiceJsonExtensionsTests value)
        {
            return Validate(value).Count == 0;
        }

        public void EnsureValid(LogContextServiceJsonExtensionsTests value)
        {
            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join("\n", errors));
            }
        }
    }
}
