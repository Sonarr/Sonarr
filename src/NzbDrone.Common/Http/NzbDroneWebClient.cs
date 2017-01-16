using System;
using System.Net;

namespace NzbDrone.Common.Http
{
    public class NzbDroneWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                ((HttpWebRequest)request).KeepAlive = false;
                ((HttpWebRequest)request).ServicePoint.Expect100Continue = false;
            }

            return request;
        }
    }
}
