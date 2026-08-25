#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotnetServiceScaffold.Shared.Utilities;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Interface for string manipulation benchmarks.
/// </summary>
public interface IStringBenchmarks
{
    string ToSnakeCase();
    string ToSnakeCasePascal();
    string ToCamelCase();
    string MaskSensitive();
    string GenerateRandomString32();
    string GenerateRandomString64();
    string ToSlug();
    string Truncate();
}