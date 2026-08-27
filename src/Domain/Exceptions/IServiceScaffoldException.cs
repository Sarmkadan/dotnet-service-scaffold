#nullable enable

namespace DotnetServiceScaffold.Domain.Exceptions;

/// <summary>
/// Interface for service scaffold exceptions.
/// </summary>
public interface IServiceScaffoldException
{
    string? ErrorCode { get; set; }
}