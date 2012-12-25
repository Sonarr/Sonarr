using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NzbDrone.Core
{
    public static class Fluent
    {
        private const Decimal ONE_KILOBYTE = 1024M;
        private const Decimal ONE_MEGABYTE = ONE_KILOBYTE * 1024M;
        private const Decimal ONE_GIGABYTE = ONE_MEGABYTE * 1024M;
        private const Decimal ONE_TERABYTE = ONE_GIGABYTE * 1024M;

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

        public static int MaxOrDefault(this IEnumerable<int> ints)
        {
            if (ints == null)
                return 0;

            var intList = ints.ToList();

            if (!intList.Any())
                return 0;

            return intList.Max();
        }

        public static string Truncate(this string s, int maxLength)
        {
            if (Encoding.UTF8.GetByteCount(s) <= maxLength)
                return s;
            var cs = s.ToCharArray();
            int length = 0;
            int i = 0;
            while (i < cs.Length)
            {
                int charSize = 1;
                if (i < (cs.Length - 1) && char.IsSurrogate(cs[i]))
                    charSize = 2;
                int byteSize = Encoding.UTF8.GetByteCount(cs, i, charSize);
                if ((byteSize + length) <= maxLength)
                {
                    i = i + charSize;
                    length += byteSize;
                }
                else
                    break;
            }
            return s.Substring(0, i);
        }

        public static string AddSpacesToEnum(this Enum enumValue)
        {
            var text = enumValue.ToString();

            if (string.IsNullOrWhiteSpace(text))
                return "";
            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static string ToBestFileSize(this long bytes, int precision = 0)
        {
            var ulongBytes = (ulong)bytes;
            return ulongBytes.ToBestFileSize(precision);
        }

        public static string ToBestFileSize(this ulong bytes, int precision = 0)
        {
            if (bytes == 0)
                return "0B";

            decimal size = Convert.ToDecimal(bytes);

            string suffix;

            if (size > ONE_TERABYTE)
            {
                size /= ONE_TERABYTE;
                suffix = "TB";
            }

            else if (size > ONE_GIGABYTE)
            {
                size /= ONE_GIGABYTE;
                suffix = "GB";
            }

            else if (size > ONE_MEGABYTE)
            {
                size /= ONE_MEGABYTE;
                suffix = "MB";
            }

            else if (size > ONE_KILOBYTE)
            {
                size /= ONE_KILOBYTE;
                suffix = "KB";
            }

            else
            {
                suffix = " B";
            }

            return String.Format("{0:N" + precision + "} {1}", size, suffix);
        }

        public static int MinOrDefault(this IEnumerable<int> ints)
        {
            if (ints == null)
                return 0;

            var intsList = ints.ToList();

            if (!intsList.Any())
                return 0;

            return intsList.Min();
        }
    }
}
