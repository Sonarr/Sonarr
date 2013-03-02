using System;
using System.Linq;

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
    }
}