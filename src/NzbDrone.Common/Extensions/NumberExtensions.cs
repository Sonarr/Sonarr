using System;
using System.Globalization;

namespace NzbDrone.Common.Extensions
{
    public static class NumberExtensions
    {
        private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string SizeSuffix(this long bytes)
        {
            const int bytesInKb = 1024;

            if (bytes < 0)
            {
                return "-" + SizeSuffix(-bytes);
            }

            if (bytes == 0)
            {
                return "0 B";
            }

            var mag = (int)Math.Log(bytes, bytesInKb);
            var adjustedSize = bytes / (decimal)Math.Pow(bytesInKb, mag);

            return string.Format(CultureInfo.InvariantCulture, "{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
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
    }
}
