using System.Net;
using System.Net.Sockets;

namespace NzbDrone.Common.Extensions
{
    public static class IPAddressExtensions
    {
        public static bool IsLocalAddress(this IPAddress ipAddress)
        {
            if (ipAddress.IsIPv6LinkLocal)
            {
                return true;
            }

            if (IPAddress.IsLoopback(ipAddress))
            {
                return true;
            }

            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
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

            return false;
        }
    }
}
