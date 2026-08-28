#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Models;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Interface for tests of the <see cref="Result"/> class.
/// </summary>
public interface IResultTests
{
    void Success_NoArguments_ReturnsResultWithIsSuccessTrue();
    void Failure_WithMessageAndCode_SetsAllErrorProperties();
    void Failure_FromException_CapturesMessageAndUsesTypeNameAsCode();
    void Map_OnSuccessResult_TransformsValueToNewType();
    void Map_OnFailureResult_PropagatesErrorWithoutCallingMapper();
}