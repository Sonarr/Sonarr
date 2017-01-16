using System;

namespace NzbDrone.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool InNextDays(this DateTime dateTime, int days)
        {
            return InNext(dateTime, new TimeSpan(days, 0, 0, 0));
        }

        public static bool InLastDays(this DateTime dateTime, int days)
        {
            return InLast(dateTime, new TimeSpan(days, 0, 0, 0));
        }

        public static bool InNext(this DateTime dateTime, TimeSpan timeSpan)
        {
            return dateTime >= DateTime.UtcNow && dateTime <= DateTime.UtcNow.Add(timeSpan);
        }

        public static bool InLast(this DateTime dateTime, TimeSpan timeSpan)
        {
            return dateTime >= DateTime.UtcNow.Add(-timeSpan) && dateTime <= DateTime.UtcNow;
        }

        public static bool Before(this DateTime dateTime, DateTime beforeDateTime)
        {
            return dateTime <= beforeDateTime;
        }

        public static bool After(this DateTime dateTime, DateTime afterDateTime)
        {
            return dateTime >= afterDateTime;
        }

        public static bool Between(this DateTime dateTime, DateTime afterDateTime, DateTime beforeDateTime)
        {
            return dateTime >= afterDateTime && dateTime <= beforeDateTime;
        }
    }
}
