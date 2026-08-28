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
    Task<IActionResult> GetAuthInfo();
    Task<IActionResult> RotateApiKey(Guid id);
}