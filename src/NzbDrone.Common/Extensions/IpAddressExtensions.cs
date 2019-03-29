using System.Net;

namespace NzbDrone.Common.Extensions
{
    public static class IPAddressExtensions
    {
        public static bool IsLocalAddress(this IPAddress ipAddress)
        {
            if (ipAddress.ToString() == "::1")
            {
                return true;
            }

            byte[] bytes = ipAddress.GetAddressBytes();
            switch (bytes[0])
            {
                case 10:
                case 127:
                    return true;
                case 172:
                    return bytes[1] < 32 && bytes[1] >= 16;
                case 192:
                    return bytes[1] == 168;
                default:
                    return false;
            }
        }
    }
}
