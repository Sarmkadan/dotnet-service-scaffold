#nullable enable

using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Validator for <see cref="HttpClientOptions"/>.
/// </summary>
public class HttpClientOptionsValidator : IValidateOptions<HttpClientOptions>
{
    /// <summary>
    /// Validates the options.
    /// </summary>
    public ValidateOptionsResult Validate(string? name, HttpClientOptions options)
    {
        if (options.TimeoutSeconds < 1 || options.TimeoutSeconds > 300)
        {
            return ValidateOptionsResult.Fail($"TimeoutSeconds must be between 1 and 300. Provided: {options.TimeoutSeconds}");
        }

        if (string.IsNullOrWhiteSpace(options.UserAgent))
        {
            return ValidateOptionsResult.Fail("UserAgent cannot be null, empty, or whitespace.");
        }

        return ValidateOptionsResult.Success;
    }
}
