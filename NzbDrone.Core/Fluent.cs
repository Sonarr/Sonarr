using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NzbDrone.Core
{
    public static class Fluent
    {
        public static string WithDefault(this string actual, object defaultValue)
        {
            if (defaultValue == null)
                throw new ArgumentNullException("defaultValue");
            if (String.IsNullOrWhiteSpace(actual))
            {
                return defaultValue.ToString();
            }

            return actual;
        }

        public static Int64 Megabytes(this int megabytes)
        {
            return megabytes * 1048576L;
        }

        public static Int64 Gigabytes(this int gigabytes)
        {
            return gigabytes * 1073741824L;
        }

        public static string ToBestDateString(this DateTime dateTime)
        {
            if (dateTime == DateTime.Today.AddDays(-1))
                return "Yesterday";

            if (dateTime == DateTime.Today)
                return "Today";

            if (dateTime == DateTime.Today.AddDays(1))
                return "Tomorrow";

            if (dateTime > DateTime.Today.AddDays(1) && dateTime < DateTime.Today.AddDays(7))
                return dateTime.DayOfWeek.ToString();

            return dateTime.ToShortDateString();
        }

        public static string ParentUriString(this Uri uri)
        {
            return uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - String.Join("", uri.Segments).Length - uri.Query.Length);
        }
    }
}
