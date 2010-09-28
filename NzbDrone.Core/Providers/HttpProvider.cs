using System.Net;

namespace NzbDrone.Core.Providers
{
    internal class HttpProvider : IHttpProvider
    {
        public string DownloadString(string request)
        {
            return new WebClient().DownloadString(request);
        }
    }
}