// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotnetServiceScaffold.Shared.Utilities;

namespace DotnetServiceScaffold.Benchmarks;

/// <summary>
/// Benchmarks for string manipulation operations that run on every request.
/// Covers slug generation, case conversion, masking, and random token generation.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class StringBenchmarks
{
    private const string CamelCaseInput = "userAccountServiceManager";
    private const string PascalCaseInput = "UserAccountServiceManager";
    private const string SnakeCaseInput = "user_account_service_manager";
    private const string ApiKey = "sk_live_abc123xyz789deadbeef";
    private const string SlugSource = "My Service Name - Production v2";

    [Benchmark(Description = "ToSnakeCase (camelCase → snake_case)")]
    public string ToSnakeCase() => StringUtility.ToSnakeCase(CamelCaseInput);

    [Benchmark(Description = "ToSnakeCase (PascalCase → snake_case)")]
    public string ToSnakeCasePascal() => StringUtility.ToSnakeCase(PascalCaseInput);

    [Benchmark(Description = "ToCamelCase (snake_case → camelCase)")]
    public string ToCamelCase() => StringUtility.ToCamelCase(SnakeCaseInput);

    [Benchmark(Description = "MaskSensitive (API key, 4 visible chars)")]
    public string MaskSensitive() => StringUtility.MaskSensitive(ApiKey, 4);

    [Benchmark(Description = "GenerateRandomString (length=32)")]
    public string GenerateRandomString32() => StringUtility.GenerateRandomString(32);

    [Benchmark(Description = "GenerateRandomString (length=64)")]
    public string GenerateRandomString64() => StringUtility.GenerateRandomString(64);

    [Benchmark(Description = "ToSlug (human-readable → URL slug)")]
    public string ToSlug() => StringUtility.ToSlug(SlugSource);

    [Benchmark(Description = "Truncate (string at 20 chars with ellipsis)")]
    public string Truncate() => StringUtility.Truncate(SlugSource, 20);
}
