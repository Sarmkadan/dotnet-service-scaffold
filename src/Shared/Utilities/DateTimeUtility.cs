#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Utility class for date/time operations. Provides helpers for common datetime
/// manipulations like age calculation, relative time formatting, and timezone handling.
/// All dates are assumed UTC internally for consistency.
/// </summary>
public static class DateTimeUtility
{
    /// <summary>
    /// Calculates the age in years between two dates. Useful for user age validation.
    /// </summary>
    public static int CalculateAge(DateTime birthDate, DateTime? referenceDate = null)
    {
        referenceDate ??= DateTime.UtcNow;

        var age = referenceDate.Value.Year - birthDate.Year;

        // Adjust if birthday hasn't occurred this year yet
        if (birthDate.Date > referenceDate.Value.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    /// <summary>
    /// Formats a datetime as relative time (e.g., "2 hours ago", "in 3 days").
    /// Returns null if the absolute difference is beyond 1 year.
    /// </summary>
    public static string? GetRelativeTime(DateTime dateTime, DateTime? referenceDate = null)
    {
        referenceDate ??= DateTime.UtcNow;

        var difference = dateTime - referenceDate.Value;
        var absDifference = Math.Abs(difference.TotalSeconds);

        // Format based on time difference
        return absDifference switch
        {
            < DateTimeUtilityConstants.SecondsInMinute => $"{(int)absDifference}{DateTimeUtilityConstants.SecondsSuffix}{(difference.TotalSeconds > 0 ? DateTimeUtilityConstants.FromNowSuffix : DateTimeUtilityConstants.AgoSuffix)}",
            < DateTimeUtilityConstants.SecondsInHour => $"{(int)(absDifference / DateTimeUtilityConstants.SecondsInMinute)}{DateTimeUtilityConstants.MinutesSuffix}{(difference.TotalSeconds > 0 ? DateTimeUtilityConstants.FromNowSuffix : DateTimeUtilityConstants.AgoSuffix)}",
            < DateTimeUtilityConstants.SecondsInDay => $"{(int)(absDifference / DateTimeUtilityConstants.SecondsInHour)}{DateTimeUtilityConstants.HoursSuffix}{(difference.TotalSeconds > 0 ? DateTimeUtilityConstants.FromNowSuffix : DateTimeUtilityConstants.AgoSuffix)}",
            < DateTimeUtilityConstants.SecondsInWeek => $"{(int)(absDifference / DateTimeUtilityConstants.SecondsInDay)}{DateTimeUtilityConstants.DaysSuffix}{(difference.TotalSeconds > 0 ? DateTimeUtilityConstants.FromNowSuffix : DateTimeUtilityConstants.AgoSuffix)}",
            < DateTimeUtilityConstants.SecondsInMonth => $"{(int)(absDifference / DateTimeUtilityConstants.SecondsInWeek)}{DateTimeUtilityConstants.WeeksSuffix}{(difference.TotalSeconds > 0 ? DateTimeUtilityConstants.FromNowSuffix : DateTimeUtilityConstants.AgoSuffix)}",
            < DateTimeUtilityConstants.SecondsInYear => $"{(int)(absDifference / DateTimeUtilityConstants.SecondsInMonth)}{DateTimeUtilityConstants.MonthsSuffix}{(difference.TotalSeconds > 0 ? DateTimeUtilityConstants.FromNowSuffix : DateTimeUtilityConstants.AgoSuffix)}",
            _ => null
        };
    }

    /// <summary>
    /// Returns whether a datetime is within business hours (Monday-Friday, 9am-5pm UTC).
    /// </summary>
    public static bool IsBusinessHours(DateTime dateTime)
    {
        var dayOfWeek = dateTime.DayOfWeek;
        var hour = dateTime.Hour;

        return dayOfWeek >= DayOfWeek.Monday &&
               dayOfWeek <= DayOfWeek.Friday &&
               hour >= DateTimeUtilityConstants.BusinessHourStart &&
               hour < DateTimeUtilityConstants.BusinessHourEnd;
    }

    /// <summary>
    /// Gets the start of the day (00:00:00) for a given datetime.
    /// </summary>
    public static DateTime GetStartOfDay(DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59) for a given datetime.
    /// </summary>
    public static DateTime GetEndOfDay(DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-DateTimeUtilityConstants.EndOfDayTicksSubtract);
    }

    /// <summary>
    /// Gets the start of the week (Monday) for a given datetime.
    /// </summary>
    public static DateTime GetStartOfWeek(DateTime dateTime)
    {
        var daysFromMonday = dateTime.DayOfWeek - DayOfWeek.Monday;
        if (daysFromMonday < 0)
            daysFromMonday += 7;

        return dateTime.AddDays(-daysFromMonday).Date;
    }

    /// <summary>
    /// Gets the start of the month for a given datetime.
    /// </summary>
    public static DateTime GetStartOfMonth(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Returns true if the datetime is in the past.
    /// </summary>
    public static bool IsPast(DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Returns true if the datetime is in the future.
    /// </summary>
    public static bool IsFuture(DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Returns true if the datetime is today.
    /// </summary>
    public static bool IsToday(DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Parses an ISO 8601 duration string and returns a TimeSpan.
    /// Example: "P3DT4H5M6S" = 3 days, 4 hours, 5 minutes, 6 seconds
    /// </summary>
    public static TimeSpan ParseIsoDuration(string duration)
    {
        return System.Xml.XmlConvert.ToTimeSpan(duration);
    }
}
