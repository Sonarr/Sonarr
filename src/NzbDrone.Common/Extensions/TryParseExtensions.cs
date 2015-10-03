using System;

namespace NzbDrone.Common.Extensions
{
    public static class TryParseExtensions
    {
        public static Nullable<int> ParseInt32(this string source)
        {
            int result = 0;

            if (int.TryParse(source, out result))
            {
                return result;
            }

            return null;
        }

        public static Nullable<long> ParseInt64(this string source)
        {
            long result = 0;

            if (long.TryParse(source, out result))
            {
                return result;
            }

            return null;
        }
    }
}