using System;

namespace NzbDrone.Common.Extensions
{
    public static class Base64Extensions
    {
        public static string ToBase64(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string ToBase64(this long input)
        {
            return BitConverter.GetBytes(input).ToBase64();
        }
    }
}
