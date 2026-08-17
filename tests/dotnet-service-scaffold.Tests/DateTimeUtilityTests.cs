#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests;

/// <summary>
/// Contains tests for the DateTimeUtility class.
/// </summary>
public class DateTimeUtilityTests
{
    [Theory]
    [InlineData("2000-01-01", "2020-01-01", 20)]
    [InlineData("2000-01-02", "2020-01-01", 19)]
    public void CalculateAge_VariousDates_ReturnsCorrectAge(string birthDateStr, string referenceDateStr, int expectedAge)
    {
        /// <summary>
        /// Verifies that the CalculateAge method correctly calculates age in years.
        /// </summary>
        var birthDate = DateTime.Parse(birthDateStr);
        var referenceDate = DateTime.Parse(referenceDateStr);

        var age = DateTimeUtility.CalculateAge(birthDate, referenceDate);

        age.Should().Be(expectedAge);
    }

    [Theory]
    [InlineData("2020-01-01 10:00:10", "2020-01-01 10:00:00", "10 seconds from now")]
    [InlineData("2020-01-01 10:00:00", "2020-01-01 10:00:10", "10 seconds ago")]
    public void GetRelativeTime_VariousInputs_ReturnsCorrectString(string dateTimeStr, string referenceDateStr, string expected)
    {
        /// <summary>
        /// Verifies that the GetRelativeTime method correctly formats relative time strings.
        /// </summary>
        var dateTime = DateTime.Parse(dateTimeStr);
        var referenceDate = DateTime.Parse(referenceDateStr);

        var result = DateTimeUtility.GetRelativeTime(dateTime, referenceDate);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("2026-08-16 10:00:00", false)] // Sunday
    [InlineData("2026-08-17 10:00:00", true)]  // Monday 10am
    [InlineData("2026-08-17 08:00:00", false)] // Monday 8am
    [InlineData("2026-08-17 18:00:00", false)] // Monday 6pm
    public void IsBusinessHours_VariousTimes_ReturnsCorrectResult(string dateTimeStr, bool expected)
    {
        /// <summary>
        /// Verifies that the IsBusinessHours method correctly identifies business hours (Mon-Fri, 9am-5pm UTC).
        /// </summary>
        var dateTime = DateTime.Parse(dateTimeStr);

        var result = DateTimeUtility.IsBusinessHours(dateTime);

        result.Should().Be(expected);
    }

    [Fact]
    public void DayBounds_ReturnCorrectDateTime()
    {
        /// <summary>
        /// Verifies that GetStartOfDay and GetEndOfDay return correct boundaries.
        /// </summary>
        var date = new DateTime(2026, 8, 17, 10, 30, 0);

        var start = DateTimeUtility.GetStartOfDay(date);
        var end = DateTimeUtility.GetEndOfDay(date);

        start.Should().Be(new DateTime(2026, 8, 17, 0, 0, 0));
        end.Should().Be(new DateTime(2026, 8, 17, 23, 59, 59, 999).AddTicks(9999));
    }

    [Fact]
    public void WeekAndMonthStart_ReturnCorrectDateTime()
    {
        /// <summary>
        /// Verifies that GetStartOfWeek and GetStartOfMonth return correct start times.
        /// </summary>
        var date = new DateTime(2026, 8, 17); // A Monday

        var startOfWeek = DateTimeUtility.GetStartOfWeek(date);
        var startOfMonth = DateTimeUtility.GetStartOfMonth(date);

        startOfWeek.Should().Be(new DateTime(2026, 8, 17));
        startOfMonth.Should().Be(new DateTime(2026, 8, 1));
    }

    [Fact]
    public void TemporalChecks_ReturnCorrectResult()
    {
        /// <summary>
        /// Verifies that IsPast, IsFuture, and IsToday return correct results relative to UtcNow.
        /// </summary>
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var futureDate = DateTime.UtcNow.AddDays(1);
        var today = DateTime.UtcNow;

        DateTimeUtility.IsPast(pastDate).Should().BeTrue();
        DateTimeUtility.IsFuture(futureDate).Should().BeTrue();
        DateTimeUtility.IsToday(today).Should().BeTrue();
    }
}
