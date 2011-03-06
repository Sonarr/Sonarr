using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

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
