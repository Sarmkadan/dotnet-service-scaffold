#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Running;
using DotnetServiceScaffold.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(StringBenchmarks).Assembly).Run(args);
