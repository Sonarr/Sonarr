using System.Net;
using System.Net.Sockets;

namespace NzbDrone.Common.Extensions
{
    public static class IPAddressExtensions
    {
        public static bool IsLocalAddress(this IPAddress ipAddress)
        {
            // Map back to IPv4 if mapped to IPv6, for example "::ffff:1.2.3.4" to "1.2.3.4".
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ipAddress = ipAddress.MapToIPv4();
            }

            // Checks loopback ranges for both IPv4 and IPv6.
            if (IPAddress.IsLoopback(ipAddress))
            {
                return true;
            }

            // IPv4
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                return IsLocalIPv4(ipAddress.GetAddressBytes());
            }

            // IPv6
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return ipAddress.IsIPv6LinkLocal ||
                        ipAddress.IsIPv6UniqueLocal ||
                        ipAddress.IsIPv6SiteLocal;
            }

            return false;
        }

        private static bool IsLocalIPv4(byte[] ipv4Bytes)
        {
            // Link local (no IP assigned by DHCP): 169.254.0.0 to 169.254.255.255 (169.254.0.0/16)
            var isLinkLocal = ipv4Bytes[0] == 169 && ipv4Bytes[1] == 254;

            // Class A private range: 10.0.0.0 – 10.255.255.255 (10.0.0.0/8)
            var isClassA = ipv4Bytes[0] == 10;

            // Class B private range: 172.16.0.0 – 172.31.255.255 (172.16.0.0/12)
            var isClassB = ipv4Bytes[0] == 172 && ipv4Bytes[1] >= 16 && ipv4Bytes[1] <= 31;

            // Class C private range: 192.168.0.0 – 192.168.255.255 (192.168.0.0/16)
            var isClassC = ipv4Bytes[0] == 192 && ipv4Bytes[1] == 168;

            return isLinkLocal || isClassA || isClassC || isClassB;
        }

        public static bool IsCgnatIpAddress(this IPAddress ipAddress)
        {
            var bytes = ipAddress.GetAddressBytes();
            return bytes.Length == 4 && bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127;
        }
    }
}
