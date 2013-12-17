using System;

namespace NzbDrone.Common
{
    public static class TryParseExtension
    {
        public static Nullable<int> ParseInt32(this string source)
        {
            Int32 result = 0;

            if (Int32.TryParse(source, out result))
            {
                return result;
            }

            return null;
        }

        public static Nullable<long> ParseInt64(this string source)
        {
            Int64 result = 0;

            if (Int64.TryParse(source, out result))
            {
                return result;
            }

            return null;
        }
    }
}