#nullable enable

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Constants for DateTimeUtility.
/// </summary>
internal static class DateTimeUtilityConstants
{
    /// <summary>
    /// Number of seconds in a minute.
    /// </summary>
    public const int SecondsInMinute = 60;

    /// <summary>
    /// Number of seconds in an hour.
    /// </summary>
    public const int SecondsInHour = 3600;

    /// <summary>
    /// Number of seconds in a day.
    /// </summary>
    public const int SecondsInDay = 86400;

    /// <summary>
    /// Number of seconds in a week.
    /// </summary>
    public const int SecondsInWeek = 604800;

    /// <summary>
    /// Approximate number of seconds in a month (30 days).
    /// </summary>
    public const int SecondsInMonth = 2592000;

    /// <summary>
    /// Approximate number of seconds in a year (365 days).
    /// </summary>
    public const int SecondsInYear = 31536000;

    /// <summary>
    /// Suffix for seconds in relative time formatting.
    /// </summary>
    public const string SecondsSuffix = " seconds ";

    /// <summary>
    /// Suffix for minutes in relative time formatting.
    /// </summary>
    public const string MinutesSuffix = " minutes ";

    /// <summary>
    /// Suffix for hours in relative time formatting.
    /// </summary>
    public const string HoursSuffix = " hours ";

    /// <summary>
    /// Suffix for days in relative time formatting.
    /// </summary>
    public const string DaysSuffix = " days ";

    /// <summary>
    /// Suffix for weeks in relative time formatting.
    /// </summary>
    public const string WeeksSuffix = " weeks ";

    /// <summary>
    /// Suffix for months in relative time formatting.
    /// </summary>
    public const string MonthsSuffix = " months ";

    /// <summary>
    /// Suffix for future relative time ("from now").
    /// </summary>
    public const string FromNowSuffix = " from now";

    /// <summary>
    /// Suffix for past relative time ("ago").
    /// </summary>
    public const string AgoSuffix = " ago";

    /// <summary>
    /// Start of business hour (inclusive, 24-hour format).
    /// </summary>
    public const int BusinessHourStart = 9;

    /// <summary>
    /// End of business hour (exclusive, 24-hour format).
    /// </summary>
    public const int BusinessHourEnd = 17;

    /// <summary>
    /// Number of ticks to subtract to get the end of the day.
    /// </summary>
    public const int EndOfDayTicksSubtract = 1;
}