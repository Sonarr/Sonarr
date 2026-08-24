using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Configuration
{
    public static class AllowedHostsParser
    {
        private static readonly char[] Separators = { ',', ';' };

        public static List<string> Parse(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return new List<string>();
            }

            return value.Split(Separators)
                        .Select(h => Normalize(h.Trim()))
                        .Where(h => h.IsNotNullOrWhiteSpace())
                        .Distinct()
                        .ToList();
        }

        public static bool IsValidHost(string host)
        {
            if (host.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (host.StartsWith("*."))
            {
                return Uri.CheckHostName(host.Substring(2)) == UriHostNameType.Dns;
            }

            return Uri.CheckHostName(host) != UriHostNameType.Unknown;
        }

        private static string Normalize(string host)
        {
            if (!IsBracketed(host) && host.Contains(':') && host.IsValidIpAddress())
            {
                return $"[{host}]";
            }

            return host;
        }

        private static bool IsBracketed(string host)
        {
            return host.StartsWith("[") && host.EndsWith("]");
        }
    }
}
