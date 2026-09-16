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
    /// <param name="birthDate">The date of birth from which to calculate the age.</param>
    /// <param name="referenceDate">The date against which to calculate the age, or <see langword="null"/> to use the current UTC date and time.</param>
    /// <returns>The number of full years between <paramref name="birthDate"/> and the reference date.</returns>
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
    /// <param name="dateTime">The date and time to express relative to the reference date.</param>
    /// <param name="referenceDate">The date and time used as the point of reference, or <see langword="null"/> to use the current UTC date and time.</param>
    /// <returns>A relative time string, or <see langword="null"/> when the absolute difference is at least one year.</returns>
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
    /// <param name="dateTime">The date and time to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="dateTime"/> is on a weekday from 9:00 inclusive to 17:00 exclusive; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="dateTime">The date and time whose day boundary is requested.</param>
    /// <returns>A date and time representing the start of the day containing <paramref name="dateTime"/>.</returns>
    public static DateTime GetStartOfDay(DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59) for a given datetime.
    /// </summary>
    /// <param name="dateTime">The date and time whose day boundary is requested.</param>
    /// <returns>The final tick of the day containing <paramref name="dateTime"/>.</returns>
    public static DateTime GetEndOfDay(DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-DateTimeUtilityConstants.EndOfDayTicksSubtract);
    }

    /// <summary>
    /// Gets the start of the week (Monday) for a given datetime.
    /// </summary>
    /// <param name="dateTime">The date and time whose week boundary is requested.</param>
    /// <returns>The start of the Monday that begins the week containing <paramref name="dateTime"/>.</returns>
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
    /// <param name="dateTime">The date and time whose month boundary is requested.</param>
    /// <returns>The first day of the month containing <paramref name="dateTime"/> at midnight.</returns>
    public static DateTime GetStartOfMonth(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Returns true if the datetime is in the past.
    /// </summary>
    /// <param name="dateTime">The date and time to compare with the current UTC date and time.</param>
    /// <returns><see langword="true"/> if <paramref name="dateTime"/> precedes the current UTC date and time; otherwise, <see langword="false"/>.</returns>
    public static bool IsPast(DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Returns true if the datetime is in the future.
    /// </summary>
    /// <param name="dateTime">The date and time to compare with the current UTC date and time.</param>
    /// <returns><see langword="true"/> if <paramref name="dateTime"/> follows the current UTC date and time; otherwise, <see langword="false"/>.</returns>
    public static bool IsFuture(DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Returns true if the datetime is today.
    /// </summary>
    /// <param name="dateTime">The date and time whose date component is compared with the current UTC date.</param>
    /// <returns><see langword="true"/> if <paramref name="dateTime"/> has the same date as the current UTC date; otherwise, <see langword="false"/>.</returns>
    public static bool IsToday(DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Parses an ISO 8601 duration string and returns a TimeSpan.
    /// Example: "P3DT4H5M6S" = 3 days, 4 hours, 5 minutes, 6 seconds
    /// </summary>
    /// <param name="duration">The ISO 8601 duration string to parse.</param>
    /// <returns>The time interval represented by <paramref name="duration"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="duration"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="duration"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="duration"/> is not a valid ISO 8601 duration.</exception>
    public static TimeSpan ParseIsoDuration(string duration)
    {
        ArgumentException.ThrowIfNullOrEmpty(duration);
        return System.Xml.XmlConvert.ToTimeSpan(duration);
    }
}
