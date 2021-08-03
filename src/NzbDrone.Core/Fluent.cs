using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Core
{
    public static class Fluent
    {
        public static string WithDefault(this string actual, object defaultValue)
        {
            Ensure.That(defaultValue, () => defaultValue).IsNotNull();

            if (string.IsNullOrWhiteSpace(actual))
            {
                return defaultValue.ToString();
            }

            return actual;
        }

        public static long Megabytes(this int megabytes)
        {
            return Convert.ToInt64(megabytes * 1024L * 1024L);
        }

        public static long Gigabytes(this int gigabytes)
        {
            return Convert.ToInt64(gigabytes * 1024L * 1024L * 1024L);
        }

        public static long Megabytes(this double megabytes)
        {
            return Convert.ToInt64(megabytes * 1024L * 1024L);
        }

        public static long Gigabytes(this double gigabytes)
        {
            return Convert.ToInt64(gigabytes * 1024L * 1024L * 1024L);
        }

        public static long Round(this long number, long level)
        {
            return Convert.ToInt64(Math.Floor((decimal)number / level) * level);
        }

        public static string ToBestDateString(this DateTime dateTime)
        {
            if (dateTime == DateTime.Today.AddDays(-1))
            {
                return "Yesterday";
            }

            if (dateTime == DateTime.Today)
            {
                return "Today";
            }

            if (dateTime == DateTime.Today.AddDays(1))
            {
                return "Tomorrow";
            }

            if (dateTime > DateTime.Today.AddDays(1) && dateTime < DateTime.Today.AddDays(7))
            {
                return dateTime.DayOfWeek.ToString();
            }

            return dateTime.ToShortDateString();
        }

        public static int MaxOrDefault(this IEnumerable<int> ints)
        {
            if (ints == null)
            {
                return 0;
            }

            var intList = ints.ToList();

            if (!intList.Any())
            {
                return 0;
            }

            return intList.Max();
        }

        public static int GetByteCount(this string input)
        {
            return Encoding.UTF8.GetByteCount(input);
        }

        public static string Truncate(this string s, int maxLength)
        {
            if (Encoding.UTF8.GetByteCount(s) <= maxLength)
            {
                return s;
            }

            var cs = s.ToCharArray();
            int length = 0;
            int i = 0;
            while (i < cs.Length)
            {
                int charSize = 1;
                if (i < (cs.Length - 1) && char.IsSurrogate(cs[i]))
                {
                    charSize = 2;
                }

                int byteSize = Encoding.UTF8.GetByteCount(cs, i, charSize);
                if ((byteSize + length) <= maxLength)
                {
                    i = i + charSize;
                    length += byteSize;
                }
                else
                {
                    break;
                }
            }

            return s.Substring(0, i);
        }

        public static int MinOrDefault(this IEnumerable<int> ints)
        {
            if (ints == null)
            {
                return 0;
            }

            var intsList = ints.ToList();

            if (!intsList.Any())
            {
                return 0;
            }

            return intsList.Min();
        }
    }
}
