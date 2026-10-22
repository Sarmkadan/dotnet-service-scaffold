#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;
using Serilog.Events;

namespace DotnetServiceScaffold.Infrastructure.Logging;

/// <summary>
/// Validates <see cref="StructuredLoggingOptions"/> at startup. <c>MinimumLevel</c> is checked
/// against the Serilog <see cref="LogEventLevel"/> names, because that is the parser Program.cs uses.
/// </summary>
public sealed class StructuredLoggingOptionsValidator : IValidateOptions<StructuredLoggingOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, StructuredLoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.MinimumLevel))
        {
            // Already reported by [Required]; avoid a duplicate message.
            return ValidateOptionsResult.Success;
        }

        return Enum.TryParse<LogEventLevel>(options.MinimumLevel, ignoreCase: true, out _)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(
                $"MinimumLevel must be one of {string.Join(", ", Enum.GetNames<LogEventLevel>())}, but was '{options.MinimumLevel}'.");
    }
}
