using System;

namespace NzbDrone.Common.Extensions
{
    public static class Int64Extensions
    {
        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string SizeSuffix(this Int64 value)
        {
            const int bytesInKb = 1000;

            if (value < 0) return "-" + SizeSuffix(-value);
            if (value == 0) return "0 bytes";
            
            var mag = (int)Math.Log(value, bytesInKb);
            var adjustedSize = value / (decimal)Math.Pow(bytesInKb, mag);

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}
