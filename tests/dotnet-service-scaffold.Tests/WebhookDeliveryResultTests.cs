#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Integration;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Tests for the <see cref="WebhookDeliveryResult"/> and <see cref="WebhookAttemptRecord"/> record types.
/// </summary>
public class WebhookDeliveryResultTests
{
    [Fact]
    public void Success_DeliveredTrue_StatusCodePropagated_AttemptCountMatches_ErrorMessageNull_NotCancelled()
    {
        /// <summary>
        /// Verifies that calling <see cref="WebhookDeliveryResult.Success(int, IReadOnlyList{WebhookAttemptRecord})"/>
        /// returns a result with Delivered true, the provided status code, attempt count matching the list size,
        /// null error message, and cancelled false.
        /// </summary>
        var attempts = new List<WebhookAttemptRecord>
        {
            new(1, 200, 100, null, DateTime.UtcNow),
            new(2, 200, 150, null, DateTime.UtcNow.AddSeconds(1))
        };

        var result = WebhookDeliveryResult.Success(201, attempts);

        result.Delivered.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.AttemptCount.Should().Be(2);
        result.ErrorMessage.Should().BeNull();
        result.Cancelled.Should().BeFalse();
        result.Attempts.Should().BeEquivalentTo(attempts);
    }

    [Fact]
    public void Failure_WithStatusCodeAndErrorMessage_DeliveredFalse_StatusCodePropagated_AttemptCountMatches_ErrorMessageSet_NotCancelledByDefault()
    {
        /// <summary>
        /// Verifies that calling <see cref="WebhookDeliveryResult.Failure(int?, string, IReadOnlyList{WebhookAttemptRecord}, bool)"/>
        /// with cancelled false (default) returns a result with Delivered false, the provided status code,
        /// attempt count matching the list size, the provided error message, and cancelled false.
        /// </summary>
        var attempts = new List<WebhookAttemptRecord>
        {
            new(1, 500, 200, "Internal server error", DateTime.UtcNow),
            new(2, 500, 250, "Internal server error", DateTime.UtcNow.AddSeconds(1))
        };

        var result = WebhookDeliveryResult.Failure(500, "Internal server error", attempts);

        result.Delivered.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.AttemptCount.Should().Be(2);
        result.ErrorMessage.Should().Be("Internal server error");
        result.Cancelled.Should().BeFalse();
        result.Attempts.Should().BeEquivalentTo(attempts);
    }

    [Fact]
    public void Failure_WithCancelledTrue_DeliveredFalse_StatusCodePropagated_AttemptCountMatches_ErrorMessageSet_CancelledTrue()
    {
        /// <summary>
        /// Verifies that calling <see cref="WebhookDeliveryResult.Failure(int?, string, IReadOnlyList{WebhookAttemptRecord}, bool)"/>
        /// with cancelled true returns a result with Delivered false, the provided status code,
        /// attempt count matching the list size, the provided error message, and cancelled true.
        /// </summary>
        var attempts = new List<WebhookAttemptRecord>
        {
            new(1, null, 500, "request timed out", DateTime.UtcNow)
        };

        var result = WebhookDeliveryResult.Failure(null, "request timed out", attempts, cancelled: true);

        result.Delivered.Should().BeFalse();
        result.StatusCode.Should().BeNull();
        result.AttemptCount.Should().Be(1);
        result.ErrorMessage.Should().Be("request timed out");
        result.Cancelled.Should().BeTrue();
        result.Attempts.Should().BeEquivalentTo(attempts);
    }

    [Fact]
    public void Success_WithEmptyAttemptsList_DeliveredTrue_StatusCodePropagated_AttemptCountZero_ErrorMessageNull_NotCancelled()
    {
        /// <summary>
        /// Verifies that Success works correctly with an empty attempts list.
        /// </summary>
        var result = WebhookDeliveryResult.Success(200, Array.Empty<WebhookAttemptRecord>());

        result.Delivered.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.AttemptCount.Should().Be(0);
        result.ErrorMessage.Should().BeNull();
        result.Cancelled.Should().BeFalse();
        result.Attempts.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithEmptyAttemptsList_DeliveredFalse_StatusCodePropagated_AttemptCountZero_ErrorMessageSet_NotCancelled()
    {
        /// <summary>
        /// Verifies that Failure works correctly with an empty attempts list.
        /// </summary>
        var result = WebhookDeliveryResult.Failure(400, "Bad request", Array.Empty<WebhookAttemptRecord>());

        result.Delivered.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.AttemptCount.Should().Be(0);
        result.ErrorMessage.Should().Be("Bad request");
        result.Cancelled.Should().BeFalse();
        result.Attempts.Should().BeEmpty();
    }

    [Fact]
    public void WebhookAttemptRecord_ValueEquality_SameValuesAreEqual_DifferentValuesAreNotEqual()
    {
        /// <summary>
        /// Verifies that WebhookAttemptRecord value equality works as expected.
        /// </summary>
        var timestamp = DateTime.UtcNow;
        var record1 = new WebhookAttemptRecord(1, 200, 100, null, timestamp);
        var record2 = new WebhookAttemptRecord(1, 200, 100, null, timestamp);
        var record3 = new WebhookAttemptRecord(2, 200, 100, null, timestamp);

        (record1 == record2).Should().BeTrue();
        record1.Equals(record2).Should().BeTrue();
        (record1 != record3).Should().BeTrue();
        record1.Equals(record3).Should().BeFalse();
    }

    [Fact]
    public void WebhookDeliveryResult_ValueEquality_SameValuesAreEqual_DifferentValuesAreNotEqual()
    {
        /// <summary>
        /// Verifies that WebhookDeliveryResult value equality works as expected.
        /// </summary>
        var attempts = new List<WebhookAttemptRecord>
        {
            new(1, 200, 100, null, DateTime.UtcNow)
        };
        var result1 = new WebhookDeliveryResult(true, 200, 1, null, attempts, false);
        var result2 = new WebhookDeliveryResult(true, 200, 1, null, attempts, false);
        var result3 = new WebhookDeliveryResult(false, 200, 1, null, attempts, false);

        (result1 == result2).Should().BeTrue();
        result1.Equals(result2).Should().BeTrue();
        (result1 != result3).Should().BeTrue();
        result1.Equals(result3).Should().BeFalse();
    }
}