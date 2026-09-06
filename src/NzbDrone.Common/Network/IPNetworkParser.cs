using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Network
{
    public static class IPNetworkParser
    {
        public static bool TryParse(string value, out IPAddress address, out int prefixLength)
        {
            address = null;
            prefixLength = 0;

            if (value.IsNullOrWhiteSpace())
            {
                return false;
            }

            var parts = value.Split('/', StringSplitOptions.TrimEntries);

            if (parts.Length > 2)
            {
                return false;
            }

            var addressPart = parts[0];

            if (!IPAddress.TryParse(addressPart, out var parsedAddress))
            {
                return false;
            }

            if (parsedAddress.AddressFamily == AddressFamily.InterNetwork && addressPart.Count(c => c == '.') != 3)
            {
                return false;
            }

            var maxPrefixLength = parsedAddress.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;

            if (parts.Length == 1)
            {
                address = parsedAddress;
                prefixLength = maxPrefixLength;

                return true;
            }

            if (!int.TryParse(parts[1], out var parsedPrefixLength) ||
                parsedPrefixLength < 1 ||
                parsedPrefixLength > maxPrefixLength)
            {
                return false;
            }

            if (HasHostBitsSet(parsedAddress, parsedPrefixLength))
            {
                return false;
            }

            address = parsedAddress;
            prefixLength = parsedPrefixLength;

            return true;
        }

        public static bool IsValidList(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return true;
            }

            foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!TryParse(entry, out _, out _))
                {
                    return false;
                }
            }

            return true;
        }

        public static string NormalizeList(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            return string.Join(", ", value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        private static bool HasHostBitsSet(IPAddress address, int prefixLength)
        {
            var bytes = address.GetAddressBytes();

            for (var i = 0; i < bytes.Length; i++)
            {
                var prefixBitsInByte = prefixLength - (i * 8);

                if (prefixBitsInByte >= 8)
                {
                    continue;
                }

                var hostMask = prefixBitsInByte <= 0 ? 0xFF : 0xFF >> prefixBitsInByte;

                if ((bytes[i] & hostMask) != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
