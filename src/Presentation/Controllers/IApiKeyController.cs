#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotnetServiceScaffold.Presentation.Controllers;

/// <summary>
/// Interface for API key controller operations.
/// </summary>
public interface IApiKeyController
{
    Task<IActionResult> GetAuthInfo(CancellationToken cancellationToken = default);
    Task<IActionResult> RotateApiKey(Guid id, CancellationToken cancellationToken = default);
}