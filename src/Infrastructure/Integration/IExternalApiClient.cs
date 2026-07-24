#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

// This interface is deprecated and will be removed in a future release.
// Use IExternalReadClient and IExternalWriteClient interfaces instead.
// This composite interface is provided for backward compatibility during the transition period.

namespace DotnetServiceScaffold.Infrastructure.Integration;

/// <summary>
/// Deprecated composite interface for external API clients.
/// This interface combines read and write operations and violates the Interface Segregation Principle.
/// Use <see cref="IExternalReadClient"/> and <see cref="IExternalWriteClient"/> interfaces instead.
/// </summary>
/// <remarks>
/// This interface will be removed in a future release. Existing consumers should migrate to:
/// - IExternalReadClient for GET operations
/// - IExternalWriteClient for POST/PUT/DELETE operations
/// </remarks>
[Obsolete("This interface is deprecated. Use IExternalReadClient and IExternalWriteClient instead.")]
public interface IExternalApiClient : IExternalReadClient, IExternalWriteClient
{
    // Composite interface that inherits from both segregated interfaces
    // Provided for backward compatibility during transition period
}