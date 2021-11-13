using System.Net;

namespace NzbDrone.Common.Http
{
    public class BasicNetworkCredential : NetworkCredential
    {
        public BasicNetworkCredential(string user, string pass)
        : base(user, pass)
        {
        }
    }
}
