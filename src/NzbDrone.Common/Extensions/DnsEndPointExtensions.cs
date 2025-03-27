using System.Net;

namespace NzbDrone.Common.Extensions
{
    public static class DnsEndPointExtensions
    {
        public static string HostPort(this DnsEndPoint endPoint) => $"{endPoint.Host}:{endPoint.Port}";
    }
}
