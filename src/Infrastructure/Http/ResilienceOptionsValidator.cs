#nullable enable

using Microsoft.Extensions.Options;

namespace DotnetServiceScaffold.Infrastructure.Http;

/// <summary>
/// Validator for <see cref="ResilienceOptions"/>.
/// </summary>
public class ResilienceOptionsValidator : IValidateOptions<ResilienceOptions>
{
    /// <summary>
    /// Validates the resilience options, ensuring the backoff and circuit-breaker
    /// configuration form a coherent policy.
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> describing the outcome.</returns>
    public ValidateOptionsResult Validate(string? name, ResilienceOptions options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("ResilienceOptions cannot be null.");

        if (options.RetryCount < 0 || options.RetryCount > 10)
            return ValidateOptionsResult.Fail($"RetryCount must be between 0 and 10. Provided: {options.RetryCount}");

        if (options.BaseDelayMilliseconds < 1)
            return ValidateOptionsResult.Fail($"BaseDelayMilliseconds must be greater than 0. Provided: {options.BaseDelayMilliseconds}");

        if (options.MaxDelayMilliseconds < options.BaseDelayMilliseconds)
            return ValidateOptionsResult.Fail("MaxDelayMilliseconds must be greater than or equal to BaseDelayMilliseconds.");

        if (options.PerAttemptTimeoutSeconds < 1 || options.PerAttemptTimeoutSeconds > 300)
            return ValidateOptionsResult.Fail($"PerAttemptTimeoutSeconds must be between 1 and 300. Provided: {options.PerAttemptTimeoutSeconds}");

        if (options.CircuitBreakerFailureThreshold < 1)
            return ValidateOptionsResult.Fail($"CircuitBreakerFailureThreshold must be greater than 0. Provided: {options.CircuitBreakerFailureThreshold}");

        if (options.CircuitBreakerBreakDurationSeconds < 1)
            return ValidateOptionsResult.Fail($"CircuitBreakerBreakDurationSeconds must be greater than 0. Provided: {options.CircuitBreakerBreakDurationSeconds}");

        return ValidateOptionsResult.Success;
    }
}
