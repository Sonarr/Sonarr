using System;
using System.Net;

namespace NzbDrone.Core.Providers
{
    class HttpProvider : IHttpProvider
    {
        public string DownloadString(string request)
        {
            return new WebClient().DownloadString(request);
        }
    }
}
