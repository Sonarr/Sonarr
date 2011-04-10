using System;

namespace NzbDrone.Core.Helpers
{
    public static class ServerHelper
    {
        public static string GetServerHostname()
        {
            //Both these seem to return the same result... Is on better than the other?
            return Environment.MachineName;
            //return Dns.GetHostName();
        }
    }
}