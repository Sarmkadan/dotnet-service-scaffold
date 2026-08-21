using System;

namespace Shared.Utilities
{
    public static class DateTimeExtensions
    {
        public static long ToUnixMs(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public static DateTime StartOfDayUtc(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Utc);
        }

        public static bool IsWithin(this DateTime dateTime, TimeSpan timeSpan)
        {
            return Math.Abs((dateTime - DateTime.UtcNow).TotalSeconds) <= timeSpan.TotalSeconds;
        }

        public static TimeSpan Age(this DateTime birthDate)
        {
            return DateTime.UtcNow - birthDate;
        }
    }
}
