#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Contract for the API key management endpoints.
/// </summary>
public interface IApiKeyController
{
    /// <summary>Gets information about the current API authentication state.</summary>
    Task<IActionResult> GetAuthInfo(CancellationToken cancellationToken = default);

    /// <summary>Lists the calling user's active API keys.</summary>
    Task<IActionResult> ListApiKeys(CancellationToken cancellationToken = default);

    /// <summary>Issues a new API key.</summary>
    Task<IActionResult> CreateApiKey(CreateApiKeyRequest request, CancellationToken cancellationToken = default);

    /// <summary>Revokes an API key.</summary>
    Task<IActionResult> RevokeApiKey(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Rotates an API key.</summary>
    Task<IActionResult> RotateApiKey(Guid id, CancellationToken cancellationToken = default);
}
