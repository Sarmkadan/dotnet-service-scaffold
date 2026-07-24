#nullable enable

namespace DotnetServiceScaffold.Infrastructure.Http;

/// <summary>
/// Thrown when a request is rejected because the circuit breaker for the target
/// HTTP client is currently open.
/// </summary>
public sealed class BrokenCircuitException : HttpRequestException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrokenCircuitException"/> class.
    /// </summary>
    /// <param name="message">A message describing why the circuit is open.</param>
    public BrokenCircuitException(string message) : base(message)
    {
    }
}
