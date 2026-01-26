using System.Net;

namespace NzbDrone.Common.Extensions
{
    public static class DnsEndPointExtensions
    {
        extension(DnsEndPoint endPoint)
        {
            public string HostPort => $"{endPoint.Host}:{endPoint.Port}";
        }
    }
}
