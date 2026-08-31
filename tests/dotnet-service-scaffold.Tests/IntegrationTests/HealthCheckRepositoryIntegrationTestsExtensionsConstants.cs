#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Tests.IntegrationTests;

/// <summary>
/// Constants for <see cref="HealthCheckRepositoryIntegrationTestsExtensions"/>.
/// </summary>
internal static class HealthCheckRepositoryIntegrationTestsExtensionsConstants
{
	public const int DefaultResponseTimeMs = 150;
	public const string DefaultDetails = "Test health check";
	public const int MinimumResultCount = 1;
	public const int MinimumRandomHealthStatusValue = 0;
	public const int MaximumRandomHealthStatusValueExclusive = 3;
	public const int MinimumRandomResponseTimeMs = 50;
	public const int MaximumRandomResponseTimeMsExclusive = 500;
	public const string IndexedHealthCheckDetailsFormat = "Health check #{0}";
}
