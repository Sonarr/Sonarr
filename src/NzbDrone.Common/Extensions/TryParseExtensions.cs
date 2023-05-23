using System.Globalization;

namespace NzbDrone.Common.Extensions
{
    public static class TryParseExtensions
    {
        public static int? ParseInt32(this string source)
        {
            if (int.TryParse(source, out var result))
            {
                return result;
            }

            return null;
        }

        public static long? ParseInt64(this string source)
        {
            if (long.TryParse(source, out var result))
            {
                return result;
            }

            return null;
        }

        public static double? ParseDouble(this string source)
        {
            if (double.TryParse(source.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
