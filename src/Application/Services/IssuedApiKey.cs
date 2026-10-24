#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Domain.Models;

namespace DotnetServiceScaffold.Application.Services;

/// <summary>
/// A newly issued API key together with its plaintext value, which is available only at issue time.
/// </summary>
/// <param name="ApiKey">The stored key entity.</param>
/// <param name="PlaintextKey">The full key to return to the client once.</param>
public sealed record IssuedApiKey(ApiKey ApiKey, string PlaintextKey);
