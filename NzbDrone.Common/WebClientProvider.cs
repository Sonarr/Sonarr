using System.Net;

namespace NzbDrone.Common
{
    public class WebClientProvider
    {
        public virtual string DownloadString(string url)
        {
            return new WebClient().DownloadString(url);
        }
    }
}